using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using P3Core.Controllers;
using P3Core.Data;
using P3Core.Models.Entities;
using P3Core.Models.ViewModels;
using P3Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace P3Core.Tests
{
    public class ProductIntegrationTestFixture : IDisposable, IClassFixture<ProductIntegrationTestFixture>
    {
        #region ----- INITIALISATION DES CHAMPS ----

        public P3Referential Context { get; private set; }
        public Models.Repositories.ProductRepository ProductService { get; private set; }
        private readonly Models.Services.ProductRepository _productService;
        private readonly ProductController _productController;
        private readonly IStringLocalizer<ProductController> _localizer;

        private ICart _Cart;

        #endregion

        #region ----- VARIABLES DE TESTS ----

        Product produit = new Product
        {
            Name = "Product 1",
            Price = 10.5,
            Quantity = 100,
            Description = "Desc1",
            Details = "Details1"
        };

        Product produit2 = new Product
        {
            Name = "Product 2",
            Price = 12.5,
            Quantity = 200,
            Description = "Desc2",
            Details = "Details2"
        };

        #endregion

        #region ----- "CONSTRUCTEUR" ----

        public ProductIntegrationTestFixture()
        {

            _Cart = new Cart();
            var options = new DbContextOptionsBuilder<P3Referential>()
               .UseInMemoryDatabase($"P3ReferentialMock_{Guid.NewGuid()}")  // Base unique pour chaque test
               .Options;
            var localizerFactory = new ResourceManagerStringLocalizerFactory(
                                   new OptionsWrapper<LocalizationOptions>(new LocalizationOptions()),
                                       NullLoggerFactory.Instance);

            _localizer = new StringLocalizer<ProductController>(localizerFactory);
            var productServiceLocalizer = new StringLocalizer<Models.Services.ProductRepository>(localizerFactory);

            Context = new P3Referential(options, new ConfigurationBuilder().Build());
            ProductService = new Models.Repositories.ProductRepository(Context);
            _productService = new Models.Services.ProductRepository(_Cart, ProductService, null, productServiceLocalizer);
            _productController = new ProductController(_productService, _localizer);
            InitializeTestData().Wait();
        }


        #endregion

        #region ----- PRIVATES METHODS ----

        private async Task InitializeTestData()
        {
            await Context.Product.AddRangeAsync(produit, produit2);
            await Context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        #endregion

        #region ----- TESTS D'INTEGRATION ----

        #region ------ DEPUIS LE SERVICE ------

        #region ----- TI01 - GetAllProduct_ReturnListOfProducts_IntegrationTest -----

        [Fact]
        public void GetAllProduct_ReturnListOfProducts_IntegrationTest()
        {
            var result = _productService.GetAllProducts();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Product 1", result.First().Name);
            Assert.Equal(12.5, result.Last().Price);
            Assert.Equal(100, result.First().Quantity);
            Dispose();
        }

        #endregion

        #region ----- TI02 -  GetProductById_ReturnOneProductFromId_IntegrationTest -----
        [Fact]
        public async Task GetProduct_ReturnOneProductFromId_IntegrationTest()
        {
            var result = await _productService.GetProduct(1);

            Assert.NotNull(result);
            Assert.Equal("Product 1", result.Name);
            Assert.Equal(10.5, result.Price);
            Assert.Equal(100, result.Quantity);
            Assert.Equal("Desc1", result.Description);
            Assert.Equal("Details1", result.Details);
            Assert.Equal(1, result.Id);
            Dispose();

        }
        #endregion

        #region ----- TI03 -  UpdateProductQuantities_ShouldReturnGoodStock_IntegrationTest -----
        /// <summary>
        /// Test d'intégration pour la méthode UpdateProductQuantities
        /// Appel de UpdateProductStocks pour chaque ligne du panier
        /// Utilisation des valeur injecté en Bdd avec les variables statiques produit et produit2
        /// </summary>
        [Fact]
        public void UpdateProductQuantities_CallsUpdateProductStocksForEachCartLine_IntegrationTest()
        {
            _Cart.AddItem(produit, 5);
            _Cart.AddItem(produit2, 5);


            _productService.UpdateProductQuantities();


            Assert.Equal(95, produit.Quantity);
            Assert.Equal(195, produit2.Quantity);
            Assert.Equal("Product 1", produit.Name);
            Assert.Equal("Product 2", produit2.Name);
            Dispose();
        }


        #endregion

        #region ----- TI04 -  SaveProduct_CallsRepositoryWithCorrectEntity_IntegrationTest -----

        [Fact]
        public void SaveProduct_CallsRepositoryWithCorrectEntity_IntegrationTest()
        {
            // Arrange
            var product3 = new ProductViewModel
            {
                Id = 3,
                Name = "Product 3",
                Price = "15,5",
                Stock = "50",
                Description = "Desc3",
                Details = "Details3"
            };
            // Act
            _productService.SaveProduct(product3);
            // Assert
            var result = Context.Product.FirstOrDefault(p => p.Name == "Product 3");
            var products = Context.Product.ToList();
            Assert.Equal(3, products.Count);
            Assert.NotNull(result);
            Assert.Equal("Product 3", result.Name);
            Assert.Equal(15.5, result.Price);
            Assert.Equal(50, result.Quantity);
            Assert.Equal("Desc3", result.Description);
            Assert.Equal("Details3", result.Details);
            Dispose();
        }
        #endregion

        #region ----- TI05 -  DeleteProduct_RemovesProductFromRepository_IntegrationTest -----

        [Fact]
        public void DeleteProduct_RemovesProductFromRepository_IntegrationTest()
        {
            // Arrange
            _Cart.AddItem(produit, 5);

            // Act
            _productService.DeleteProduct(1);
            // Assert
            var result = Context.Product.FirstOrDefault(p => p.Id == 1);
            var products = Context.Product.ToList();
            Assert.Equal(1, products.Count);
            Assert.Null(result);
            Dispose();
        }

        #endregion

        #endregion

        #region ------ DEPUIS LE CONTROLLER ------

        #region ----- TI06 -  CreateProduct_CreatesProductInDatabase_IntegrationTestPass -----

        [Fact]
        public void CreateProduct_CreatesProductInDatabase_IntegrationTestPass()
        {
            // Arrange
            var product = new ProductViewModel
            {
                Name = "Product 3",
                Price = "15,5",
                Stock = "50",
                Description = "Desc3",
                Details = "Details3"
            };
            // Act
            _productController.Create(product);
            // Assert
            var result = Context.Product.FirstOrDefault(p => p.Name == "Product 3");
            var products = Context.Product.ToList();
            Assert.Equal(3, products.Count);
            Assert.NotNull(result);
            Assert.Equal("Product 3", result.Name);
            Assert.Equal(15.5, result.Price);
            Assert.Equal(50, result.Quantity);
            Assert.Equal("Desc3", result.Description);
            Assert.Equal("Details3", result.Details);
            Dispose();
        }


        #endregion

        #region ----- TI07 -  CreateProduct_CreatesProductInDatabase_IntegrationTestNotPass -----

        [Fact]
        public void CreateProduct_CreatesProductInDatabase_IntegrationTestNotPass()
        {
            // Arrange
            var product = new ProductViewModel
            {
                Name = "Product 3",
                Price = "",
                Stock = "50",
                Description = "Desc3",
                Details = "Details3"
            };



            // Act

            _productController.Create(product);

            // Assert
            var result = Context.Product.FirstOrDefault(p => p.Name == "Product 3");
            var products = Context.Product.ToList();
            Assert.Equal(2, products.Count);
            Assert.Null(result);
            Dispose();
        }


        #endregion

        #region ----- TI08 -  DeleteProduct_RemovesProductFromRepository_IntegrationTestPass -----
        [Fact]
        public void DeleteProduct_RemovesProductFromDatabase_IntegrationTestPass()
        {
            
            // Act
            _productController.DeleteProduct(1);
            // Assert
            var result = Context.Product.FirstOrDefault(p => p.Id == 1);
            var products = Context.Product.ToList();
            Assert.Equal(1, products.Count);
            Assert.Null(result);
            Dispose();
        }

        #endregion

        #region ----- TI09 -  Admin_ReturnListOfProducts_IntegrationTest -----

        [Fact]
        public void Admin_ReturnListOfProducts_IntegrationTest()
        {
            // Act
            var result = _productController.Admin() as ViewResult;
            var products = new List<ProductViewModel>();
            foreach (var r in result.Model as IEnumerable<ProductViewModel>)
            {
                products.Add(r);
            }
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, products.Count);
            Dispose();
        }

        #endregion

        #region ----- TI10 -  Index_ReturnListOfProducts_IntegrationTest -----

        [Fact]
        public void Index_ReturnListOfProducts_IntegrationTest()
        {
            // Act
            var result = _productController.Index() as ViewResult;
            var products = new List<ProductViewModel>();
            foreach (var r in result.Model as IEnumerable<ProductViewModel>)
            {
                products.Add(r);
            }
            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, products.Count);
            Dispose();
        }

        #endregion

        #endregion

        #endregion
    }


}

