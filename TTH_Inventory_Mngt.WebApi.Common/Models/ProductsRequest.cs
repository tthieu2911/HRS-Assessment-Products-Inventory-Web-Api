using System.ComponentModel.DataAnnotations;

namespace TTH_Inventory_Mngt.WebApi.Common.Models
{
    /// <summary>
    /// Request model used when creating or updating a EFProducts.
    /// This DTO is received from the API layer.
    /// </summary>
    public class ProductsRequest
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
        /// EFProducts name (optional).
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
        /// Default Constructor
        /// </summary>
        public ProductsRequest() { }

        /// <summary>
        /// Maps the request DTO to the DB Model <see cref="Products"/> entity.
        /// </summary>
        public Products ToModel()
        {
            return new Products()
            {
                InstitutionCode = InstitutionCode,
                ProductId = ProductId,
                ProductName = ProductName,
                InStocked = InStocked,
                Created = Created,
                Updated = Updated
            };
        }

    }
}
