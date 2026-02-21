using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVCStore.Data;
using MVCStore.Models;

namespace MVCStore.Repositories
{
	public class ProductRepository : IProductRepository
	{
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
        {
            return _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id, ct);
        }

        public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync(ct);
            return product;
        }

        public async Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(ct);
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            return _context.Products.AnyAsync(p => p.ProductID == id, ct);
        }
    }
}