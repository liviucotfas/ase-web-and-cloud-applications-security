using MVCStore.Models;

namespace MVCStore.Data
{
    public interface IStoreRepository
	{
		IQueryable<Product> Products { get; }
	}
}
