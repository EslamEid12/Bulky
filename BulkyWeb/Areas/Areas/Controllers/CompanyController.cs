using BulkyBookWeb.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using BulkyBookWeb.Utility;
using BulkyBook.Models.Models;
using BulkyBook.DataAccess.Repository;

namespace BulkyBookWeb.Areas.Areas.Controllers
{
    [Area("Areas")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public CompanyController(IUnitOfWork db)
        {
            _UnitOfWork = db;
        }
        public IActionResult Index()
        {
            List<Company> ObjectCompanyList = _UnitOfWork.Company.GetAll().ToList();
            return View(ObjectCompanyList);

        }
        //public IActionResult Create()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public IActionResult Create(Company obj)
        //{
        //    if (obj.Name == obj.DisplayOrder.ToString())
        //    {
        //        ModelState.AddModelError("name", "The DisplayOrder Cannot exactly Match Name. ");
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        _UnitOfWork.Company.Add(obj);
        //        _UnitOfWork.save();
        //        TempData["success"] = "Company Created Succesfully";
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}

        //[HttpGet]
        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Company? CompanyfromDb = _UnitOfWork.Company.Get(u => u.ID == id);
        //    Company? CompanyfromDb1 = _db.Categories.FirstOrDefault(u => u.ID == id);
        //    Company? CompanyfromDb2 = _db.Categories.Where(u => u.ID == id).FirstOrDefault();
        //    if (CompanyfromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(CompanyfromDb);
        //}
        //[HttpPost]
        //public IActionResult Edit(Company obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _UnitOfWork.Company.Update(obj);
        //        _UnitOfWork.save();
        //        TempData["success"] = "Company Update Succesfully";
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}




        [HttpGet]
        public IActionResult UpSert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company CompanyObj = _UnitOfWork.Company.Get(u => u.ID == id);
                //Company? CompanyfromDb1 = _db.Categories.FirstOrDefault(u=>u.ID==id);
                //Company? CompanyfromDb2 = _db.Categories.Where(u=>u.ID==id).FirstOrDefault();

                return View(CompanyObj);
            }

        }
        [HttpPost]
        public IActionResult UpSert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.ID == 0)
                {
                    _UnitOfWork.Company.Add(obj);
                }
                else
                {
                    _UnitOfWork.Company.Update(obj);
                }

                _UnitOfWork.save();
                TempData["success"] = "Product Created Succesfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(obj);
            }
        }

        #region API Call 

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> ObjectCompanyList = _UnitOfWork.Company.GetAll().ToList();
            return Json(new { data = ObjectCompanyList });
        }

        #endregion


        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var CompanyToBeDeleted = _UnitOfWork.Company.Get(u => u.ID == id);
            if (CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            _UnitOfWork.Company.Remove(CompanyToBeDeleted);
            _UnitOfWork.save();
            return Json(new {success = true, message = " Delete Success"});
           
        }
    }

}