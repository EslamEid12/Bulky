using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.Models;
using BulkyBook.Models.Models.ViewModel;
using BulkyBookWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Areas.Controllers
{
    [Area("Areas")]
    [Authorize(Roles =SD.Role_Admin) ]
    public class OrderController : Controller
	{

		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			return View();
		}

       public IActionResult Details(int orderId)
        {
             orderVM = new() {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeproperties: "ApplicationUser"),
                OrderDetails=_unitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId==orderId, includeproperties: "Product")
            
            };
            return View(orderVM);
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetails()
        {
            var OrderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            OrderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            OrderHeaderFromDb.PhoneNumbeer = orderVM.OrderHeader.PhoneNumbeer;
            OrderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            OrderHeaderFromDb.City = orderVM.OrderHeader.City;
            OrderHeaderFromDb.State = orderVM.OrderHeader.State;
            OrderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                OrderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;

            }
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                OrderHeaderFromDb.Carrier = orderVM.OrderHeader.TrackingNumber;

            }
            _unitOfWork.OrderHeader.Update(OrderHeaderFromDb);
            _unitOfWork.save();
            TempData["Success"] = "Order Details Update Successfuly.";
            return RedirectToAction(nameof(Details),new {orderId= OrderHeaderFromDb.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id,SD.StatusInproccess);
            _unitOfWork.save();
            TempData["Success"] = "Order Details Update Successfuly.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            OrderHeader.Carrier = orderVM.OrderHeader.Carrier;
            OrderHeader.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            OrderHeader.OrderStatus = SD.StatusShipped;
            OrderHeader.ShippingDate = DateTime.Now;
            if(OrderHeader.PaymentStatus == SD.PaymentStatusDelayPayment)
            {
                OrderHeader.PaymentDuDate = DateTime.Now.AddDays(30);
            }
            _unitOfWork.OrderHeader.Update(OrderHeader);
            _unitOfWork.save();
            TempData["Success"] = "Order Shipped Successfuly.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            if (OrderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = OrderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(OrderHeader.Id,SD.StatusCancelled,SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(OrderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.save();
            TempData["Success"] = "Order Cancelled Successfuly.";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });
        }


        [HttpPost]
        [ActionName("Details")]
        public IActionResult Details_PAY_NOW()
        {
            orderVM.OrderHeader = _unitOfWork.OrderHeader
                .Get(u => u.Id == orderVM.OrderHeader.Id, includeproperties: "ApplicationUser");
            orderVM.OrderDetails = _unitOfWork.OrderDetail
                .GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id,includeproperties: "Product");


            var domain = "https://localhost:7082/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"/areas/order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.Id}",
                CancelUrl = domain + $"/areas/order/details?orderId={orderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in orderVM.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }

                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);

            }
            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId, includeproperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayPayment)
            {
                //This Is An Order By Company 
                var service = new Stripe.Checkout.SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "Paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId,orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.save();
                }
            }

            return View(orderHeaderId);
        }

        [HttpGet]
		public IActionResult GetAll(string status)
		{
            IEnumerable<OrderHeader> ObjectOrderHeader;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                ObjectOrderHeader = _unitOfWork.OrderHeader.GetAll(includeproperties: "ApplicationUser").ToList();

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                ObjectOrderHeader = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId,
                    includeproperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    ObjectOrderHeader=ObjectOrderHeader.Where(u=>u.PaymentStatus==SD.PaymentStatusDelayPayment);
                    break;
                case "inprocess":
                    ObjectOrderHeader = ObjectOrderHeader.Where(u => u.OrderStatus == SD.StatusInproccess);
                    break;
                case "completed":
                    ObjectOrderHeader = ObjectOrderHeader.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    ObjectOrderHeader = ObjectOrderHeader.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }


            return Json(new { data = ObjectOrderHeader });
		}
	}
}
