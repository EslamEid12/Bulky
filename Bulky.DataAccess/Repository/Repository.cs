using BulkyBookWeb.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess;

namespace BulkyBookWeb.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private DbSet<T> dbSet;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
            _db.Products.Include(u => u.Category).Include(u => u.CategoryId);
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> Fillter, string? includeproperties = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }
                query = query.Where(Fillter);
                if (!string.IsNullOrEmpty(includeproperties))
                {
                    foreach (var includeprop in includeproperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeproperties);
                    }
                }
                return query.FirstOrDefault();
            
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? Fillter, string? includeproperties=null)
        {
            IQueryable<T> query = dbSet;
            if (Fillter != null)
            {
                query = query.Where(Fillter);
            }
            
            if (!string.IsNullOrEmpty(includeproperties))
            {
                foreach(var includeprop in includeproperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeproperties);
                }
            }
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRnge(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
