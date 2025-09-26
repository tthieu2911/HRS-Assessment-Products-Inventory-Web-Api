using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TTH_Inventory_Mngt.WebApi.Common.Models;
using TTH_Inventory_Mngt.WebApi.DataAccess;
using Xunit;

namespace TTH_Inventory_Mngt.WebApi.CommonServices.Tests
{
    /// <summary>
    /// Unit tests for <see cref="ProductsServices"/>.
    /// Uses XUnit + Moq to validate service behavior independently from DynamoDB.
    /// </summary>
    public class ProductsServicesTests
    {
        #region Test Setup

        /// <summary>
        /// Mock repository for injecting into the service.
        /// </summary>
        private readonly Mock<IProductsRepositoryBase> _productsRepository = new();

        // Mock constants for test data
        private static readonly string _ytd = DateTime.Now.AddDays(-1).ToString(Const.FMT_DATE_TIME_DEFAULT);
        private static readonly string _tdy = DateTime.Now.ToString(Const.FMT_DATE_TIME_DEFAULT);
        private const string _mockInstitutionCode = "9900000001";
        private const string _mockProductId = "001";
        private const string _mockProductName = "Name";

        /// <summary>
        /// Sample mock product response (simulating DynamoDB response from Repository).
        /// </summary>
        private readonly List<Products> _productsResponse =
        [
            new()
            {
                InstitutionCode = _mockInstitutionCode,
                ProductId = _mockProductId,
                ProductName = _mockProductName,
                InStocked = 20,
                Created = _tdy,
                Updated = _tdy
            }
        ];

        /// <summary>
        /// Initializes the test class.
        /// </summary>
        public ProductsServicesTests()
        {
        }

        #endregion Test Setup

        #region Test: GetListAllProductsAsync

        /// <summary>
        /// Validates that <see cref="ProductsServices.GetListAllProductsAsync"/> 
        /// returns a non-empty product list when the repository returns data.
        /// </summary>
        [Fact]
        public async Task GetListAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange: mock repository behavior
            _productsRepository
                .Setup(x => x.GetListAllProductsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_productsResponse);

            // Act: invoke the service
            var service = new ProductsServices(_productsRepository.Object);
            var result = await service.GetListAllProductsAsync();

            // Assert: validate non-empty result
            Assert.NotEmpty(result);
            Assert.Single(result); // only 1 product in mock response
            Assert.Equal(_mockProductId, result[0].ProductId);
        }

        #endregion Test: GetListAllProductsAsync

        #region Test: GetListProductsAsync

        /// <summary>
        /// Validates that <see cref="ProductsServices.GetListProductsAsync"/> 
        /// correctly returns products for a given institution code.
        /// </summary>
        [Fact]
        public async Task GetListProductsAsync_ShouldReturnProductsForInstitution()
        {
            // Arrange
            _productsRepository
                .Setup(x => x.GetListProductsAsync(_mockInstitutionCode))
                .ReturnsAsync(_productsResponse);

            var service = new ProductsServices(_productsRepository.Object);

            // Act
            var result = await service.GetListProductsAsync(_mockInstitutionCode);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(_mockInstitutionCode, result[0].InstitutionCode);
        }

        /// <summary>
        /// Validates that <see cref="ProductsServices.GetListProductsAsync"/> 
        /// throws ArgumentNullException due to null value is passed for a HashKey field
        /// </summary>
        [Fact]
        public async Task GetListProductsAsync_NullInstitutionCode_ShouldThrowsArgumentNullException()
        {
            // Arrange
            _productsRepository
                .Setup(x => x.GetListProductsAsync(null))
                .ThrowsAsync(new ArgumentNullException());

            var service = new ProductsServices(_productsRepository.Object);

            // Act
            var action = async () => await service.GetListProductsAsync(null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        #endregion Test: GetListProductsAsync

        #region Test: SearchListProductsAsync

        /// <summary>
        /// Validates that <see cref="ProductsServices.SearchListProductsAsync"/> 
        /// returns products that match the search criteria.
        /// </summary>
        [Fact]
        public async Task SearchListProductsAsync_ShouldReturnMatchingProducts()
        {
            // Arrange
            var request = new ProductsRequest
            {
                InstitutionCode = _mockInstitutionCode,
                ProductId = _mockProductId,
                ProductName = _mockProductName,
                InStocked = 20,
                Created = _tdy,
                Updated = _tdy
            };

            _productsRepository
                .Setup(x => x.GetListProductsAsyncByQuery(
                    request.InstitutionCode,
                    request.ProductId,
                    request.ProductName,
                    request.InStocked,
                    request.Created,
                    request.Updated))
                .ReturnsAsync(_productsResponse);

            var service = new ProductsServices(_productsRepository.Object);

            // Act
            var result = await service.SearchListProductsAsync(request);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(_mockProductName, result[0].ProductName);
        }

        /// <summary>
        /// Validates that <see cref="ProductsServices.SearchListProductsAsync"/> 
        /// throws ArgumentNullException due to null value is passed for a key fields
        /// </summary>
        [Theory]
        [InlineData(null, _mockProductId, _mockProductName)]
        [InlineData(_mockInstitutionCode, null, _mockProductName)]
        [InlineData(_mockInstitutionCode, _mockProductId, null)]
        public async Task SearchListProductsAsync_NullQueryParameters_ShouldThrowsArgumentNullException(string institutionCode, string productId, string productName)
        {
            // Arrange
            var request = new ProductsRequest
            {
                InstitutionCode = institutionCode,
                ProductId = productId,
                ProductName = productName,
                InStocked = 20,
                Created = _tdy,
                Updated = _tdy
            };

            _productsRepository
                .Setup(x => x.GetListProductsAsyncByQuery(
                    request.InstitutionCode,
                    request.ProductId,
                    request.ProductName,
                    request.InStocked,
                    request.Created,
                    request.Updated))
                .ThrowsAsync(new ArgumentNullException());

            var service = new ProductsServices(_productsRepository.Object);

            // Act
            var action = async () => await service.SearchListProductsAsync(request);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        #endregion Test: SearchListProductsAsync

        #region Test: RegisterProductsAsync

        /// <summary>
        /// Validates that <see cref="ProductsServices.RegisterProductsAsync"/> 
        /// calls the repository's PutProductsAsync with the correct model.
        /// </summary>
        [Fact]
        public async Task RegisterProductsAsync_ShouldCallRepository()
        {
            // Arrange
            var request = new ProductsRequest
            {
                InstitutionCode = _mockInstitutionCode,
                ProductId = _mockProductId,
                ProductName = _mockProductName,
                InStocked = 20,
                Created = _tdy,
                Updated = _tdy
            };

            var service = new ProductsServices(_productsRepository.Object);

            // Act
            await service.RegisterProductsAsync(request);

            // Assert
            _productsRepository.Verify(
                x => x.PutProductsAsync(It.Is<Products>(p =>
                    p.ProductId == _mockProductId &&
                    p.ProductName == _mockProductName)),
                Times.Once);
        }

        /// <summary>
        /// Validates that <see cref="ProductsServices.RegisterProductsAsync"/> 
        /// calls the repository's PutProductsAsync with model.
        /// throws ArgumentNullException due to null value is passed for a key fields
        /// </summary>
        [Theory]
        [InlineData(null, _mockProductId, _mockProductName)]
        [InlineData(_mockInstitutionCode, null, _mockProductName)]
        [InlineData(_mockInstitutionCode, _mockProductId, null)]
        public async Task RegisterProductsAsync_NullQueryParameters_ShouldThrowsArgumentNullException(string institutionCode, string productId, string productName)
        {
            // Arrange
            var request = new ProductsRequest
            {
                InstitutionCode = institutionCode,
                ProductId = productId,
                ProductName = productName,
                InStocked = 20,
                Created = _tdy,
                Updated = _tdy
            };

            _productsRepository
                .Setup(x => x.PutProductsAsync(It.Is<Products>(r => r.InstitutionCode == institutionCode)))
                .ThrowsAsync(new ArgumentNullException());

            var service = new ProductsServices(_productsRepository.Object);

            // Act
            var action = async() => await service.RegisterProductsAsync(request);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        #endregion Test: RegisterProductsAsync

        #region Test: SaveProductsAsync

        /// <summary>
        /// Validates that <see cref="ProductsServices.SaveProductsAsync"/> 
        /// calls the repository's UpdateProductsAsync with the correct model.
        /// </summary>
        [Fact]
        public async Task SaveProductsAsync_ShouldCallRepository()
        {
            // Arrange
            var request = new ProductsRequest
            {
                InstitutionCode = _mockInstitutionCode,
                ProductId = _mockProductId,
                ProductName = _mockProductName,
                InStocked = 25,
                Created = _ytd,
                Updated = _tdy
            };

            var service = new ProductsServices(_productsRepository.Object);

            // Act
            await service.SaveProductsAsync(request);

            // Assert
            _productsRepository.Verify(
                x => x.UpdateProductsAsync(It.Is<Products>(p =>
                    p.ProductId == _mockProductId &&
                    p.InStocked == 25)),
                Times.Once);
        }

        /// <summary>
        /// Validates that <see cref="ProductsServices.SaveProductsAsync"/> 
        /// calls the repository's UpdateProductsAsync with the correct model.
        /// throws ArgumentNullException due to null value is passed for a key fields
        /// </summary>
        [Theory]
        [InlineData(null, _mockProductId, _mockProductName)]
        [InlineData(_mockInstitutionCode, null, _mockProductName)]
        [InlineData(_mockInstitutionCode, _mockProductId, null)]
        public async Task SaveProductsAsync_NullQueryParameters_ShouldThrowsArgumentNullException(string institutionCode, string productId, string productName)
        {
            // Arrange
            var request = new ProductsRequest
            {
                InstitutionCode = institutionCode,
                ProductId = productId,
                ProductName = productName,
                InStocked = 25,
                Created = _ytd,
                Updated = _tdy
            };

            _productsRepository
                .Setup(x => x.UpdateProductsAsync(It.Is<Products>(r => r.InstitutionCode == institutionCode)))
                .ThrowsAsync(new ArgumentNullException());

            var service = new ProductsServices(_productsRepository.Object);

            // Act
            var action = async () => await service.SaveProductsAsync(request);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        #endregion Test: SaveProductsAsync

    }
}
