using TTH_Inventory_Mngt.WebApi.Common.Models;
using TTH_Inventory_Mngt.WebApi.DataAccess;

namespace TTH_Inventory_Mngt.WebApi.CommonServices
{
    /// <summary>
    /// Service layer for managing product operations.
    /// Acts as a bridge between controller layer (API) and repository layer (DynamoDB).
    /// </summary>
    public class ProductsServices : IProductsServices
    {
        private readonly IProductsRepositoryBase _productsRepository;

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="productsRepository">Repository for DynamoDB product access.</param>
        public ProductsServices(IProductsRepositoryBase productsRepository)
        {
            _productsRepository = productsRepository;   
        }

        /// <summary>
        /// Gets all products.
        /// </summary>
        /// <returns>List of product responses.</returns>
        public async Task<List<ProductsResponse>> GetListAllProductsAsync()
        {
            // Repository fetches data directly from DynamoDB.
            var productsList = await _productsRepository.GetListAllProductsAsync();

            // Map domain model -> response model
            var productsReponseList = productsList.Select(x => new ProductsResponse(x)).ToList();

            return productsReponseList;
        }

        /// <summary>
        /// Gets all products for a given institution.
        /// </summary>
        /// <param name="institutionCode">Request institution code.</param>
        /// <returns>List of product responses.</returns>
        public async Task<List<ProductsResponse>> GetListProductsAsync(string institutionCode)
        {
            // Repository fetches data directly from DynamoDB.
            var productsList = await _productsRepository.GetListProductsAsync(institutionCode);

            // Map domain model -> response model
            var productsReponseList = productsList.Select(x => new ProductsResponse(x)).ToList();

            return productsReponseList;
        }

        /// <summary>
        /// Searches for products
        /// </summary>
        /// <param name="productsRequest">Search conditions (ProductName, ProductId, InStocked, Created, Updated).</param>
        /// <returns>List of matching product responses.</returns>
        public async Task<List<ProductsResponse>> SearchListProductsAsync(ProductsRequest productsRequest)
        {
            // Repository search via DynamoDB GSI + optional filters.
            var productsList =
                await _productsRepository.GetListProductsAsyncByQuery(productsRequest.InstitutionCode,
                                                                              productsRequest.ProductId,
                                                                              productsRequest.ProductName,
                                                                              productsRequest.InStocked,
                                                                              productsRequest.Created,          // yyyyMMdd format
                                                                              productsRequest.Updated);         // yyyyMMdd format

            // Map domain model -> response model
            var productsReponseList = productsList.Select(x => new ProductsResponse(x)).ToList();

            return productsReponseList;
        }

        /// <summary>
        /// Registers (inserts) a new product into DynamoDB.
        /// Uses transaction with "Put" and condition to ensure item does not already exist.
        /// </summary>
        /// <param name="productsRequest">Request containing product details.</param>
        public async Task RegisterProductsAsync(ProductsRequest productsRequest)
        {
            // Convert request -> domain model, then persist
            await _productsRepository.PutProductsAsync(productsRequest.ToModel());
        }

        /// <summary>
        /// Saves (updates) an existing product in DynamoDB.
        /// Uses transaction with "Update" and condition to ensure item already exists.
        /// </summary>
        /// <param name="productsRequest">Request containing updated product details.</param>
        public async Task SaveProductsAsync(ProductsRequest productsRequest)
        {
            // Convert request -> domain model, then update
            await _productsRepository.UpdateProductsAsync(productsRequest.ToModel());
        }
    }
}
