using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCStore.Controllers;
using MVCStore.Models.DTOs;
using MVCStore.Services;
using MVCStore.ViewModels;

namespace MVCStore.Tests
{
	public class HomeControllerTests
	{
		[Fact]
		public async Task Can_Use_Service()
		{
			// Arrange
			Mock<IProductService> mock = new Mock<IProductService>();
			mock.Setup(m => m.GetProductsPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new List<ProductListItemDto>
				{
						new ProductListItemDto { ProductID = 1, Name = "P1", Price = 100m, CategoryName = "Category1" },
						new ProductListItemDto { ProductID = 2, Name = "P2", Price = 200m, CategoryName = "Category1" }
				});
			mock.Setup(m => m.GetProductCountAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(2);

			HomeController controller = new HomeController(mock.Object);

			// Act
			IEnumerable<ProductListItemDto>? result = (await controller.Index() as ViewResult)?.ViewData.Model as IEnumerable<ProductListItemDto>;

			// Assert
			ProductListItemDto[] prodArray = result?.ToArray() ?? Array.Empty<ProductListItemDto>();
			Assert.True(prodArray.Length == 2);
			Assert.Equal("P1", prodArray[0].Name);
			Assert.Equal("P2", prodArray[1].Name);
		}

		[Fact]
		public async Task Can_Paginate()
		{
			// Arrange
			Mock<IProductService> mock = new Mock<IProductService>();

			mock.Setup(m => m.GetProductsPageAsync(2, 3, It.IsAny<CancellationToken>()))
				.ReturnsAsync(new List<ProductListItemDto>
				{
					new ProductListItemDto { ProductID = 4, Name = "P4", Price = 400m, CategoryName = "Category1" },
					new ProductListItemDto { ProductID = 5, Name = "P5", Price = 500m, CategoryName = "Category1" }
				});

			mock.Setup(m => m.GetProductCountAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(15); // Total products

			HomeController controller = new HomeController(mock.Object);
			controller.PageSize = 3;

			// Act
			IEnumerable<ProductListItemDto>? result = (await controller.Index(2) as ViewResult)?.ViewData.Model as IEnumerable<ProductListItemDto>;

			// Assert
			ProductListItemDto[] prodArray = result?.ToArray() ?? Array.Empty<ProductListItemDto>();
			Assert.True(prodArray.Length == 2);
			Assert.Equal("P4", prodArray[0].Name);
			Assert.Equal("P5", prodArray[1].Name);
		}

		[Fact]
		public async Task Can_Send_Pagination_View_Model()
		{
			// Arrange
			Mock<IProductService> mock = new Mock<IProductService>();
			mock.Setup(m => m.GetProductsPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new List<ProductListItemDto>());
			mock.Setup(m => m.GetProductCountAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(28);

			HomeController controller = new HomeController(mock.Object);
			controller.PageSize = 10;

			// Act
			ViewResult result = await controller.Index(2) as ViewResult ?? new ViewResult();
			PagingInfoViewModel? pageInfo = result.ViewData["PagingInfo"] as PagingInfoViewModel;

			// Assert
			Assert.NotNull(pageInfo);
			Assert.Equal(2, pageInfo.CurrentPage);
			Assert.Equal(10, pageInfo.ItemsPerPage);
			Assert.Equal(28, pageInfo.TotalItems);
			Assert.Equal(3, pageInfo.TotalPages);
		}
	}
}