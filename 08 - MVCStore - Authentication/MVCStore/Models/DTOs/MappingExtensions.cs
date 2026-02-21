namespace MVCStore.Models.DTOs
{
	public static class MappingExtensions
	{
		// Product → DTOs
		public static ProductDto ToDto(this Product product)
		{
			return new ProductDto
			{
				ProductID = product.ProductID,
				Name = product.Name,
				Price = product.Price,
				CategoryID = product.CategoryID,
				CategoryName = product.Category?.Name
			};
		}

		public static ProductListItemDto ToListItemDto(this Product product)
		{
			return new ProductListItemDto
			{
				ProductID = product.ProductID,
				Name = product.Name,
				Price = product.Price,
				CategoryName = product.Category?.Name ?? "Unknown"
			};
		}

		public static ProductDetailsDto ToDetailsDto(this Product product)
		{
			return new ProductDetailsDto
			{
				ProductID = product.ProductID,
				Name = product.Name,
				Price = product.Price,
				Category = product.Category?.ToDto() ?? new CategoryDto()
			};
		}

		// DTOs → Product
		public static Product ToEntity(this CreateProductDto dto)
		{
			return new Product
			{
				Name = dto.Name,
				Price = dto.Price,
				CategoryID = dto.CategoryID
			};
		}

		public static void UpdateEntity(this UpdateProductDto dto, Product product)
		{
			product.Name = dto.Name;
			product.Price = dto.Price;
			product.CategoryID = dto.CategoryID;
		}

		// Category mappings
		public static CategoryDto ToDto(this Category category)
		{
			return new CategoryDto
			{
				CategoryID = category.CategoryID,
				Name = category.Name
			};
		}

		public static Category ToEntity(this CreateCategoryDto dto)
		{
			return new Category
			{
				Name = dto.Name
			};
		}

		public static void UpdateEntity(this UpdateCategoryDto dto, Category category)
		{
			category.Name = dto.Name;
		}
	}
}
