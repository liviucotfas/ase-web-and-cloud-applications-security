using Moq;
using MVCStore.Controllers;
using MVCStore.Data;
using MVCStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MVCStore.Tests
{
    public class AdminControllerTests
    {
        [Fact]
        public void Can_Delete_Valid_Products()
        {
            // Arrange - create a Product
            Product prod = new Product { ProductID = 2, Name = "Test" };
            // Arrange - create the mock repository
            Mock<IStoreRepository> mock = new Mock<IStoreRepository>();
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
            mock.Verify(m => m.DeleteProductAsync(prod.ProductID));
        }
    }
}
