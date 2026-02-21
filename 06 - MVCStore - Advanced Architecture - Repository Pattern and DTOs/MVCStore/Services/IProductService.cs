using MVCStore.Models;

namespace MVCStore.Services
{
	public interface IProductService
	{
		Task<List<Product>> GetAllProductsAsync(CancellationToken ct = default);
		Task<List<Product>> GetProductsPageAsync(int pageNumber, int pageSize, CancellationToken ct = default);
		Task<int> GetProductCountAsync(CancellationToken ct = default);
		Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default);
		Task CreateProductAsync(Product product, CancellationToken ct = default);
		Task UpdateProductAsync(Product product, CancellationToken ct = default);
		Task DeleteProductAsync(int id, CancellationToken ct = default);
	}
}
