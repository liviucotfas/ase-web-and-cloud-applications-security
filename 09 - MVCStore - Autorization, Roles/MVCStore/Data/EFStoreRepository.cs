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

        public async Task SaveProductAsync(Product product)
        {
            if (product.ProductID == 0)
            {
                context.Products.Add(product);
            }
            else
            {
                Product dbEntry = context.Products
                    .FirstOrDefault(p => p.ProductID == product.ProductID);
                if (dbEntry != null)
                {
                    dbEntry.Name = product.Name;
                    dbEntry.Description = product.Description;
                    dbEntry.Price = product.Price;
                }
            }
            await context.SaveChangesAsync();
        }
        public async Task<Product> DeleteProductAsync(int productID)
        {
            Product dbEntry = context.Products
                    .FirstOrDefault(p => p.ProductID == productID);

            if (dbEntry != null)
            {
                context.Products.Remove(dbEntry);
                await context.SaveChangesAsync();
            }

            return dbEntry;
        }
    }
}
