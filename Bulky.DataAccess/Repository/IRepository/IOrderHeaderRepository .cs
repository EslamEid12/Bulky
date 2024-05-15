using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBookWeb.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    { 

      public void Update(OrderHeader obj);
		public void UpdateStatus(int id,string orderStatus,string? paymentStatus=null);
		public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
	}
}
