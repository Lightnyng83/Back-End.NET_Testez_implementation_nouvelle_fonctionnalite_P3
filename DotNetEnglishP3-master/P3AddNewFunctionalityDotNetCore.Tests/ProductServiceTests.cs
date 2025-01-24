using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using P3Core.Controllers;
using P3Core.Data;
using P3Core.Models;
using P3Core.Models.Entities;
using P3Core.Models.Repositories;
using P3Core.Models.Services;
using P3Core.Models.ViewModels;
using P3Core.Resources.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace P3Core.Tests
{
    public class ProductServiceTests
    {
        #region ----- INITIALISATION DES CHAMPS -----

        private readonly Mock<ICart> _mockCart;
        private ICart _Cart;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IStringLocalizer<Models.Services.ProductRepository>> _mockLocalizer;
        private readonly Models.Services.ProductRepository _productService;

        #endregion----- INITIALISATION DES CHAMPS -----

        #region ----- CONSTRUCTEUR -----
        public ProductServiceTests()
        {
            _mockCart = new Mock<ICart>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockLocalizer = new Mock<IStringLocalizer<Models.Services.ProductRepository>>();

            _productService = new Models.Services.ProductRepository(
                _mockCart.Object,
                _mockProductRepository.Object,
                _mockOrderRepository.Object,
                _mockLocalizer.Object
            );

        }

        #endregion

        #region ----- VARIABLES DE TESTS -----

        private static readonly Product produit1 = new Product
        {
            Id = 1,
            Name = "Product 1",
            Price = 10.5,
            Quantity = 100,
            Description = "Desc1",
            Details = "Details1"
        };

        private static readonly Product produit2 = new Product
        {
            Id = 2,
            Name = "Product 2",
            Price = 12.5,
            Quantity = 200,
            Description = "Desc2",
            Details = "Details2"
        };

        private static readonly ProductViewModel productViewModel = new ProductViewModel
        {
            Name = "Product 1",
            Details = "Details",
            Description = "Description",
            Price = "10.5",
            Stock = "20"
        };
        private static readonly ProductViewModel productViewModel2 = new ProductViewModel
        {
            Name = "Product 1",
            Details = "Details",
            Description = "Description",
            Price = "10,5",
            Stock = "20"
        };

        #endregion

        #region ----- TESTS UNITAIRES -----

        #region ----- TU01 - GetAllProductsViewModel_ReturnsMappedViewModels -----

        [Fact]
        public void GetAllProductsViewModel_ReturnsMappedViewModels()
        {
            // Arrange
            var products = new List<Product>
            {
                produit1,produit2
            };

            _mockProductRepository.Setup(repo => repo.GetAllProducts()).Returns(products);

            // Act
            var result = _productService.GetAllProductsViewModel();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Product 1", result[0].Name);
            Assert.Equal("12.5", result[1].Price);
            Assert.Equal("200", result[1].Stock);

        }

        #endregion

        #region ----- TU02 -  GetAllProduct_ReturnListOfProducts -----

        [Fact]
        public void GetAllProduct_ReturnListOfProducts()
        {
            var product = new List<Product>
            {
                produit1,
                produit2
            };

            _mockProductRepository.Setup(repo => repo.GetAllProducts()).Returns(product);

            var result = _productService.GetAllProducts();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Product 1", result[0].Name);
            Assert.Equal(12.5, result[1].Price);
            Assert.Equal("100", result[0].Quantity.ToString());

        }

        #endregion

        #region ----- TU03 -  GetProductById_ReturnOneProductFromId -----
        [Fact]
        public void GetProductById_ReturnOneProductFromId()
        {
            var product = new List<Product>
            {
                produit1,
                produit2
            };

            _mockProductRepository.Setup(repo => repo.GetAllProducts()).Returns(product);

            var result = _productService.GetProductById(1);

            Assert.NotNull(result);
            Assert.Equal("Product 1", result.Name);
            Assert.Equal(10.5, result.Price);
            Assert.Equal(100, result.Quantity);
            Assert.Equal("Desc1", result.Description);
            Assert.Equal("Details1", result.Details);
            Assert.Equal(1, result.Id);
        }
        #endregion

        #region ----- TU04 -  GetProductByIdViewModel_ReturnsCorrectProduct -----

        [Fact]
        public void GetProductByIdViewModel_ReturnsCorrectProduct()
        {
            // Arrange
            var products = new List<Product>
            {
                produit1,
                produit2

            };

            _mockProductRepository.Setup(repo => repo.GetAllProducts()).Returns(products);

            // Act
            var result = _productService.GetProductByIdViewModel(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product 1", result.Name);
        }

        #endregion

        #region ----- TU05 -  SaveProduct_CallsRepositoryWithCorrectEntity -----

        [Fact]
        public void SaveProduct_CallsRepositoryWithCorrectEntity()
        {
            // Arrange
            //productViewModel

            // Act
            _productService.SaveProduct(productViewModel2);

            // Assert
            _mockProductRepository.Verify(repo =>
                repo.SaveProduct(It.Is<Product>(p =>
                    p.Name == "Product 1" &&
                    p.Price == 10.5 &&
                    p.Quantity == 20
                )), Times.Once);
        }

        #endregion

        #region ----- TU06/TU07 -  CheckProductModelErrors_WithoutErrors / CheckProductModelErrors_WithErrors -----

        [Fact]
        public void CheckProductModelErrors_WithoutErrors()
        {
            //productViewModel

            _mockProductRepository.Setup(repo => repo.GetAllProducts()).Returns(new List<Product>());

            var result = _productService.CheckProductModelErrors(productViewModel2);

            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void CheckProductModelErrors_WithErrors()
        {
            //productViewModel

            _mockProductRepository.Setup(repo => repo.GetAllProducts()).Returns(new List<Product>());

            var result = _productService.CheckProductModelErrors(productViewModel);

            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
            Assert.Equal(null, result[0]);
        }


        #endregion

        #region ----- TU08 -  DeleteProduct_RemovesProductFromCartAndRepository -----

        [Fact]
        public void DeleteProduct_RemovesProductFromCartAndRepository()
        {
            // Arrange
            //produit1

            // Simuler la méthode GetProductById pour retourner un produit
            _mockProductRepository.Setup(repo => repo.GetAllProducts()).Returns(new List<Product> { produit1 });

            // Simuler le panier (_cart)
            var mockCart = new Mock<ICart>();
            _mockCart.Setup(cart => cart.RemoveLine(produit1)); // On ne fait qu'enregistrer l'appel ici

            // Act
            _productService.DeleteProduct(1);

            // Assert

            // Vérifie que le produit a été supprimé du panier
            _mockCart.Verify(cart => cart.RemoveLine(It.Is<Product>(p => p.Id == 1)), Times.Once);

            // Vérifie que le produit a été supprimé du repository
            _mockProductRepository.Verify(repo => repo.DeleteProduct(1), Times.Once);
        }

        #endregion

        #region ----- TU09 -  MapToViewModel_TranformEntityToViewModel -----

        [Fact]
        public void MapToViewModel_TranformEntityToViewModel()
        {
            IEnumerable<Product> productEntities = new List<Product>
            {
                produit1,
                produit2
            };

            var result = MapToViewModel(productEntities);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Product 1", result[0].Name);
            Assert.Equal("10.5", result[0].Price);
            Assert.Equal("200", result[1].Stock);

        }

        /// <summary>
        /// Clone from ProductService.MapToViewModel for testing
        /// Cause the method is private and static
        /// </summary>
        /// <param name="productEntities"></param>
        /// <returns></returns>
        private static List<ProductViewModel> MapToViewModel(IEnumerable<Product> productEntities)
        {
            List<ProductViewModel> products = new List<ProductViewModel>();
            foreach (Product product in productEntities)
            {
                products.Add(new ProductViewModel
                {
                    Id = product.Id,
                    Stock = product.Quantity.ToString(),
                    Price = product.Price.ToString(CultureInfo.InvariantCulture),
                    Name = product.Name,
                    Description = product.Description,
                    Details = product.Details
                });
            }

            return products;
        }

        #endregion

        #region ----- TU10 -  MapToProductEntity_TransformProductViewModelToProductEntity -----

        [Fact]
        public void MapToProductEntity_TransformProductViewModelToProductEntity()
        {


            var result = MapToProductEntity(productViewModel2);


            Assert.NotNull(result);
            Assert.Equal("Product 1", result.Name);
            Assert.Equal(10.5, result.Price);
            Assert.Equal(20, result.Quantity);
            Assert.Equal("Description", result.Description);
            Assert.Equal("Details", result.Details);
        }

        private static Product MapToProductEntity(ProductViewModel product)
        {
            Product productEntity = new Product
            {
                Name = product.Name,
                Price = double.Parse(product.Price),
                Quantity = int.Parse(product.Stock),
                Description = product.Description,
                Details = product.Details
            };
            return productEntity;
        }

        #endregion

        #region ----- TU11 -   GetProduct_ReturnListOfProduct -----

        [Fact]
        public async Task GetProduct_ReturnsListOfProducts()
        {

            var products = new List<Product> { produit1, produit2 };

            // Simuler GetProduct() pour qu'il retourne une tâche terminée
            _mockProductRepository.Setup(repo => repo.GetProduct()).Returns(Task.FromResult((IList<Product>)products));

            var result = await _productService.GetProduct();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Product 1", result[0].Name);
            Assert.Equal(12.5, result[1].Price);
            Assert.Equal(100, result[0].Quantity);
        }


        #endregion

        #endregion


    }

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

            Context = new P3Referential(options, new ConfigurationBuilder().Build());
            ProductService = new Models.Repositories.ProductRepository(Context);
            _productService = new Models.Services.ProductRepository(_Cart, ProductService, null, null);
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

        #region ----- TI01 - GetAllProduct_ReturnListOfProducts_IntegrationTest -----

        [Fact]
        public void GetAllProduct_ReturnListOfProducts_IntegrationTest()
        {
            var result = _productService.GetAllProducts();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Product 1", result.First().Name);
            Assert.Equal(12.5, result.Last().Price);
            Assert.Equal("100", result.First().Quantity.ToString());
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
            try
            {
                _productController.Create(product);

            }
            catch
            {
                Assert.False(false);
            }
            // Assert
            var result = Context.Product.FirstOrDefault(p => p.Name == "Product 3");
            var products = Context.Product.ToList();
            Assert.Equal(2, products.Count);
            Assert.Null(result);
            Dispose();
        }


        #endregion

        #endregion
    }
}
