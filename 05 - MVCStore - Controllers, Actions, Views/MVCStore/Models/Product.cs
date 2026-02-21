using System.ComponentModel.DataAnnotations.Schema;

namespace MVCStore.Models
{
	public class Product
	{
		public int ProductID { get; set; }
		public required string Name { get; set; }

		[Column(TypeName = "decimal(8, 2)")]
		public decimal Price { get; set; }

		public int CategoryID { get; set; }

		// Navigation property
		public Category? Category { get; set; }
	}
}
