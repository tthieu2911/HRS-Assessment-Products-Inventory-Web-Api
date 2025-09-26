using TTH_Inventory_Mngt.WebApi.Common.Models;

namespace TTH_Inventory_Mngt.WebApi.DataAccess
{
    /// <summary>
    /// Base Repository interface for CRUD operations on the Products DynamoDB table.
    /// </summary>
    public interface IProductsRepositoryBase
    {
        /// <summary>
        /// Retrieves all products
        /// </summary>
        /// <returns></returns>
        abstract Task<List<Products>> GetListAllProductsAsync(int page = 1, int pageSize = 100);

        /// <summary>
        /// Retrieves all products for a given institution code.
        /// </summary>
        /// <param name="institutionCode"></param>
        /// <returns></returns>
        Task<List<Products>> GetListProductsAsync(string institutionCode);

        /// <summary>
        /// Retrieves products by InstitutionCode and ProductId prefix and ProductName prefix
        /// Optional filters (InStocked, Created, Updated) can be applied.
        /// </summary>
        Task<List<Products>> GetListProductsAsyncByQuery(string institutionCode, string productId, string productName, int? inStocked, string? created, string? updated);

        /// <summary>
        /// Inserts a single product into DB
        /// </summary>
        Task PutProductsAsync(Products product);

        /// <summary>
        /// Updates a single product in DB
        /// </summary>
        Task UpdateProductsAsync(Products product);
    }
}
