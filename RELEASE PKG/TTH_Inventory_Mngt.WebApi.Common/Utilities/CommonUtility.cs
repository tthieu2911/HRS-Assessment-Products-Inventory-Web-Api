using Amazon.DynamoDBv2.Model;

namespace TTH_Inventory_Mngt.WebApi.Common.Utilities
{
    /// <summary>
    /// CommonUtility
    /// </summary>
    public class CommonUtility : ICommonUtility
    {
        /// <summary>
        /// Retrieves a value from a DynamoDB item based on the specified column name and type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="defaultValue"></param>
        /// <returns>T</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static T GetValue<T>(Dictionary<string, AttributeValue> item, string columnName, T defaultValue)
        {
            if (!item.TryGetValue(columnName, out AttributeValue attributeValue))
            {
                // Return the default value if the column is not present in the item
                return defaultValue;
            }

            // Return the corresponding value based on the type
            if (typeof(T) == typeof(string) && attributeValue.S != null)
            {
                return (T)(object)attributeValue.S;
            }
            else if (typeof(T) == typeof(int) && attributeValue.N != null)
            {
                return (T)(object)int.Parse(attributeValue.N);
            }
            else if (typeof(T) == typeof(int?) && attributeValue.N != null)
            {
                return (T)(object)int.Parse(attributeValue.N);
            }
            else if (typeof(T) == typeof(long) && attributeValue.N != null)
            {
                return (T)(object)long.Parse(attributeValue.N);
            }
            else if (typeof(T) == typeof(double) && attributeValue.N != null)
            {
                return (T)(object)double.Parse(attributeValue.N);
            }
            else if (typeof(T) == typeof(bool))
            {
                return (T)(object)attributeValue.BOOL;
            }
            else if (typeof(T) == typeof(List<string>) && attributeValue.SS != null)
            {
                return (T)(object)attributeValue.SS;
            }
            else if (typeof(T) == typeof(List<int>) && attributeValue.NS != null)
            {
                // Convert the list of string numbers to a list of integers
                List<int> intList = new List<int>();
                foreach (var number in attributeValue.NS)
                {
                    intList.Add(int.Parse(number));
                }
                return (T)(object)intList;
            }
            else
            {
                throw new InvalidCastException($"Unable to cast column '{columnName}' to the specified type.");
            }
        }
    }
}
