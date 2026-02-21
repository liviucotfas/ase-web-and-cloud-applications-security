namespace MVCStore.Models
{
	public class Category
	{
		public int CategoryID { get; set; }
		public required string Name { get; set; }

		// Navigation property
		public ICollection<Product> Products { get; set; } = new List<Product>();
	}
}
