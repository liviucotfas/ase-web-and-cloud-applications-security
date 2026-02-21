using Moq;
using MVCStore.Models.DTOs;
using MVCStore.Repositories;
using MVCStore.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MVCStore.Tests
{
	public class ProductServiceTests
	{
		[Fact]
		public async Task CreateProductAsync_NegativePrice_ThrowsException()
		{
			// Arrange
			var mockRepo = new Mock<IProductRepository>();
			var service = new ProductService(mockRepo.Object);

			var dto = new CreateProductDto
			{
				Name = "Test",
				Price = -10,
				CategoryID = 1
			};

			// Act & Assert
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => service.CreateProductAsync(dto));
		}
	}
}
