using Microsoft.EntityFrameworkCore;
using MVCStore.Data;
using MVCStore.Models;

namespace MVCStore.Repositories
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly ApplicationDbContext _context;

		public CategoryRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public Task<List<Category>> GetAllAsync(CancellationToken ct = default)
			=> _context.Categories.OrderBy(c => c.Name).AsNoTracking().ToListAsync(ct);

		public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
			=> _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == id, ct);
	}
}
