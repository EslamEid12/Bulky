using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.Models;
using BulkyBookWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _UnitOfWork;
        public HomeController(IUnitOfWork db, ILogger<HomeController> logger)
        {
            _logger = logger;
            _UnitOfWork = db;
        }

        public IActionResult Index()
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //if (claim != null)
            //{
            //    HttpContext.Session.SetInt32(SD.SessionCart,
            //   _UnitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count()); ;
            //}
            List<Product> ObjectProductList = _UnitOfWork.Product.GetAll(includeproperties: "Category").ToList();

            return View(ObjectProductList);

        }

        public IActionResult Details(int productId)
        {
            ShoppingCart Cart = new()
            {
                Product = _UnitOfWork.Product.Get(u => u.Id == productId, includeproperties: "Category"),
                Count = 1,
                ProductId =productId
            };
            return View(Cart);

        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            ShoppingCart CartFromDB = _UnitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId
            &&u.ProductId == shoppingCart.ProductId);
            if (CartFromDB != null)
            {
                CartFromDB.Count += shoppingCart.Count;
                _UnitOfWork.ShoppingCart.Update(CartFromDB);
                _UnitOfWork.save();
            }
            else
            {
                _UnitOfWork.ShoppingCart.Add(shoppingCart);
                _UnitOfWork.save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                _UnitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
               
            }
            TempData["Success"] = "Cart Update Successfully";
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}