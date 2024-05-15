using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.Models.ViewModel
{
	public class OrderVM
	{
		[ValidateNever]
		public IEnumerable<OrderDetail> OrderDetails { get; set; }
		public OrderHeader OrderHeader { get; set; }
	}
}
