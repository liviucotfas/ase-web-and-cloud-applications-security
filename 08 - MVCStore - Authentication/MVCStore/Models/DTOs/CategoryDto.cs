using System.ComponentModel.DataAnnotations;

namespace MVCStore.Models.DTOs
{
	public class CategoryDto
	{
		public int CategoryID { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	public class CreateCategoryDto
	{
		[Required(ErrorMessage = "Category name is required")]
		[StringLength(50, MinimumLength = 2)]
		public required string Name { get; set; }
	}

	public class UpdateCategoryDto
	{
		[Required]
		public int CategoryID { get; set; }

		[Required(ErrorMessage = "Category name is required")]
		[StringLength(50, MinimumLength = 2)]
		public required string Name { get; set; }
	}
}
