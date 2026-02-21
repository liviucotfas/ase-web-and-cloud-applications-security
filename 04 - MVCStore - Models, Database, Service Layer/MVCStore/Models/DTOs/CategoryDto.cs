using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

namespace MVCStore.Models.DTOs
{
	/// <summary>
	/// DTO for displaying category information
	/// </summary>
	public class CategoryDto
	{
		public int CategoryID { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	/// <summary>
	/// DTO for creating a new category
	/// </summary>
	public class CreateCategoryDto
	{
		[Required(ErrorMessage = "Category name is required")]
		[StringLength(50, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 50 characters")]
		public required string Name { get; set; }
	}

	/// <summary>
	/// DTO for updating an existing category
	/// </summary>
	public class UpdateCategoryDto
	{
		[Required]
		public int CategoryID { get; set; }

		[Required(ErrorMessage = "Category name is required")]
		[StringLength(50, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 50 characters")]
		public required string Name { get; set; }
	}

	/// <summary>
	/// DTO for displaying category with product count
	/// </summary>
	public class CategoryWithProductCountDto
	{
		public int CategoryID { get; set; }
		public string Name { get; set; } = string.Empty;
		public int ProductCount { get; set; }
	}

	/// <summary>
	/// DTO for category details including all products
	/// </summary>
	public class CategoryDetailsDto
	{
		public int CategoryID { get; set; }
		public string Name { get; set; } = string.Empty;
		public List<ProductListItemDto> Products { get; set; } = new();
	}
}