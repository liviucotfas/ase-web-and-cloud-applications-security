using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCStore.Controllers;
using MVCStore.Data;
using MVCStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MVCStore.Tests
{
	public class AdminControllerTests
	{
		[Fact]
		public void Index_Contains_All_Products()
		{
			// Arrange - create the mock repository
			Mock<IProductRepository> mock = new Mock<IProductRepository>();
			mock.Setup(m => m.Products).Returns(new Product[] {
				new Product {ProductID = 1, Name = "P1"},
				new Product {ProductID = 2, Name = "P2"},
				new Product {ProductID = 3, Name = "P3"},
			}.AsQueryable<Product>());
			// Arrange - create a controller
			AdminController target = new AdminController(mock.Object);
			// Action
			Product[] result
				= GetViewModel<IEnumerable<Product>>(target.Index())?.ToArray();
			// Assert
			Assert.Equal(3, result.Length);
			Assert.Equal("P1", result[0].Name);
			Assert.Equal("P2", result[1].Name);
			Assert.Equal("P3", result[2].Name);
		}
		private T GetViewModel<T>(IActionResult result) where T : class
		{
			return (result as ViewResult)?.ViewData.Model as T;
		}

		[Fact]
		public void Can_Delete_Valid_Products()
		{
			// Arrange - create a Product
			Product prod = new Product { ProductID = 2, Name = "Test" };
			// Arrange - create the mock repository
			Mock<IProductRepository> mock = new Mock<IProductRepository>();
			mock.Setup(m => m.Products).Returns(new Product[] {
			new Product {ProductID = 1, Name = "P1"},
			prod,
			new Product {ProductID = 3, Name = "P3"},
		}.AsQueryable<Product>());
			// Arrange - create the controller
			AdminController target = new AdminController(mock.Object);
			// Act - delete the product
			target.Delete(prod.ProductID);
			// Assert - ensure that the repository delete method was
			// called with the correct Product
			mock.Verify(m => m.DeleteProduct(prod.ProductID));
		}
	}
}
