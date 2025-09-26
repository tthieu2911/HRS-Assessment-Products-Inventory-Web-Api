using Microsoft.EntityFrameworkCore;
using TTH_Inventory_Mngt.WebApi.Common.Models;

namespace TTH_Inventory_Mngt.WebApi.DataAccess
{
    public class ProductsRepositoryEF : IProductsRepositoryBase
    {
        private readonly InventoryDbContext _context;

        public ProductsRepositoryEF(InventoryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Get All Products
        /// <summary>
        /// Retrieves all products (with pagination if needed).
        /// </summary>
        public async Task<List<Products>> GetListAllProductsAsync(int page = 1, int pageSize = 100)
        {
            return await _context.Products
                .OrderBy(p => p.InstitutionCode)
                .ThenBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        #endregion

        #region Get Products by Institution
        /// <summary>
        /// Retrieves all products for a given institution.
        /// </summary>
        public async Task<List<Products>> GetListProductsAsync(string institutionCode)
        {
            return await _context.Products
                .Where(p => p.InstitutionCode == institutionCode)
                .OrderBy(p => p.ProductId)
                .ToListAsync();
        }
        #endregion

        #region Get Products by Query
        /// <summary>
        /// Retrieves products by InstitutionCode and ProductName with optional filters.
        /// </summary>
        public async Task<List<Products>> GetListProductsAsyncByQuery(
            string institutionCode,
            string productId,
            string productName,
            int? inStocked = null,
            string? created = null,
            string? updated = null)
        {
            var query = _context.Products.AsQueryable();

            query = query.Where(p => p.InstitutionCode == institutionCode &&
                                     p.ProductId.StartsWith(productId) &&
                                     p.ProductName.StartsWith(productName));

            if (inStocked.HasValue)
                query = query.Where(p => p.InStocked == inStocked);

            if (!string.IsNullOrEmpty(created))
                query = query.Where(p => p.Created != null && p.Created.StartsWith(created));

            if (!string.IsNullOrEmpty(updated))
                query = query.Where(p => p.Updated != null && p.Updated.StartsWith(updated));

            return await query.ToListAsync();
        }
        #endregion

        #region Insert Product
        /// <summary>
        /// Inserts a new product. Throws if already exists.
        /// </summary>
        public async Task PutProductsAsync(Products product)
        {
            var exists = await _context.Products
                .AnyAsync(p => p.InstitutionCode == product.InstitutionCode &&
                               p.ProductId == product.ProductId);

            if (exists)
                throw new InvalidOperationException(" Products already exists.");

            product.Created ??= DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        #endregion

        #region Update Product
        /// <summary>
        /// Updates an existing product. Throws if not exists.
        /// </summary>
        public async Task UpdateProductsAsync(Products product)
        {
            var existing = await _context.Products
                .FirstOrDefaultAsync(p => p.InstitutionCode == product.InstitutionCode &&
                                          p.ProductId == product.ProductId);

            if (existing == null)
                throw new InvalidOperationException(" Products not found.");

            existing.ProductName = string.IsNullOrEmpty(product.ProductName) ? existing.ProductName : product.ProductName;
            existing.InStocked = product.InStocked ?? existing.InStocked;
            existing.Updated = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
