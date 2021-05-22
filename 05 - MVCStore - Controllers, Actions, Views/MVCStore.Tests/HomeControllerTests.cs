using Microsoft.AspNetCore.Mvc;
using Moq;
using MVCStore.Controllers;
using MVCStore.Data;
using MVCStore.Models;
using MVCStore.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MVCStore.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Can_Send_Pagination_View_Model()
        {
            // Arrange
            Mock<IStoreRepository> mock = new Mock<IStoreRepository>();
            mock.Setup(m => m.Products).Returns((new Product[] {
                new Product {ProductID = 1, Name = "P1"},
                new Product {ProductID = 2, Name = "P2"},
                new Product {ProductID = 3, Name = "P3"},
                new Product {ProductID = 4, Name = "P4"},
                new Product {ProductID = 5, Name = "P5"}
                }).AsQueryable<Product>());
            // Arrange
            HomeController controller =
            new HomeController(mock.Object) { PageSize = 3 };
            // Act
            ProductsListViewModel result =
            (controller.Index(2) as ViewResult).ViewData.Model as ProductsListViewModel;
            // Assert
            PagingInfo pageInfo = result.PagingInfo;
            Assert.Equal(2, pageInfo.CurrentPage);
            Assert.Equal(3, pageInfo.ItemsPerPage);
            Assert.Equal(5, pageInfo.TotalItems);
            Assert.Equal(2, pageInfo.TotalPages);
        }

        [Fact]
        public void Can_Use_Repository()
        {
            // Arrange
            Mock<IStoreRepository> mock = new Mock<IStoreRepository>();

            mock.Setup(m => m.Products).Returns(
                (new Product[] {
                    new Product {ProductID = 1, Name = "P1"},
                    new Product {ProductID = 2, Name = "P2"}
                    }).AsQueryable<Product>()
                 );

            HomeController controller = new HomeController(mock.Object);

            // Act
            // !!!! new/updated code {
            ProductsListViewModel result = (controller.Index() as ViewResult).ViewData.Model as ProductsListViewModel;
            //}

            // Assert
            // !!!! new/updated code {
            Product[] prodArray = result.Products.ToArray();
            //}
            Assert.True(prodArray.Length == 2);
            Assert.Equal("P1", prodArray[0].Name);
            Assert.Equal("P2", prodArray[1].Name);
        }

        [Fact]
        public void Can_Paginate()
        {
            // Arrange
            Mock<IStoreRepository> mock = new Mock<IStoreRepository>();
            mock.Setup(m => m.Products).Returns((new Product[] {
                new Product {ProductID = 1, Name = "P1"},
                new Product {ProductID = 2, Name = "P2"},
                new Product {ProductID = 3, Name = "P3"},
                new Product {ProductID = 4, Name = "P4"},
                new Product {ProductID = 5, Name = "P5"}
                }).AsQueryable<Product>());
            HomeController controller = new HomeController(mock.Object);
            controller.PageSize = 3;

            // Act
            // !!!! new/updated code {
            ProductsListViewModel result = (controller.Index(2) as ViewResult).ViewData.Model as ProductsListViewModel;
            //}

            // Assert
            // !!!! new/updated code {
            Product[] prodArray = result.Products.ToArray();
            //}
            Assert.True(prodArray.Length == 2);
            Assert.Equal("P4", prodArray[0].Name);
            Assert.Equal("P5", prodArray[1].Name);
        }
    }
}
