using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsStore.Models
{
	public class EFProductRepository : IProductRepository
	{
		private ApplicationDbContext context;
		public EFProductRepository(ApplicationDbContext ctx)
		{
			context = ctx;
		}
		public IEnumerable<Product> Products => context.Products;
	}
}
