using MVCStore.Models;

namespace MVCStore.Repositories
{
	public interface ICategoryRepository
	{
		Task<List<Category>> GetAllAsync(CancellationToken ct = default);
		Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
	}
}
