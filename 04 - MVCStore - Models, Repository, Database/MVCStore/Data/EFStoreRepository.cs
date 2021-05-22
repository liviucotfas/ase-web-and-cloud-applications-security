using MVCStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
