using Microsoft.EntityFrameworkCore;
using MVCStore.Data;
using MVCStore.Models;
using MVCStore.Models.DTOs;
using MVCStore.Repositories;

namespace MVCStore.Services
{
	public class ProductService : IProductService
	{
		private readonly IProductRepository _productRepository;

		public ProductService(IProductRepository productRepository)
		{
			_productRepository = productRepository;  // Now depends on repository, not DbContext!
		}

		public async Task<List<ProductListItemDto>> GetAllProductsAsync(CancellationToken ct = default)
		{
			var products = await _productRepository.GetAllAsync(ct);
			return products.Select(p => p.ToListItemDto()).ToList();  // Map to DTO
		}

		public async Task<ProductDetailsDto?> GetProductByIdAsync(int id, CancellationToken ct = default)
		{
			var product = await _productRepository.GetByIdAsync(id, ct);
			return product?.ToDetailsDto();  // Map to DTO
		}

		public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken ct = default)
		{
			ArgumentNullException.ThrowIfNull(dto);

			// Business validation (separate from DTO validation)
			ValidateProductDto(dto.Name, dto.Price);

			var product = dto.ToEntity();  // Convert DTO to entity
			var created = await _productRepository.AddAsync(product, ct);

			return created.ToDto();  // Return DTO
		}

		public async Task UpdateProductAsync(UpdateProductDto dto, CancellationToken ct = default)
		{
			ArgumentNullException.ThrowIfNull(dto);

			// Business validation
			ValidateProductDto(dto.Name, dto.Price);

			var product = await _productRepository.GetByIdAsync(dto.ProductID, ct);
			if (product is null)
			{
				throw new InvalidOperationException($"Product with ID {dto.ProductID} not found.");
			}

			dto.UpdateEntity(product);  // Update existing entity
			await _productRepository.UpdateAsync(product, ct);
		}

		public async Task DeleteProductAsync(int id, CancellationToken ct = default)
		{
			var product = await _productRepository.GetByIdAsync(id, ct);
			if (product is not null)
			{
				await _productRepository.DeleteAsync(product, ct);
			}
		}

		// Private helper for business validation
		private static void ValidateProductDto(string name, decimal price)
		{
			if (price < 0)
			{
				throw new InvalidOperationException("Price cannot be negative.");
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				throw new InvalidOperationException("Product name is required.");
			}

			if (name.Length > 100)
			{
				throw new InvalidOperationException("Product name cannot exceed 100 characters.");
			}
		}
	}
}
