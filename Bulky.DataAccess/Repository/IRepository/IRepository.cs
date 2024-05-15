using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBookWeb.DataAccess.Repository.IRepository
{
    public interface IRepository<T>where T:class
    {
        IEnumerable<T> GetAll( Expression<Func<T, bool>>? Fillter=null, string? includeproperties = null);
        T Get(Expression<Func<T,bool>>Fillter, string? includeproperties = null,bool tracked=false);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRnge(IEnumerable<T>entity);


    }
}
