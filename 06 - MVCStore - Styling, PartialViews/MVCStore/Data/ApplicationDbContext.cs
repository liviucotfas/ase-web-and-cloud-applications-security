using Microsoft.EntityFrameworkCore;
using MVCStore.Models;

namespace MVCStore.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
		public DbSet<Product> Products { get; set; }
	}
}
