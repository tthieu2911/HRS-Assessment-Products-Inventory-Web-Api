using TTH_Inventory_Mngt.WebApi.Common.Models;

namespace TTH_Inventory_Mngt.WebApi.CommonServices
{
    /// <summary>
    /// Service layer for managing product operations.
    /// Acts as a bridge between controller layer (API) and repository layer (DynamoDB).
    /// </summary>
    public interface IProductsServices
    {
        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>List of product responses.</returns>
        Task<List<ProductsResponse>> GetListAllProductsAsync();

        /// <summary>
        /// Gets all products for a given institution.
        /// </summary>
        /// <param name="institutionCode">Request institution code.</param>
        /// <returns>List of product responses.</returns>
        Task<List<ProductsResponse>> GetListProductsAsync(string institutionCode);

        /// <summary>
        /// Searches for products
        /// </summary>
        /// <param name="productsRequest">Search conditions (ProductName, ProductId, InStocked, Created, Updated).</param>
        /// <returns>List of matching product responses.</returns>
        Task<List<ProductsResponse>> SearchListProductsAsync(ProductsRequest productsRequest);

        /// <summary>
        /// Registers (inserts) a new product into DynamoDB.
        /// Uses transaction with "Put" and condition to ensure item does not already exist.
        /// </summary>
        /// <param name="productsRequest">Request containing product details.</param>
        Task RegisterProductsAsync(ProductsRequest productsRequest);

        /// <summary>
        /// Saves (updates) an existing product in DynamoDB.
        /// Uses transaction with "Update" and condition to ensure item already exists.
        /// </summary>
        /// <param name="productsRequest">Request containing updated product details.</param>
        Task SaveProductsAsync(ProductsRequest productsRequest);
    }
}
