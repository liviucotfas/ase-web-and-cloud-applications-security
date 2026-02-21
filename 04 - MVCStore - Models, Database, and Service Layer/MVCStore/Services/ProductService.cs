using Microsoft.EntityFrameworkCore;
using MVCStore.Data;
using MVCStore.Models;

namespace MVCStore.Services
{
	public class ProductService : IProductService
	{
		private readonly ApplicationDbContext _context;

		public ProductService(ApplicationDbContext context)
		{
			_context = context;
		}

		public Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default)
		{
			return _context.Products
				.Include(p => p.Category)
				.OrderBy(p => p.Name)
				.ToListAsync(ct);
		}

		public Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default)
		{
			return _context.Products
				.Include(p => p.Category)
				.FirstOrDefaultAsync(p => p.ProductID == id, ct);
		}

		public async Task CreateProductAsync(Product product, CancellationToken ct = default)
		{
			// Business validation
			if (product.Price < 0)
			{
				throw new InvalidOperationException("Price cannot be negative.");
			}

			_context.Products.Add(product);
			await _context.SaveChangesAsync(ct);
		}

		public async Task UpdateProductAsync(Product product, CancellationToken ct = default)
		{
			// Business validation
			if (product.Price < 0)
			{
				throw new InvalidOperationException("Price cannot be negative.");
			}

			_context.Products.Update(product);
			await _context.SaveChangesAsync(ct);
		}

		public async Task DeleteProductAsync(int id, CancellationToken ct = default)
		{
			var product = await GetProductByIdAsync(id, ct);
			if (product is not null)
			{
				_context.Products.Remove(product);
				await _context.SaveChangesAsync(ct);
			}
		}
	}
}
