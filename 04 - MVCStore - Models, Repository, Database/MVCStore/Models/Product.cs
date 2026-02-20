using System.ComponentModel.DataAnnotations.Schema;

namespace MVCStore.Models
{
	public class Product
	{
		public int ProductID { get; set; }
		public required string Name { get; set; }
		public required string Description { get; set; }
		[Column(TypeName = "decimal(8, 2)")]
		public decimal Price { get; set; }
		public required string Category { get; set; }
	}
}
