using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using TTH_Inventory_Mngt.WebApi.Common.Models;
using TTH_Inventory_Mngt.WebApi.Common.Utilities;

namespace TTH_Inventory_Mngt.WebApi.DataAccess
{
    /// <summary>
    /// Repository implementation for handling CRUD operations on the Products DynamoDB table.
    /// </summary>
    public class ProductsRepository : IProductsRepositoryBase
    {
        private readonly IAmazonDynamoDB _amazonDynamoDBClient;

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="amazonDynamoDBClient"></param>
        public ProductsRepository(IAmazonDynamoDB amazonDynamoDBClient)
        {
            _amazonDynamoDBClient = amazonDynamoDBClient ?? throw new ArgumentNullException(nameof(amazonDynamoDBClient));
        }

        /// Table name reference
        /// Could be implemeted later for specific Environment
        /// For e.g.: 
        ///     Dev     -> Products-Dev
        ///     Staging -> Products-Stag
        ///     Prod    -> Products
        private string TableName = "Products";

        /// <summary>
        /// Retrieves all products
        /// Method should be used at least as possible to prevent the RequestTimeOut exception or System Hang while handling dataset
        /// To enhance the performance,
        ///     Use DynamoDB Pagination here to retrieve the enough items at one time
        ///     around 100 records per call
        /// For the best performance,
        ///     If "GetAll" method is in need, can consider to combine using
        ///         - API Pagination
        ///         - Streaming to S3 (via Data Pipeline or DDB Stream + Lambda -> S3 for analytics/ bulk export)
        /// </summary>
        /// <returns></returns>
        public async Task<List<Products>> GetListAllProductsAsync(int page = 1, int pageSize = 100)
        {
            List<Products> productsList = new List<Products>();

            // Scan request against the whole table
            var scanRequest = new ScanRequest
            {
                TableName = TableName,
                Select = "ALL_ATTRIBUTES"
            };

            // DynamoDB paginates results, loop until all items are retrieved
            var itemList = new List<Dictionary<string, AttributeValue>>();
            do
            {
                ScanResponse response = await _amazonDynamoDBClient.ScanAsync(scanRequest).ConfigureAwait(false);

                // Accumulate items in memory
                itemList.AddRange(response.Items);

                // Check if more pages exist
                if (response.LastEvaluatedKey != null && response.LastEvaluatedKey.Count > 0)
                {
                    scanRequest.ExclusiveStartKey = response.LastEvaluatedKey;
                }
                else
                {
                    break;
                }
            }
            while (true);

            // Map DynamoDB attributes back to domain model
            foreach (var attribs in itemList)
            {
                Products products = new()
                {
                    InstitutionCode = attribs["InstitutionCode"].S,
                    ProductId = attribs["ProductId"].S,
                    ProductName = attribs["ProductName"].S,
                    InStocked = CommonUtility.GetValue<int?>(attribs, "InStocked", null),
                    Created = CommonUtility.GetValue<string?>(attribs, "Created", null),
                    Updated = CommonUtility.GetValue<string?>(attribs, "Updated", null)
                };
                productsList.Add(products);
            }

            return productsList;
        }

        /// <summary>
        /// Retrieves all products for a given institution code.
        /// </summary>
        /// <param name="institutionCode"></param>
        /// <returns></returns>
        public async Task<List<Products>> GetListProductsAsync(string institutionCode)
        {
            List<Products> productsList = new List<Products>();

            // Query request against the base table, partitioned by InstitutionCode
            QueryRequest queryRequest = new()
            {
                TableName = TableName,
                Select = "ALL_ATTRIBUTES",
                ScanIndexForward = true, // Ascending order by range key (ProductId)
                KeyConditionExpression = $"InstitutionCode = :institutionCode",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":institutionCode", new AttributeValue{ S = $"{institutionCode}" } }
                }
            };

            // DynamoDB paginates results, loop until all items are retrieved
            var itemList = new List<Dictionary<string, AttributeValue>>();
            do
            {
                QueryResponse response = await _amazonDynamoDBClient.QueryAsync(queryRequest).ConfigureAwait(false);

                // Accumulate items in memory
                itemList.AddRange(response.Items);

                // Check if more pages exist
                if (response.LastEvaluatedKey != null && response.LastEvaluatedKey.Count > 0)
                {
                    queryRequest.ExclusiveStartKey = response.LastEvaluatedKey;
                }
                else
                {
                    break;
                }
            }
            while (true);

            // Map DynamoDB attributes back to domain model
            foreach (var attribs in itemList)
            {
                Products products = new()
                {
                    InstitutionCode = attribs["InstitutionCode"].S,
                    ProductId = attribs["ProductId"].S,
                    ProductName = attribs["ProductName"].S,
                    InStocked = CommonUtility.GetValue<int?>(attribs, "InStocked", null),
                    Created = CommonUtility.GetValue<string?>(attribs, "Created", null),
                    Updated = CommonUtility.GetValue<string?>(attribs, "Updated", null)
                };
                productsList.Add(products);
            }

            return productsList;
        }

        /// <summary>
        /// Retrieves products by InstitutionCode and ProductName using the GSI "ProductName-Index".
        /// Optional filters (InStocked, Created, Updated) can be applied.
        /// </summary>
        public async Task<List<Products>> GetListProductsAsyncByQuery(string institutionCode, string productId, string productName, int? inStocked, string? created, string? updated)
        {
            List<Products> productsList = new List<Products>();

            // Query request against the GSI
            QueryRequest queryRequest = new()
            {
                TableName = TableName,
                IndexName = "ProductName-Index", // Must exist in your table definition
                Select = "ALL_ATTRIBUTES",
                ScanIndexForward = true,
                KeyConditionExpression = $"InstitutionCode = :institutionCode AND begins_with(ProductId, :productId) AND begins_with(Productname, :productName)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":institutionCode", new AttributeValue{ S = $"{institutionCode}" } },
                    { ":productId", new AttributeValue{ S = $"{productId}" } },
                    { ":productName", new AttributeValue{ S = $"{productName}" } }
                }
            };

            // Dynamically add filters
            var filterExpressions = new List<string>();
            if (inStocked != null)
            {
                filterExpressions.Add("InStocked = :inStocked");
                queryRequest.ExpressionAttributeValues[":inStocked"] = new AttributeValue { N = inStocked.ToString() };
            }
            if (!string.IsNullOrEmpty(created))
            {
                filterExpressions.Add("begins_with(Created, :created)");
                queryRequest.ExpressionAttributeValues[":created"] = new AttributeValue { S = created };
            }
            if (!string.IsNullOrEmpty(updated))
            {
                filterExpressions.Add("begins_with(Updated, :updated)");
                queryRequest.ExpressionAttributeValues[":updated"] = new AttributeValue { S = updated };
            }
            if (filterExpressions.Count > 0)
            {
                queryRequest.FilterExpression = string.Join(" AND ", filterExpressions);
            }

            // DynamoDB paginates results, loop until all items are retrieved
            var itemList = new List<Dictionary<string, AttributeValue>>();
            do
            {
                QueryResponse response = await _amazonDynamoDBClient.QueryAsync(queryRequest).ConfigureAwait(false);

                itemList.AddRange(response.Items);

                // Check if more pages exist
                if (response.LastEvaluatedKey != null && response.LastEvaluatedKey.Count > 0)
                {
                    queryRequest.ExclusiveStartKey = response.LastEvaluatedKey;
                }
                else
                {
                    break;
                }
            }
            while (true);

            // Map back to entity
            foreach (var attribs in itemList)
            {
                Products products = new Products()
                {
                    InstitutionCode = attribs["InstitutionCode"].S,
                    ProductId = attribs["ProductId"].S,
                    ProductName = attribs["ProductName"].S,
                    InStocked = CommonUtility.GetValue<int?>(attribs, "InStocked", null),
                    Created = CommonUtility.GetValue<string?>(attribs, "Created", null),
                    Updated = CommonUtility.GetValue<string?>(attribs, "Updated", null)
                };
                productsList.Add(products);
            }

            return productsList;
        }

        /// <summary>
        /// Inserts a single product into DynamoDB using a transaction.
        /// Transaction ensures atomicity and ConditionExpression ensures "insert only" (no overwrite).
        /// </summary>
        public async Task PutProductsAsync(Products product)
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "InstitutionCode", new AttributeValue { S = product.InstitutionCode } },
                { "ProductId", new AttributeValue { S = product.ProductId } },
                { "ProductName", new AttributeValue { S = product.ProductName } }
            };

            if (product.InStocked.HasValue)
                item["InStocked"] = new AttributeValue { N = product.InStocked.Value.ToString() };

            if (string.IsNullOrEmpty(product.Created))
                item["Created"] = new AttributeValue { S = DateTime.UtcNow.ToString(Const.FMT_DATE_TIME_DEFAULT) };

            var transactRequest = new TransactWriteItemsRequest
            {
                TransactItems = new List<TransactWriteItem>
                {
                    new TransactWriteItem
                    {
                        Put = new Put
                        {
                            TableName = TableName,
                            Item = item,
                            // Prevent overwriting an existing item
                            ConditionExpression = "attribute_not_exists(InstitutionCode) AND attribute_not_exists(ProductId)"
                        }
                    }
                }
            };

            await _amazonDynamoDBClient.TransactWriteItemsAsync(transactRequest).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates a single product in DynamoDB using a transaction.
        /// ConditionExpression ensures update-only (item must exist).
        /// </summary>
        public async Task UpdateProductsAsync(Products product)
        {
            var updateExprParts = new List<string>();
            var exprValues = new Dictionary<string, AttributeValue>();

            if (!string.IsNullOrEmpty(product.ProductName))
            {
                updateExprParts.Add("ProductName = :productName");
                exprValues[":productName"] = new AttributeValue { S = product.ProductName };
            }

            if (product.InStocked.HasValue)
            {
                updateExprParts.Add("InStocked = :inStocked");
                exprValues[":inStocked"] = new AttributeValue { N = product.InStocked.Value.ToString() };
            }

            // Always update Updated timestamp
            updateExprParts.Add("Updated = :updated");
            exprValues[":updated"] = new AttributeValue { S = DateTime.UtcNow.ToString(Const.FMT_DATE_TIME_DEFAULT) };

            var transactRequest = new TransactWriteItemsRequest
            {
                TransactItems = new List<TransactWriteItem>
                {
                    new TransactWriteItem
                    {
                        Update = new Update
                        {
                            TableName = TableName,
                            Key = new Dictionary<string, AttributeValue>
                            {
                                { "InstitutionCode", new AttributeValue { S = product.InstitutionCode } },
                                { "ProductId", new AttributeValue { S = product.ProductId } }
                            },
                            UpdateExpression = "SET " + string.Join(", ", updateExprParts),
                            ExpressionAttributeValues = exprValues,
                            // Prevent accidental inserts
                            ConditionExpression = "attribute_exists(InstitutionCode) AND attribute_exists(ProductId)"
                        }
                    }
                }
            };

            await _amazonDynamoDBClient.TransactWriteItemsAsync(transactRequest).ConfigureAwait(false);
        }

    }
}
