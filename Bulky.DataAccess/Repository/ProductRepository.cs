using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBookWeb.DataAccess.Repository;
using BulkyBookWeb.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository: Repository<Product>,IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product obj)
        {

            var objfromdb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            if (objfromdb != null) 
            { 
                objfromdb.Title=obj.Title;
                objfromdb.ISBN = obj.ISBN;
                objfromdb.Price = obj.Price;
                objfromdb.ListPrice = obj.ListPrice;
                objfromdb.Price50 = obj.Price50;
                objfromdb.Price100 = obj.Price100;
                objfromdb.Description=obj.Description;  
                objfromdb.CategoryId=obj.CategoryId;
                objfromdb.Author = obj.Author;
                if (obj.ImageUrl != null)
                {
                    objfromdb.ImageUrl = obj.ImageUrl;
                }


            }
        }


    }
}
