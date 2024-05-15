using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.Models;
using BulkyBook.Models.Models.ViewModel;
using BulkyBookWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        [BindProperty]
		public ShoppingCartVM shoppingCartVm { get; set; }
		public CartController(IUnitOfWork unitOfWork,IEmailSender emailSender)
		{
			_unitOfWork = unitOfWork;
			_emailSender = emailSender;

        }
		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			shoppingCartVm = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
				includeproperties: "Product"),
				OrderHeader = new()
			};


			foreach (var cart in shoppingCartVm.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				shoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return View(shoppingCartVm);
		}
		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			shoppingCartVm = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
				includeproperties: "Product"),
				OrderHeader = new()
			};



			shoppingCartVm.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

			shoppingCartVm.OrderHeader.Name = shoppingCartVm.OrderHeader.ApplicationUser.Name;
			shoppingCartVm.OrderHeader.PhoneNumbeer = shoppingCartVm.OrderHeader.ApplicationUser.PhoneNumber;
			shoppingCartVm.OrderHeader.State = shoppingCartVm.OrderHeader.ApplicationUser.State;
			shoppingCartVm.OrderHeader.City = shoppingCartVm.OrderHeader.ApplicationUser.City;
			shoppingCartVm.OrderHeader.StreetAddress = shoppingCartVm.OrderHeader.ApplicationUser.StreetAddress;
			shoppingCartVm.OrderHeader.PostalCode = shoppingCartVm.OrderHeader.ApplicationUser.PostalCode;



			foreach (var cart in shoppingCartVm.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				shoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(shoppingCartVm);
		}

		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			shoppingCartVm.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
				includeproperties: "Product");
			shoppingCartVm.OrderHeader.OrderDate = System.DateTime.Now;
			shoppingCartVm.OrderHeader.ApplicationUserId = userId;
			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


			foreach (var cart in shoppingCartVm.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				shoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				//Its a Regular Customer 
				shoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				shoppingCartVm.OrderHeader.OrderStatus = SD.StatusPending;
			}
			else
			{
				//Its a Company User
				shoppingCartVm.OrderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
				shoppingCartVm.OrderHeader.OrderStatus = SD.StatusApproved;
			}
			_unitOfWork.OrderHeader.Add(shoppingCartVm.OrderHeader);
			_unitOfWork.save();
			foreach (var cart in shoppingCartVm.ShoppingCartList)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderHeaderId = shoppingCartVm.OrderHeader.Id,
					Count = cart.Count,
					Price = cart.Price
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.save();
			}
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				//Its a Regular Customer account nd we need to capture payment
				var domain = "https://localhost:7082/";
				var options = new Stripe.Checkout.SessionCreateOptions
				{
					SuccessUrl = domain + $"/customer/cart/OrderConfirmation?id={shoppingCartVm.OrderHeader.Id}",
					CancelUrl= domain + "/customer/cart/index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};
				foreach(var item in shoppingCartVm.ShoppingCartList)
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
				Session session= service.Create(options);
				_unitOfWork.OrderHeader.UpdateStripePaymentId(shoppingCartVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unitOfWork.save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}

			return RedirectToAction(nameof(OrderConfirmation), new { id = shoppingCartVm.OrderHeader.Id });
		}
		public IActionResult OrderConfirmation(int Id)
		{

			OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == Id, includeproperties : "ApplicationUser");
			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayPayment)
			{
				var service = new Stripe.Checkout.SessionService();
				Session session = service.Get(orderHeader.SessionId);
				if (session.PaymentStatus.ToLower() == "Paid") {
					_unitOfWork.OrderHeader.UpdateStripePaymentId(Id, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(Id, SD.StatusApproved, SD.PaymentStatusApproved);
					_unitOfWork.save();
				}
				_emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email,"New Order-Buky Book","<p>New Order Created</p>");
				HttpContext.Session.Clear();
			}
			List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
				.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
			_unitOfWork.ShoppingCart.RemoveRnge(shoppingCarts);
			_unitOfWork.save();

			return View(Id);
		}
		public IActionResult Plus(int cartId)
		{
			var CartFromDB = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
			CartFromDB.Count += 1;
			_unitOfWork.ShoppingCart.Update(CartFromDB);
			_unitOfWork.save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			var CartFromDB = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);
			if (CartFromDB.Count <= 1) { 
				_unitOfWork.ShoppingCart.Remove(CartFromDB);
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
				.GetAll(u => u.ApplicationUserId == CartFromDB.ApplicationUserId).Count() - 1);

            }
            else
			{
				CartFromDB.Count -= 1;
				_unitOfWork.ShoppingCart.Update(CartFromDB);
			}

			_unitOfWork.save();
			return RedirectToAction(nameof(Index));
		}



		public IActionResult Remove(int cartId)
		{
			var CartFromDB = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId,tracked:true);
            _unitOfWork.ShoppingCart.Remove(CartFromDB);
            HttpContext.Session.SetInt32(SD.SessionCart,_unitOfWork.ShoppingCart
		    .GetAll(u => u.ApplicationUserId == CartFromDB.ApplicationUserId).Count()-1);
			_unitOfWork.save();
			return RedirectToAction(nameof(Index));
		}


		private double GetPriceBasedOnQuantity(ShoppingCart shoppingcart)
		{
			if (shoppingcart.Count <= 50) { return shoppingcart.Product.Price; }
			else
			{
				if (shoppingcart.Count <= 100)
				{ return shoppingcart.Product.Price50; }
				else { return shoppingcart.Product.Price100; }
			}
		}
	}
}

