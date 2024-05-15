using BulkyBookWeb.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess;
namespace BulkyBookWeb.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
            var orderfromDB = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderfromDB != null)
            {
                orderfromDB.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderfromDB.PaymentStatus = paymentStatus;
                }
            }
		}

		public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
			var orderfromDB = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
				if (!string.IsNullOrEmpty(sessionId))
				{
					orderfromDB.SessionId = sessionId;
				}
			if (!string.IsNullOrEmpty(paymentIntentId))
			{
				orderfromDB.PaymentIntentId = paymentIntentId;
                orderfromDB.PaymentDate = DateTime.Now;
			}
		}
	}
}
