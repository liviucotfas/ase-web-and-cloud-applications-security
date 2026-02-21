using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCStore.Controllers;
using MVCStore.Models;
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
				.ReturnsAsync(new List<Product>
				{
						new Product { ProductID = 1, Name = "P1", Price = 100m, CategoryID = 1 },
						new Product { ProductID = 2, Name = "P2", Price = 200m, CategoryID = 1 }
				});

			HomeController controller = new HomeController(mock.Object);

			// Act
			IEnumerable<Product>? result = (await controller.Index(CancellationToken.None) as ViewResult)?.ViewData.Model as IEnumerable<Product>;

			// Assert
			Product[] prodArray = result?.ToArray() ?? Array.Empty<Product>();
			Assert.True(prodArray.Length == 2);
			Assert.Equal("P1", prodArray[0].Name);
			Assert.Equal("P2", prodArray[1].Name);
		}
	}
}