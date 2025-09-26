using Amazon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TTH_Inventory_Mngt.WebApi.Common.Models;
using TTH_Inventory_Mngt.WebApi.CommonServices;

namespace TTH_Inventory_Mngt.WebApi.Public.Controllers
{
    /// <summary>
    /// Management controller for handling Products-related operations.
    /// Provides endpoints to retrieve, search, register, and update product data.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class MngtController : ControllerBase
    {
        private readonly IProductsServices _productsServices;

        /// <summary>
        /// Constructor initializes product service dependency 
        /// and registers type mappings for DynamoDB entities.
        /// </summary>
        /// <param name="productsServices">Service for product-related operations</param>
        /// <exception cref="ArgumentNullException">Thrown when productsServices is null</exception>
        public MngtController(IProductsServices productsServices)
        {
            var typeMappings = new Dictionary<Type, string>()
            {
                { typeof(Products), "Products" }
            };

            // Register DynamoDB type mappings for entity classes
            foreach (var pair in typeMappings)
            {
                AWSConfigsDynamoDB.Context.TypeMappings.TryAdd(pair.Key, new Amazon.Util.TypeMapping(pair.Key, pair.Value));
            }

            _productsServices = productsServices ?? throw new ArgumentNullException(nameof(productsServices));
        }

        /// <summary>
        /// Retrieves all products across all institutions.
        /// </summary>
        /// <returns>A list of <see cref="ProductsResponse"/> on success</returns>
        [HttpGet]
        [Description("Get all products across all institutions")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<ProductsResponse>), StatusCodes.Status200OK)]  // Success response
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]          // Bad request
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)] // Server error
        public async Task<ActionResult> GetListAllProductsAsync()
        {
            List<ProductsResponse> responses = [];
            if (!ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, "Invalid request body");
            }

            try
            {
                responses = await _productsServices.GetListAllProductsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Error occurs");
            }
            return Ok(responses);
        }

        /// <summary>
        /// Retrieves products for a specific institution.
        /// </summary>
        /// <param name="institutionCode">10-digit institution code</param>
        /// <returns>A list of <see cref="ProductsResponse"/> on success</returns>
        [HttpGet("{institutionCode}")]
        [Description("Get products for a specific institution")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<ProductsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetListProductsAsync(
            [Required, BindRequired, RegularExpression(@"^[0-9]{10}$")] string institutionCode)
        {
            List<ProductsResponse> responses = [];
            if (!ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, "Invalid request body");
            }

            try
            {
                responses = await _productsServices.GetListProductsAsync(institutionCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Error occurs");
            }
            return Ok(responses);
        }

        /// <summary>
        /// Searches for products in a specific institution based on request criteria.
        /// </summary>
        /// <param name="institutionCode">10-digit institution code</param>
        /// <param name="productsRequest">Search conditions</param>
        /// <returns>A list of <see cref="ProductsResponse"/> that match criteria</returns>
        [HttpPost("search")]
        [Description("Search products within a specific institution")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<ProductsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SearchListProductsAsync(
            [FromBody] ProductsRequest productsRequest)
        {
            List<ProductsResponse> responses = [];
            if (!ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, "Invalid request body");
            }

            try
            {
                responses = await _productsServices.SearchListProductsAsync(productsRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Error occurs");
            }
            return Ok(responses);
        }

        /// <summary>
        /// Registers new products in a specific institution.
        /// </summary>
        /// <param name="institutionCode">10-digit institution code</param>
        /// <param name="productsRequest">EFProducts data to register</param>
        /// <returns>Status 200 if registration succeeded</returns>
        [HttpPut("{institutionCode}")]
        [Description("Register new products")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<ProductsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RegisterProductsAsync(
            [Required, BindRequired, RegularExpression(@"^[0-9]{10}$")] string institutionCode,
            [FromBody] ProductsRequest productsRequest)
        {
            if (!ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, "Invalid request body");
            }

            try
            {
                await _productsServices.RegisterProductsAsync(productsRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Error occurs");
            }
            return Ok();
        }

        /// <summary>
        /// Updates existing products in a specific institution.
        /// </summary>
        /// <param name="institutionCode">10-digit institution code</param>
        /// <param name="productsRequest">Updated product data</param>
        /// <returns>Status 200 if update succeeded</returns>
        [HttpPost("{institutionCode}")]
        [Description("Update products")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<ProductsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SaveProductsAsync(
            [Required, BindRequired, RegularExpression(@"^[0-9]{10}$")] string institutionCode,
            [FromBody] ProductsRequest productsRequest)
        {
            if (!ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, "Invalid request body");
            }

            try
            {
                await _productsServices.SaveProductsAsync(productsRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Error occurs");
            }
            return Ok();
        }
    }
}
