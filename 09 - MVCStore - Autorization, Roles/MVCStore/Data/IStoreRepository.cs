using MVCStore.Models;

namespace MVCStore.Data
{
    public interface IStoreRepository
	{
		IQueryable<Product> Products { get; }
		Task SaveProductAsync(Product product);
		Task<Product> DeleteProductAsync(int productID);
	}
}
