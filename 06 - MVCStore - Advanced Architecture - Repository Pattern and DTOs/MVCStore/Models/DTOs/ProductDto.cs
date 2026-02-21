using System.ComponentModel.DataAnnotations;

namespace MVCStore.Models.DTOs
{
    /// <summary>
    /// DTO for displaying product details
    /// </summary>
    public class ProductDto
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }  // Flattened from Category.Name
    }

    /// <summary>
    /// DTO for creating a new product
    /// </summary>
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999,999.99")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        [Display(Name = "Category")]
        public int CategoryID { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing product
    /// </summary>
    public class UpdateProductDto
    {
        [Required]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }
    }

    /// <summary>
    /// Lightweight DTO for product listings
    /// </summary>
    public class ProductListItemDto
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for product details view with full category information
    /// </summary>
    public class ProductDetailsDto
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public CategoryDto Category { get; set; } = null!;
    }
}
