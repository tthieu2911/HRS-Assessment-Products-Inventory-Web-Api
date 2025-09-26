using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TTH_Inventory_Mngt.WebApi.Common.Models;
using Xunit;

namespace TTH_Inventory_Mngt.WebApi.DataAccess.Tests
{
    /// <summary>
    /// Unit tests for <see cref="ProductsRepositoryEF"/>.
    /// Verifies EF Core repository behavior using InMemoryDatabase and reflection for private fields.
    /// </summary>
    public class ProductsRepositoryEFTests
    {
        #region Helpers

        /// <summary>
        /// Creates a repository backed by an in-memory <see cref="InventoryDbContext"/>.
        /// Each call uses an isolated database name, so tests do not interfere.
        /// </summary>
        /// <param name="dbName">Unique database name for isolation.</param>
        /// <returns>A new instance of <see cref="ProductsRepositoryEF"/>.</returns>
        private ProductsRepositoryEF CreateRepository(string dbName)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new InventoryDbContext(options);
            return new ProductsRepositoryEF(context);
        }

        /// <summary>
        /// Uses reflection to obtain the private field '_context' from the repository instance.
        /// This allows assertions against the backing <see cref="InventoryDbContext"/> used by the repo.
        /// </summary>
        /// <param name="repo">Repository instance.</param>
        /// <returns>The <see cref="InventoryDbContext"/> instance stored in the private field.</returns>
        private InventoryDbContext GetPrivateContext(ProductsRepositoryEF repo)
        {
            var field = typeof(ProductsRepositoryEF)
                .GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new InvalidOperationException("Private field '_context' not found on ProductsRepositoryEF.");

            return (InventoryDbContext)field.GetValue(repo)!;
        }

        #endregion Helpers


        #region Constructor Tests

        /// <summary>
        /// Validates that the repository constructor properly sets the private _context field.
        /// </summary>
        [Fact]
        public void Constructor_Should_Set_Context()
        {
            // Arrange
            var repo = CreateRepository(nameof(Constructor_Should_Set_Context));

            // Act
            var context = GetPrivateContext(repo);

            // Assert
            Assert.NotNull(context);
        }

        #endregion Constructor Tests

        #region GetListAllProductsAsync Tests

        /// <summary>
        /// Verifies that GetListAllProductsAsync returns products ordered by InstitutionCode, then ProductId.
        /// </summary>
        [Fact]
        public async Task GetListAllProductsAsync_ShouldReturnOrderedProducts()
        {
            // Arrange
            var repo = CreateRepository(nameof(GetListAllProductsAsync_ShouldReturnOrderedProducts));
            var context = GetPrivateContext(repo);

            context.Products.AddRange(
                new Products { InstitutionCode = "B", ProductId = "2", ProductName = "B2" },
                new Products { InstitutionCode = "A", ProductId = "1", ProductName = "A1" },
                new Products { InstitutionCode = "A", ProductId = "2", ProductName = "A2" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetListAllProductsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("A", result[0].InstitutionCode);
            Assert.Equal("1", result[0].ProductId);
        }

        #endregion GetListAllProductsAsync Tests

        #region GetListProductsAsync Tests

        /// <summary>
        /// Verifies that GetListProductsAsync filters by InstitutionCode.
        /// </summary>
        [Fact]
        public async Task GetListProductsAsync_ShouldFilterByInstitution()
        {
            // Arrange
            var repo = CreateRepository(nameof(GetListProductsAsync_ShouldFilterByInstitution));
            var context = GetPrivateContext(repo);

            context.Products.AddRange(
                new Products { InstitutionCode = "X", ProductId = "1", ProductName = "X1" },
                new Products { InstitutionCode = "Y", ProductId = "2", ProductName = "Y1" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetListProductsAsync("X");

            // Assert
            Assert.Single(result);
            Assert.Equal("X1", result[0].ProductName);
            Assert.Equal("X", result[0].InstitutionCode);
        }

        #endregion GetListProductsAsync Tests

        #region GetListProductsAsyncByQuery Tests

        /// <summary>
        /// Verifies GetListProductsAsyncByQuery applies starts-with filters and optional filters correctly.
        /// </summary>
        [Fact]
        public async Task GetListProductsAsyncByQuery_ShouldApplyFilters()
        {
            // Arrange
            var repo = CreateRepository(nameof(GetListProductsAsyncByQuery_ShouldApplyFilters));
            var context = GetPrivateContext(repo);

            context.Products.AddRange(
                new Products
                {
                    InstitutionCode = "Z",
                    ProductId = "123",
                    ProductName = "Apple",
                    InStocked = 10,
                    Created = "202509011200",
                    Updated = "202509021200"
                },
                new Products
                {
                    InstitutionCode = "Z",
                    ProductId = "124",
                    ProductName = "Banana",
                    InStocked = 5,
                    Created = "202509011200",
                    Updated = "202509021200"
                }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetListProductsAsyncByQuery(
                "Z",
                "12",      // starts-with ProductId
                "App",     // starts-with ProductName
                10,        // inStocked
                "20250901",  // created starts-with
                "20250902" // updated
            );

            // Assert
            Assert.Single(result);
            Assert.Equal("Apple", result[0].ProductName);
        }

        /// <summary>
        /// Verifies GetListProductsAsyncByQuery applies starts-with filters and optional filters correctly.
        /// </summary>
        [Fact]
        public async Task GetListProductsAsyncByQuery_LeastQueryParam_ShouldApplyFilters()
        {
            // Arrange
            var repo = CreateRepository(nameof(GetListProductsAsyncByQuery_ShouldApplyFilters));
            var context = GetPrivateContext(repo);

            context.Products.AddRange(
                new Products
                {
                    InstitutionCode = "Z",
                    ProductId = "225",
                    ProductName = "Apple 5",
                    InStocked = 10,
                    Created = "202509011200",
                    Updated = "202509021200"
                },
                new Products
                {
                    InstitutionCode = "Z",
                    ProductId = "226",
                    ProductName = "Banana 6",
                    InStocked = 5,
                    Created = "202509011200",
                    Updated = "202509021200"
                }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetListProductsAsyncByQuery(
                "Z",
                "225",     // starts-with ProductId
                "App",     // starts-with ProductName
                null,      // inStocked
                null,      // created
                null       // updated
            );

            // Assert
            Assert.Single(result);
            Assert.Equal("Apple 5", result[0].ProductName);
        }

        #endregion GetListProductsAsyncByQuery Tests

        #region PutProductsAsync Tests

        /// <summary>
        /// Verifies that PutProductsAsync inserts a product when it does not already exist.
        /// </summary>
        [Fact]
        public async Task PutProductsAsync_ShouldInsertProduct_WhenNotExists()
        {
            // Arrange
            var repo = CreateRepository(nameof(PutProductsAsync_ShouldInsertProduct_WhenNotExists));
            var context = GetPrivateContext(repo);

            var product = new Products
            {
                InstitutionCode = "N",
                ProductId = "P1",
                ProductName = "New Product"
            };

            // Act
            await repo.PutProductsAsync(product);

            // Assert
            var stored = (await context.Products.Where(p => p.InstitutionCode == "N" && p.ProductId == "P1").ToListAsync()).FirstOrDefault();
            Assert.NotNull(stored);
            Assert.Equal("New Product", stored!.ProductName);
            Assert.False(string.IsNullOrEmpty(stored.Created));
        }

        /// <summary>
        /// Verifies that PutProductsAsync throws when the product already exists.
        /// </summary>
        [Fact]
        public async Task PutProductsAsync_ShouldThrow_WhenAlreadyExists()
        {
            // Arrange
            var repo = CreateRepository(nameof(PutProductsAsync_ShouldThrow_WhenAlreadyExists));
            var context = GetPrivateContext(repo);

            context.Products.Add(new Products { InstitutionCode = "N", ProductId = "P1", ProductName = "Exists" });
            await context.SaveChangesAsync();

            var duplicate = new Products { InstitutionCode = "N", ProductId = "P1", ProductName = "Dup" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await repo.PutProductsAsync(duplicate));
        }

        #endregion PutProductsAsync Tests

        #region UpdateProductsAsync Tests

        /// <summary>
        /// Verifies that UpdateProductsAsync updates an existing product's fields and sets Updated timestamp.
        /// </summary>
        [Fact]
        public async Task UpdateProductsAsync_ShouldUpdate_WhenExists()
        {
            // Arrange
            var repo = CreateRepository(nameof(UpdateProductsAsync_ShouldUpdate_WhenExists));
            var context = GetPrivateContext(repo);

            context.Products.Add(new Products
            {
                InstitutionCode = "N",
                ProductId = "P1",
                ProductName = "Old Name",
                InStocked = 1
            });
            await context.SaveChangesAsync();

            var updated = new Products
            {
                InstitutionCode = "N",
                ProductId = "P1",
                ProductName = "New Name",
                InStocked = 50
            };

            // Act
            await repo.UpdateProductsAsync(updated);

            // Assert
            var stored = (await context.Products.Where(p => p.InstitutionCode == "N" && p.ProductId == "P1").ToListAsync()).FirstOrDefault();
            Assert.NotNull(stored);
            Assert.Equal("New Name", stored!.ProductName);
            Assert.Equal(50, stored.InStocked);
            Assert.False(string.IsNullOrEmpty(stored.Updated));
        }

        /// <summary>
        /// Verifies that UpdateProductsAsync updates an existing product's fields and sets Updated timestamp.
        /// </summary>
        [Fact]
        public async Task UpdateProductsAsync_ShouldUpdateExistingValue_WhenNotProvidedProductNameAndInstocked()
        {
            // Arrange
            var repo = CreateRepository(nameof(UpdateProductsAsync_ShouldUpdate_WhenExists));
            var context = GetPrivateContext(repo);

            context.Products.Add(new Products
            {
                InstitutionCode = "N",
                ProductId = "P2",
                ProductName = "Old Name",
                InStocked = 1
            });
            await context.SaveChangesAsync();

            var updated = new Products
            {
                InstitutionCode = "N",
                ProductId = "P2"
            };

            // Act
            await repo.UpdateProductsAsync(updated);

            // Assert
            var stored = (await context.Products.Where(p => p.InstitutionCode == "N" && p.ProductId == "P2").ToListAsync()).FirstOrDefault();
            Assert.NotNull(stored);
            Assert.Equal("Old Name", stored!.ProductName);
            Assert.Equal(1, stored.InStocked);
            Assert.False(string.IsNullOrEmpty(stored.Updated));
        }

        /// <summary>
        /// Verifies that UpdateProductsAsync throws when the product does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateProductsAsync_ShouldThrow_WhenNotExists()
        {
            // Arrange
            var repo = CreateRepository(nameof(UpdateProductsAsync_ShouldThrow_WhenNotExists));
            var context = GetPrivateContext(repo);

            var product = new Products
            {
                InstitutionCode = "N",
                ProductId = "999",
                ProductName = "NotExists"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await repo.UpdateProductsAsync(product));
        }

        #endregion UpdateProductsAsync Tests

        #region Test: Constructor

        /// <summary>
        /// Ensures repository constructor throws <see cref="ArgumentNullException"/> 
        /// when null client is provided.
        /// </summary>
        [Fact]
        public void Constructor_NullClient_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProductsRepositoryEF(null!));
        }

        #endregion Test: Constructor
    }
}
