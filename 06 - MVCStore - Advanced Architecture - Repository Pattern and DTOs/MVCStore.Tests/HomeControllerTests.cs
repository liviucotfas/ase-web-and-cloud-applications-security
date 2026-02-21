using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCStore.Controllers;
using MVCStore.Models;
using MVCStore.Models.DTOs;
using MVCStore.Services;

namespace MVCStore.Tests
{
	public class HomeControllerTests
	{
		[Fact]
		public async Task Can_Use_Service()
		{
			// Arrange
			Mock<IProductService> mock = new Mock<IProductService>();
			mock.Setup(m => m.GetAllProductsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(new List<ProductListItemDto>
				{
					new ProductListItemDto { ProductID = 1, Name = "P1", Price = 100m, CategoryName = "Category1" },
					new ProductListItemDto { ProductID = 2, Name = "P2", Price = 200m, CategoryName = "Category1" }
				});

			HomeController controller = new HomeController(mock.Object);

			// Act
			IEnumerable<ProductListItemDto>? result = (await controller.Index() as ViewResult)?.ViewData.Model as IEnumerable<ProductListItemDto>;

			// Assert
			ProductListItemDto[] prodArray = result?.ToArray() ?? Array.Empty<ProductListItemDto>();
			Assert.True(prodArray.Length == 2);
			Assert.Equal("P1", prodArray[0].Name);
			Assert.Equal("P2", prodArray[1].Name);
		}
	}
}