namespace MVCStore.Models
{
	public class Category
	{
		public int CategoryID { get; set; }
		public required string Name { get; set; }
		public required ICollection<Product> Products { get; set; }
	}
}
