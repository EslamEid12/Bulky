using BulkyBookWeb.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using BulkyBookWeb.Utility;

namespace BulkyBookWeb.Areas.Areas.Controllers
{
    [Area("Areas")]
    [Authorize(Roles=SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CategoryController(IUnitOfWork db)
        {
            _UnitOfWork = db;
        }
        public IActionResult Index()
        {
            List<Category> ObjectCategoryList = _UnitOfWork.Category.GetAll().ToList();
            return View(ObjectCategoryList);

        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            //if(obj.Name==obj.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("name", "The DisplayOrder Cannot exactly Match Name. ");
            //}
            if (ModelState.IsValid)
            {
                _UnitOfWork.Category.Add(obj);
                _UnitOfWork.save();
                TempData["success"] = "Category Created Succesfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryfromDb = _UnitOfWork.Category.Get(u => u.ID == id);
            //Category? categoryfromDb1 = _db.Categories.FirstOrDefault(u=>u.ID==id);
            //Category? categoryfromDb2 = _db.Categories.Where(u=>u.ID==id).FirstOrDefault();
            if (categoryfromDb == null)
            {
                return NotFound();
            }
            return View(categoryfromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _UnitOfWork.Category.Update(obj);
                _UnitOfWork.save();
                TempData["success"] = "Category Update Succesfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryfromDb = _UnitOfWork.Category.Get(u => u.ID == id);
            //Category? categoryfromDb1 = _db.Categories.FirstOrDefault(u=>u.ID==id);
            //Category? categoryfromDb2 = _db.Categories.Where(u=>u.ID==id).FirstOrDefault();
            if (categoryfromDb == null)
            {
                return NotFound();
            }
            return View(categoryfromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int id)
        {
            Category? obj = _UnitOfWork.Category.Get(u => u.ID == id);
            if (obj == null)
            {
                return NotFound();
            }
            _UnitOfWork.Category.Remove(obj);
            _UnitOfWork.save();
            TempData["success"] = "Category Delete Succesfully";
            return RedirectToAction("Index");
        }
    }
}
