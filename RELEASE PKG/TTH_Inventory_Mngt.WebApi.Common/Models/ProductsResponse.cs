using System.ComponentModel.DataAnnotations;

namespace TTH_Inventory_Mngt.WebApi.Common.Models
{
    /// <summary>
    /// Response model returned from the API when fetching a EFProducts.
    /// Maps data from the DynamoDB <see cref="Products"/> entity.
    /// </summary>
    public class ProductsResponse
    {
        /// <summary>
        /// Institution code (required).
        /// </summary>
        [Required]
        public string InstitutionCode { get; set; } = string.Empty;

        /// <summary>
        /// Unique product identifier (required).
        /// </summary>
        [Required]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// EFProducts name.
        /// </summary>
        [Required]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Stock quantity (nullable).
        /// </summary>
        public int? InStocked { get; set; } = null;

        /// <summary>
        /// EFProducts creation timestamp.
        /// </summary>
        public string? Created { get; set; } = null;

        /// <summary>
        /// EFProducts last update timestamp.
        /// </summary>
        public string? Updated { get; set; } = null;

        /// <summary>
        /// Maps a DB <see cref="Products"/> entity model to the response DTO.
        /// </summary>
        public ProductsResponse(Products products)
        {
            InstitutionCode = products.InstitutionCode;
            ProductId = products.ProductId;
            ProductName = products.ProductName;
            InStocked = products.InStocked;
            Created = products.Created;
            Updated = products.Updated;
        }
    }
}
