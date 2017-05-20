using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCStore.Models
{
	public interface IProductRepository
	{
		IEnumerable<Product> Products { get; }

		void SaveProduct(Product product);
	}
}
