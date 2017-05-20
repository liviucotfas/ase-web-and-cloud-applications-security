using System.Collections.Generic;

namespace MVCStore.Models
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
