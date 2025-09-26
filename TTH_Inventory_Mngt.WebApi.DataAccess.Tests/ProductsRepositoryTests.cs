using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Moq;
using TTH_Inventory_Mngt.WebApi.Common.Models;
using Xunit;

namespace TTH_Inventory_Mngt.WebApi.DataAccess.Tests
{
    /// <summary>
    /// Unit tests for <see cref="ProductsRepository"/>.
    /// Validates DynamoDB access layer behavior using mocked <see cref="IAmazonDynamoDB"/>.
    /// </summary>
    public class ProductsRepositoryTests
    {
        #region Test Setup

        /// <summary>
        /// Mock Amazon DynamoDB client used for simulating responses.
        /// </summary>
        private readonly Mock<IAmazonDynamoDB> _mockDynamoDb;

        // Test constants
        private static readonly string _ytd = DateTime.Now.AddDays(-1).ToString(Const.FMT_DATE_TIME_DEFAULT);
        private static readonly string _tdy = DateTime.Now.ToString(Const.FMT_DATE_TIME_DEFAULT);
        private const string _mockInstitutionCode = "9900000001";
        private const string _mockProductId = "001";
        private const string _mockProductName = "Name";

        /// <summary>
        /// Sample DynamoDB JSON response (Scan/Query).
        /// Values {created}/{updated} are dynamically replaced.
        /// </summary>
        private string _mockResponseStr_with_LastEvaluatedKey = """
                {
                  "Items": [
                    {
                      "InstitutionCode": { "S": "9900000001" },
                      "ProductId": { "S": "P^A001" },
                      "ProductName": { "S": "Test name PA1" },
                      "InStocked": { "N": "1" },
                      "Created": { "S": "{created}" },
                      "Updated": { "S": "{updated}" }
                    },
                    {
                      "InstitutionCode": { "S": "9900000001" },
                      "ProductId": { "S": "P^A002" },
                      "ProductName": { "S": "Test name PA2" },
                      "InStocked": { "N": "10" },
                      "Created": { "S": "{created}" },
                      "Updated": { "S": "{updated}" }
                    },
                    {
                      "InstitutionCode": { "S": "9900000001" },
                      "ProductId": { "S": "P^A003" },
                      "ProductName": { "S": "Test name PA3" },
                      "Created": { "S": "{created}" }
                    },
                    {
                      "InstitutionCode": { "S": "9900000001" },
                      "ProductId": { "S": "P^A004" },
                      "ProductName": { "S": "Test name PA4" },
                      "Created": { "S": "{created}" },
                      "Updated": { "S": "{updated}" }
                    }
                  ],
                  "LastEvaluatedKey": {
                    "InstitutionCode": { "S": "9900000001" },
                    "ProductId": { "S": "P^A009" },
                    "ProductName": { "S": "Test name PA9" }
                  }
                }
                """;
        private string _mockResponseStr_with_out_LastEvaluatedKey = """
                {
                  "Items": [
                    {
                      "InstitutionCode": { "S": "9900000001" },
                      "ProductId": { "S": "P^A009" },
                      "ProductName": { "S": "Test name PA9" },
                      "InStocked": { "N": "1" },
                      "Created": { "S": "{created}" },
                      "Updated": { "S": "{updated}" }
                    }
                  ]
                }
                """;

        /// <summary>
        /// Constructor: sets up mocks and replaces placeholders in JSON.
        /// </summary>
        public ProductsRepositoryTests()
        {
            _mockDynamoDb = new Mock<IAmazonDynamoDB>();

            // Inject dynamic date values into response template
            _mockResponseStr_with_LastEvaluatedKey = _mockResponseStr_with_LastEvaluatedKey
                .Replace("{created}", _ytd)
                .Replace("{updated}", _tdy);

            _mockResponseStr_with_out_LastEvaluatedKey = _mockResponseStr_with_out_LastEvaluatedKey
                .Replace("{created}", _ytd)
                .Replace("{updated}", _tdy);
        }

        #endregion Test Setup

        #region Test: GetListAllProductsAsync

        /// <summary>
        /// Ensures <see cref="ProductsRepository.GetListAllProductsAsync"/> 
        /// returns a non-empty list when DynamoDB Scan returns items.
        /// </summary>
        [Fact]
        public async Task GetListAllProductsAsync_ShouldReturnListAllProducts()
        {
            // Arrange
            var mockScanResponse_with_LastEvaluatedKey = JsonSerializer.Deserialize<ScanResponse>(_mockResponseStr_with_LastEvaluatedKey);
            var mockScanResponse_with_out_LastEvaluatedKey = JsonSerializer.Deserialize<ScanResponse>(_mockResponseStr_with_out_LastEvaluatedKey);
            _mockDynamoDb
                .SetupSequence(x => x.ScanAsync(It.IsAny<ScanRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockScanResponse_with_LastEvaluatedKey)
                .ReturnsAsync(mockScanResponse_with_out_LastEvaluatedKey);

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            var result = await repository.GetListAllProductsAsync();

            // Assert
            Assert.NotEmpty(result);
        }

        #endregion Test: GetListAllProductsAsync

        #region Test: GetListProductsAsync

        /// <summary>
        /// Ensures <see cref="ProductsRepository.GetListProductsAsync"/> 
        /// returns products for a given institution code.
        /// </summary>
        [Fact]
        public async Task GetListProductsAsync_ByInstitutionCode_ShouldReturnListProducts()
        {
            // Arrange
            var mockQueryResponse_with_LastEvaluatedKey = JsonSerializer.Deserialize<QueryResponse>(_mockResponseStr_with_LastEvaluatedKey);
            var mockQueryResponse_with_out_LastEvaluatedKey = JsonSerializer.Deserialize<QueryResponse>(_mockResponseStr_with_out_LastEvaluatedKey);
            _mockDynamoDb
                .SetupSequence(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockQueryResponse_with_LastEvaluatedKey)
                .ReturnsAsync(mockQueryResponse_with_out_LastEvaluatedKey);

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            var result = await repository.GetListProductsAsync(_mockInstitutionCode);

            // Assert
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Ensures <see cref="ProductsRepository.GetListProductsAsync"/> 
        /// throws ArgumentNullException due to null value is passed for a key fields
        /// </summary>
        [Fact]
        public async Task GetListProductsAsync_NullInstitutionCode_ShouldThrowsArgumentNullException()
        {
            // Arrange
            _mockDynamoDb
                .SetupSequence(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentNullException());

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            var action = async () => await repository.GetListProductsAsync(null!);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        #endregion Test: GetListProductsAsync

        #region Test: GetListProductsAsyncByQuery

        /// <summary>
        /// Ensures <see cref="ProductsRepository.GetListProductsAsyncByQuery"/> 
        /// works with different filter combinations.
        /// </summary>
        [Theory]
        [InlineData(null, null, null)]
        [InlineData(1, null, null)]
        [InlineData(null, "{created}", null)]
        [InlineData(null, null, "{updated}")]
        [InlineData(10, "{created}", "{updated}")]
        public async Task GetListProductsAsyncByQuery_ByInputProductsRequest_ShouldReturnListProducts(
            int? inStocked, string? created, string? updated)
        {
            // Arrange
            var mockQueryResponse_with_LastEvaluatedKey = JsonSerializer.Deserialize<QueryResponse>(_mockResponseStr_with_LastEvaluatedKey);
            var mockQueryResponse_with_out_LastEvaluatedKey = JsonSerializer.Deserialize<QueryResponse>(_mockResponseStr_with_out_LastEvaluatedKey);
            _mockDynamoDb
                .SetupSequence(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockQueryResponse_with_LastEvaluatedKey)
                .ReturnsAsync(mockQueryResponse_with_out_LastEvaluatedKey);

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            var result = await repository.GetListProductsAsyncByQuery(
                _mockInstitutionCode, _mockProductId, _mockProductName, inStocked, created, updated);

            // Assert
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Ensures <see cref="ProductsRepository.GetListProductsAsyncByQuery"/> 
        /// throws ArgumentNullException due to null value is passed for a key fields
        /// </summary>
        [Theory]
        [InlineData(null, _mockProductId, _mockProductName)]
        [InlineData(_mockInstitutionCode, null, _mockProductName)]
        [InlineData(_mockInstitutionCode, _mockProductId, null)]
        public async Task GetListProductsAsyncByQuery_NullQueryParameters_ShouldThrowsArgumentNullException(string institutionCode, string productId, string productName)
        {
            // Arrange
            _mockDynamoDb
                .SetupSequence(x => x.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentNullException());

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            var action = async () => await repository.GetListProductsAsyncByQuery(institutionCode, productId, productName, null, null, null);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        #endregion Test: GetListProductsAsyncByQuery

        #region Test: PutProductsAsync

        /// <summary>
        /// Ensures <see cref="ProductsRepository.PutProductsAsync"/> 
        /// triggers a DynamoDB transaction write (insert).
        /// </summary>
        [Fact]
        public async Task PutProductsAsync_ShouldCallTransactWriteItemsAsync()
        {
            // Arrange
            var product = new Products
            {
                InstitutionCode = _mockInstitutionCode,
                ProductId = _mockProductId,
                ProductName = _mockProductName,
                InStocked = 20,
                Created = _tdy
            };

            _mockDynamoDb
                .Setup(x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransactWriteItemsResponse());

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            await repository.PutProductsAsync(product);

            // Assert
            _mockDynamoDb.Verify(
                x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Ensures <see cref="ProductsRepository.PutProductsAsync"/> 
        /// triggers a DynamoDB transaction write (insert).
        /// </summary>
        [Fact]
        public async Task PutProductsAsync_NullCreatedDate_ShouldUseCurrentDateTime_ShouldCallTransactWriteItemsAsync()
        {
            // Arrange
            var product = new Products
            {
                InstitutionCode = _mockInstitutionCode,
                ProductId = _mockProductId,
                ProductName = _mockProductName,
                InStocked = 20
            };

            var checkRequest = new TransactWriteItemsRequest();
            _mockDynamoDb
                .Setup(x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()))
                .Callback((TransactWriteItemsRequest request, CancellationToken token) =>
                {
                    checkRequest = request;
                })
                .ReturnsAsync(new TransactWriteItemsResponse());

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            await repository.PutProductsAsync(product);

            // Assert            
            _mockDynamoDb.Verify(
                x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.Equal(_tdy[..8], checkRequest.TransactItems[0].Put.Item["Created"].S.ToString()[..8]);
        }

        /// <summary>
        /// Ensures <see cref="ProductsRepository.PutProductsAsync"/> 
        /// throws ArgumentNullException due to null value is passed for a key fields
        /// </summary>
        [Theory]
        [InlineData(null, _mockProductId, _mockProductName)]
        [InlineData(_mockInstitutionCode, null, _mockProductName)]
        [InlineData(_mockInstitutionCode, _mockProductId, null)]
        public async Task PutProductsAsync_NullQueryParameters_ShouldThrowsArgumentNullException(string institutionCode, string productId, string productName)
        {
            // Arrange
            var product = new Products
            {
                InstitutionCode = institutionCode,
                ProductId = productId,
                ProductName = productName,
                InStocked = 20,
                Created = _tdy
            };

            _mockDynamoDb
                .Setup(x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentNullException());

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            var action = async () => await repository.PutProductsAsync(product);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        #endregion Test: PutProductsAsync

        #region Test: UpdateProductsAsync

        /// <summary>
        /// Ensures <see cref="ProductsRepository.UpdateProductsAsync"/> 
        /// triggers a DynamoDB transaction write (update).
        /// </summary>
        [Fact]
        public async Task UpdateProductsAsync_ShouldCallTransactWriteItemsAsync()
        {
            // Arrange
            var product = new Products
            {
                InstitutionCode = _mockInstitutionCode,
                ProductId = _mockProductId,
                ProductName = _mockProductName,
                InStocked = 20,
                Created = _tdy,
                Updated = _tdy
            };

            _mockDynamoDb
                .Setup(x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TransactWriteItemsResponse());

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            await repository.UpdateProductsAsync(product);

            // Assert
            _mockDynamoDb.Verify(
                x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// Ensures <see cref="ProductsRepository.UpdateProductsAsync"/> 
        /// throws ArgumentNullException due to null value is passed for a key fields
        /// </summary>
        [Theory]
        [InlineData(null, _mockProductId, _mockProductName)]
        [InlineData(_mockInstitutionCode, null, _mockProductName)]
        [InlineData(_mockInstitutionCode, _mockProductId, null)]
        public async Task UpdateProductsAsync_NullQueryParameters_ShouldThrowsArgumentNullException(string institutionCode, string productId, string productName)
        {
            // Arrange
            var product = new Products
            {
                InstitutionCode = institutionCode,
                ProductId = productId,
                ProductName = productName,
                InStocked = 20,
                Created = _tdy,
                Updated = _tdy
            };

            _mockDynamoDb
                .Setup(x => x.TransactWriteItemsAsync(It.IsAny<TransactWriteItemsRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentNullException());

            var repository = new ProductsRepository(_mockDynamoDb.Object);

            // Act
            var action = async () => await repository.UpdateProductsAsync(product);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        #endregion Test: UpdateProductsAsync

        #region Test: Constructor

        /// <summary>
        /// Ensures repository constructor throws <see cref="ArgumentNullException"/> 
        /// when null client is provided.
        /// </summary>
        [Fact]
        public void Constructor_NullClient_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductsRepository(null!));
        }

        #endregion Test: Constructor
    }
}
