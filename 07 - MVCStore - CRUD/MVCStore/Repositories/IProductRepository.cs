using MVCStore.Models;

namespace MVCStore.Repositories
{
	public interface IProductRepository
	{
		Task<List<Product>> GetAllAsync(CancellationToken ct = default);
		Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
		Task<Product> AddAsync(Product product, CancellationToken ct = default);
		Task UpdateAsync(Product product, CancellationToken ct = default);
		Task DeleteAsync(Product product, CancellationToken ct = default);
		Task<bool> ExistsAsync(int id, CancellationToken ct = default);
	}
}
