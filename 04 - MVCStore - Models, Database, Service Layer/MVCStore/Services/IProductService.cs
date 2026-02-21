using MVCStore.Models.DTOs;

using MVCStore.Models.DTOs;

namespace MVCStore.Services
{
	public interface IProductService
	{
		Task<List<ProductListItemDto>> GetAllProductsAsync(CancellationToken ct = default);
		Task<ProductDetailsDto?> GetProductByIdAsync(int id, CancellationToken ct = default);
		Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken ct = default);
		Task UpdateProductAsync(UpdateProductDto dto, CancellationToken ct = default);
		Task DeleteProductAsync(int id, CancellationToken ct = default);
	}
}
