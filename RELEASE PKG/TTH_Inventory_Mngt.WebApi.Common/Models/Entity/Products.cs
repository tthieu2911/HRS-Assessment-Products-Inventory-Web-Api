using Amazon.DynamoDBv2.DataModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTH_Inventory_Mngt.WebApi.Common.Models
{
    /// <summary>
    /// Represents a product entity that can be stored in both DynamoDB and SQL-based databases.
    /// Supports dual mapping via DynamoDB attributes and EF Core attributes.
    /// </summary>
    [Table("Products")] // EF Core: maps to "Products" table
    [DynamoDBTable("Products")] // DynamoDB: maps to "Products" table
    public class Products
    {
        /// <summary>
        /// Partition key (DynamoDB) and part of composite primary key (EF).
        /// Represents the Institution (organization) to which the product belongs.
        /// </summary>
        [Key] // EF Core
        [Column(Order = 0)] // EF Core: composite PK order
        [DynamoDBHashKey] // DynamoDB
        public string InstitutionCode { get; set; } = string.Empty;

        /// <summary>
        /// Sort key (DynamoDB) and part of composite primary key (EF).
        /// Represents the unique identifier of the product within the institution.
        /// </summary>
        [Key] // EF Core
        [Column(Order = 1)] // EF Core: composite PK order
        [DynamoDBRangeKey] // DynamoDB
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Products name. Required in EF, GSI range key in DynamoDB.
        /// </summary>
        [Required] // EF Core
        [DynamoDBGlobalSecondaryIndexRangeKey] // DynamoDB: GSI for ProductName
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Current stock quantity of the product (nullable).
        /// </summary>
        public int? InStocked { get; set; } = null;

        /// <summary>
        /// Creation timestamp (string formatted "yyyyMMddHHmmss").
        /// </summary>
        public string? Created { get; set; } = null;

        /// <summary>
        /// Last updated timestamp (string formatted "yyyyMMddHHmmss").
        /// </summary>
        public string? Updated { get; set; } = null;
    }
}
