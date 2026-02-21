using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCStore.Controllers;
using MVCStore.Models.DTOs;
using MVCStore.Repositories;
using MVCStore.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MVCStore.Tests
{
	public class AdminControllerTests
	{
		private static AdminController CreateController(
			Mock<IProductService> serviceMock,
			Mock<ICategoryRepository>? repoMock = null)
		{
			repoMock ??= new Mock<ICategoryRepository>();
			return new AdminController(serviceMock.Object, repoMock.Object);
		}

		[Fact]
		public async Task Index_Returns_All_Products()
		{
			// Arrange
			var products = new List<ProductListItemDto>
				{
					new() { ProductID = 1, Name = "P1", CategoryName = "Cat1" },
					new() { ProductID = 2, Name = "P2", CategoryName = "Cat2" }
				};
			var mockService = new Mock<IProductService>();
			mockService.Setup(s => s.GetAllProductsAsync(It.IsAny<CancellationToken>()))
					   .ReturnsAsync(products);

			var controller = CreateController(mockService);

			// Act
			var result = await controller.Index() as ViewResult;

			// Assert
			var model = Assert.IsAssignableFrom<IEnumerable<ProductListItemDto>>(result!.Model);
			Assert.Equal(2, model.Count());
		}

		[Fact]
		public async Task Delete_Calls_Service_And_Sets_TempData()
		{
			// Arrange
			var productDetails = new ProductDetailsDto
			{
				ProductID = 2,
				Name = "Test Product",
				Price = 9.99m,
				Category = new CategoryDto { CategoryID = 1, Name = "Cat" }
			};
			var mockService = new Mock<IProductService>();
			mockService.Setup(s => s.GetProductByIdAsync(2, It.IsAny<CancellationToken>()))
					   .ReturnsAsync(productDetails);
			mockService.Setup(s => s.DeleteProductAsync(2, It.IsAny<CancellationToken>()))
					   .Returns(Task.CompletedTask);

			var controller = CreateController(mockService);

			// Act
			await controller.Delete(2);

			// Assert – service was called and TempData contains the product name
			mockService.Verify(s => s.DeleteProductAsync(2, It.IsAny<CancellationToken>()), Times.Once);
			Assert.Equal("Test Product was deleted.", controller.TempData["message"]);
		}

		[Fact]
		public async Task Delete_NonExistent_Product_Does_Not_Call_DeleteAsync()
		{
			// Arrange
			var mockService = new Mock<IProductService>();
			mockService.Setup(s => s.GetProductByIdAsync(99, It.IsAny<CancellationToken>()))
					   .ReturnsAsync((ProductDetailsDto?)null);

			var controller = CreateController(mockService);

			// Act
			await controller.Delete(99);

			// Assert
			mockService.Verify(s => s.DeleteProductAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
		}
	}
}
