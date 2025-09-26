using Amazon.Runtime.Internal.Transform;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TTH_Inventory_Mngt.WebApi.Common.Models;
using TTH_Inventory_Mngt.WebApi.CommonServices;
using TTH_Inventory_Mngt.WebApi.Public.Controllers;
using Xunit;

namespace TTH_Inventory_Mngt.WebApi.Public.Tests
{
    /// <summary>
    /// Unit tests for <see cref="MngtController"/>.
    /// Ensures all endpoints are tested for success and failure scenarios.
    /// </summary>
    public class MngtControllerTests
    {
        #region Test Setup
        private readonly Mock<IProductsServices> _mockService;
        private readonly MngtController _controller;

        private static readonly string _ytd = DateTime.Now.AddDays(-1).ToString(Const.FMT_DATE_TIME_DEFAULT);
        private static readonly string _tdy = DateTime.Now.ToString(Const.FMT_DATE_TIME_DEFAULT);
        private const string _mockInstitutionCode = "9900000001";
        private const string _mockProductId = "001";
        private const string _mockProductName = "Name";

        /// <summary>
        /// Initializes test dependencies and controller instance.
        /// </summary>
        public MngtControllerTests()
        {
            _mockService = new Mock<IProductsServices>();
            _controller = new MngtController(_mockService.Object);
        }

        #endregion Test Setup

        #region GetListAllProductsAsync Tests

        /// <summary>
        /// Should return 200 OK with a product list when service succeeds.
        /// </summary>
        [Fact]
        public async Task GetListAllProductsAsync_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var expected = new List<ProductsResponse>
            {
                new ProductsResponse(new Products { InstitutionCode = _mockInstitutionCode, ProductId = _mockProductId })
            };
            _mockService.Setup(s => s.GetListAllProductsAsync()).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetListAllProductsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(expected, okResult.Value);
        }

        /// <summary>
        /// Should return 500 when service throws exception.
        /// </summary>
        [Fact]
        public async Task GetListAllProductsAsync_ShouldReturn500_WhenServiceThrows()
        {
            // Arrange
            _mockService.Setup(s => s.GetListAllProductsAsync()).ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.GetListAllProductsAsync();

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
        }

        /// <summary>
        /// Should return 400 when ModelState is invalid in GetListAllProductsAsync.
        /// </summary>
        [Fact]
        public async Task GetListAllProductsAsync_ShouldReturn400_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = await _controller.GetListAllProductsAsync();

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
        }
        #endregion

        #region GetListProductsAsync (institutionCode) Tests

        /// <summary>
        /// Should return 200 OK with products for given institution.
        /// </summary>
        [Fact]
        public async Task GetListProductsAsync_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var expected = new List<ProductsResponse>
            {
                new ProductsResponse(new Products { InstitutionCode = _mockInstitutionCode, ProductId = _mockProductId })
            };
            _mockService.Setup(s => s.GetListProductsAsync(_mockInstitutionCode)).ReturnsAsync(expected);

            // Act
            var result = await _controller.GetListProductsAsync(_mockInstitutionCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
            Assert.Equal(expected, okResult.Value);
        }

        /// <summary>
        /// Should return 500 when service throws.
        /// </summary>
        [Fact]
        public async Task GetListProductsAsync_ShouldReturn500_WhenServiceThrows()
        {
            // Arrange
            _mockService.Setup(s => s.GetListProductsAsync(_mockInstitutionCode)).ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.GetListProductsAsync(_mockInstitutionCode);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
        }

        /// <summary>
        /// Should return 400 when ModelState is invalid in GetListProductsAsync (institutionCode).
        /// </summary>
        [Fact]
        public async Task GetListProductsAsync_ShouldReturn400_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = await _controller.GetListProductsAsync("1234567890");

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
        }
        #endregion

        #region GetListProductsAsync (search) Tests

        /// <summary>
        /// Should return 200 OK with search results.
        /// </summary>
        [Fact]
        public async Task GetListProductsSearchAsync_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var request = new ProductsRequest
            {
                InstitutionCode = _mockInstitutionCode,
                ProductId = _mockProductId,
                ProductName = _mockProductName,
                InStocked = 20,
                Created = _tdy[..8],
                Updated = _tdy[..8]
            };

            var expected = new List<ProductsResponse>
            {
                new ProductsResponse(new Products { InstitutionCode = _mockInstitutionCode, ProductId = _mockProductId })
            };
            _mockService.Setup(s => s.SearchListProductsAsync(request)).ReturnsAsync(expected);

            // Act
            var result = await _controller.SearchListProductsAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, okResult.Value);
        }

        /// <summary>
        /// Should return 500 when search fails.
        /// </summary>
        [Fact]
        public async Task GetListProductsSearchAsync_ShouldReturn500_WhenServiceThrows()
        {
            // Arrange
            var request = new ProductsRequest { InstitutionCode = _mockInstitutionCode };
            _mockService.Setup(s => s.SearchListProductsAsync(request)).ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.SearchListProductsAsync(request);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
        }

        /// <summary>
        /// Should return 400 when ModelState is invalid in GetListProductsAsync (search).
        /// </summary>
        [Fact]
        public async Task GetListProductsSearchAsync_ShouldReturn400_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "error");
            var request = new ProductsRequest { InstitutionCode = "INST001" };

            // Act
            var result = await _controller.SearchListProductsAsync(request);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
        }
        #endregion

        #region RegisterProductsAsync Tests

        /// <summary>
        /// Should return 200 OK when registration succeeds.
        /// </summary>
        [Fact]
        public async Task RegisterProductsAsync_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var request = new ProductsRequest { InstitutionCode = _mockInstitutionCode, ProductId = _mockProductId };
            _mockService.Setup(s => s.RegisterProductsAsync(request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RegisterProductsAsync(_mockInstitutionCode, request);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Should return 500 when registration throws.
        /// </summary>
        [Fact]
        public async Task RegisterProductsAsync_ShouldReturn500_WhenServiceThrows()
        {
            // Arrange
            var request = new ProductsRequest { InstitutionCode = _mockInstitutionCode };
            _mockService.Setup(s => s.RegisterProductsAsync(request)).ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.RegisterProductsAsync(_mockInstitutionCode, request);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
        }

        /// <summary>
        /// Should return 400 when ModelState is invalid in RegisterProductsAsync.
        /// </summary>
        [Fact]
        public async Task RegisterProductsAsync_ShouldReturn400_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "error");
            var request = new ProductsRequest { InstitutionCode = "INST001" };

            // Act
            var result = await _controller.RegisterProductsAsync("INST001", request);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
        }
        #endregion

        #region UpdateProductsAsync Tests

        /// <summary>
        /// Should return 200 OK when update succeeds.
        /// </summary>
        [Fact]
        public async Task UpdateProductsAsync_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var request = new ProductsRequest { InstitutionCode = _mockInstitutionCode, ProductId = _mockProductId };
            _mockService.Setup(s => s.SaveProductsAsync(request)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SaveProductsAsync(_mockInstitutionCode, request);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        /// <summary>
        /// Should return 500 when update throws.
        /// </summary>
        [Fact]
        public async Task UpdateProductsAsync_ShouldReturn500_WhenServiceThrows()
        {
            // Arrange
            var request = new ProductsRequest { InstitutionCode = _mockInstitutionCode };
            _mockService.Setup(s => s.SaveProductsAsync(request)).ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _controller.SaveProductsAsync(_mockInstitutionCode, request);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, obj.StatusCode);
        }

        /// <summary>
        /// Should return 400 when ModelState is invalid in SaveProductsAsync.
        /// </summary>
        [Fact]
        public async Task UpdateProductsAsync_ShouldReturn400_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "error");
            var request = new ProductsRequest { InstitutionCode = "INST001" };

            // Act
            var result = await _controller.SaveProductsAsync("INST001", request);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, obj.StatusCode);
        }

        #endregion

        #region Test: Constructor

        /// <summary>
        /// Ensures controller constructor throws <see cref="ArgumentNullException"/> 
        /// when null service is provided.
        /// </summary>
        [Fact]
        public void Constructor_NullRepository_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new MngtController(null!));
        }

        #endregion Test: Constructor

        #region Startup DI Tests
        [Fact]
        public void Startup_DI_ShouldBeRegistered()
        {
            // -- Arrange --------------------------------------------------------------------- //

            var serviceCollection = new ServiceCollection();

            // Register the mocked IConfiguration
            // Build in-memory configuration
            var inMemorySettings = new Dictionary<string, string> {
                {"AWS:ServiceURL", "http://localhost:8000"},
                {"AWS:Region", "ap-southeast-1"},
                {"AWS:AccessKey", "dummy"},
                {"AWS:SecretKey", "dummy"},
                {"ConnectionStrings:DefaultConnection", "Server=localhost;Database=InventoryDb;Trusted_Connection=True;TrustServerCertificate=True;" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Register all services as done in Startup.cs
            var startup = new Startup(configuration);
            startup.ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // -- Act & Assert  ---------------------------------------------------------------------- //
            // Test if all services can be resolved
            var servicesToTest = serviceCollection.Select(service => service.ServiceType).Distinct().ToList();
            foreach (var serviceType in servicesToTest)
            {
                try
                {
                    if (serviceType.FullName.Contains("TTH_Inventory_Mngt.WebApi.") ||
                        serviceType.FullName.Contains("Amazon."))
                    {
                        var service = serviceProvider.GetService(serviceType);
                        Assert.NotNull(service); // Ensure the service resolves correctly
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Failed to resolve service of type {serviceType}: {ex.Message}");
                }
            }
        }

        #endregion Startup DI Tests
    }
}
