using BulkyBookWeb.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using BulkyBook.Models.Models.ViewModel;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.AspNetCore.Authorization;
using BulkyBookWeb.Utility;

namespace BulkyBookWeb.Areas.Areas.Controllers
{
    [Area("Areas")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db, IWebHostEnvironment webhostEnvironment)
        {
            _UnitOfWork = db;
            _webHostEnvironment = webhostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> ProductList = _UnitOfWork.Product.GetAll().ToList();
            return View(ProductList);

        }
        public IActionResult UpSert(int? id)
        {
            
            ProductVM productVM = new()
            {
                CategoryList = _UnitOfWork.Category.GetAll().
                Select(u => new SelectListItem {
                 Text=u.Name,
                 Value=u.ID.ToString()
                }),
                Product = new Product()
            };
            if (id == 0 || id == null) 
            {
                //Create
                return View(productVM);
            }
            else
            {
                //Update
                productVM.Product=_UnitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
           
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpSert(ProductVM obj,IFormFile? file)
        {

            ///if(obj.Name==obj.DisplayOrder.ToString())
            ///{
            ///    ModelState.AddModelError("name", "The DisplayOrder Cannot exactly Match Name. ");
            ///}
            if (ModelState.IsValid)
            {
                string wwwrootpath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = @"images\Product\" + fileName;
                    string finalPath = Path.Combine(wwwrootpath, productPath);
                    string extFile = Path.GetExtension(fileName);

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        var OldPath = Path.Combine(wwwrootpath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(OldPath))
                        {
                            System.IO.File.Delete(OldPath);
                        }
                    }

                    if (extFile != ".jpg")
                    {
                        ModelState.AddModelError("", "Invalid file format");
                    }

                    using (var fileStream = new FileStream(Path.Combine(finalPath), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl = @"\images\Product\" + fileName;
                }
                if (obj.Product.Id == 0)
                {
                    _UnitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    _UnitOfWork.Product.Update(obj.Product);
                }
               
                _UnitOfWork.save();
                TempData["success"]="Product Created Succesfully";
                return RedirectToAction("Index");
            }
            else
            {
               obj.CategoryList = _UnitOfWork.Category.GetAll().Select(u => new SelectListItem
              {
                  Text = u.Name,
                  Value = u.ID.ToString()
              });
            }
            return View(obj);
        }

        //[HttpGet]
        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product? categoryfromDb = _UnitOfWork.Product.Get(u => u.Id == id);
        //    //Product? categoryfromDb1 = _db.Categories.FirstOrDefault(u=>u.ID==id);
        //    //Product? categoryfromDb2 = _db.Categories.Where(u=>u.ID==id).FirstOrDefault();
        //    if (categoryfromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(categoryfromDb);
        //}
        //[HttpPost]
        //public IActionResult Edit(Product obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _UnitOfWork.Product.Update(obj);
        //        _UnitOfWork.save();
        //        TempData["success"] = "Product Update Succesfully";
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}

        //[HttpGet]
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product? categoryfromDb = _UnitOfWork.Product.Get(u => u.Id==id );
        //    //Product? categoryfromDb1 = _db.Categories.FirstOrDefault(u=>u.ID==id);
        //    //Product? categoryfromDb2 = _db.Categories.Where(u=>u.ID==id).FirstOrDefault();
        //    if (categoryfromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(categoryfromDb);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int id)
        //{
        //    Product? obj = _UnitOfWork.Product.Get(u => u.Id== id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _UnitOfWork.Product.Remove(obj);
        //    _UnitOfWork.save();
        //    TempData["success"] = "Product Delete Succesfully";
        //    return RedirectToAction("Index");
        //}

        #region API Call 

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> ObjectProductList = _UnitOfWork.Product.GetAll(includeproperties: "Category").ToList();
            return Json(new { data = ObjectProductList });
        }

        #endregion
        public IActionResult Delete(int? id)
        {

            var ProductTtoBeDeleted = _UnitOfWork.Product.Get(u => u.Id == id);
            if (ProductTtoBeDeleted==null)
            {
                return Json(new { succss = false, Message = "Error While Deleting" });
            }
            var OldPath = Path.Combine(_webHostEnvironment.WebRootPath, ProductTtoBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(OldPath))
            {
                System.IO.File.Delete(OldPath);
            }
            _UnitOfWork.Product.Remove(ProductTtoBeDeleted);
            _UnitOfWork.save();
                return Json(new { succss = false, Message = " Delete Succesfull" });
        }
    
    }
}
