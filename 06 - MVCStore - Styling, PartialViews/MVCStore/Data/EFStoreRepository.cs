using MVCStore.Models;

namespace MVCStore.Data
{
    public class EFStoreRepository : IStoreRepository
    {
        private ApplicationDbContext context;

        public EFStoreRepository(ApplicationDbContext ctx)
        {
            context = ctx;
        }

        public IQueryable<Product> Products
        {
            get
            {
                return context.Products;
            }
        }
    }
}
