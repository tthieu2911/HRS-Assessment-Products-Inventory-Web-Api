using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using TTH_Inventory_Mngt.WebApi.Common.Utilities;
using Xunit;

namespace TTH_Inventory_Mngt.WebApi.Common.Tests
{
    public class CommonUtilityTests
    {
        #region Test GetValue
        [Fact]
        public void GetValue_ShouldReturnString_WhenStringColumnExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Name",
                    new AttributeValue { S = "John" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<string>(item, "Name", string.Empty);

            // Assert
            Assert.Equal("John", result);
        }

        [Fact]
        public void GetValue_ShouldReturnString_WhenStringColumnNotExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Name1",
                    new AttributeValue { S = "John" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<string>(item, "Name", string.Empty);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void GetValue_ShouldReturnInt_WhenIntColumnExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Age",
                    new AttributeValue { N = "30" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<int>(item, "Age", 0);

            // Assert
            Assert.Equal(30, result);
        }

        [Fact]
        public void GetNullAbleIntValue_ShouldReturnInt_WhenIntColumnExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Age",
                    new AttributeValue { N = "30" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<int?>(item, "Age", 0);

            // Assert
            Assert.Equal(30, result);
        }

        [Fact]
        public void GetValue_ShouldReturnInt_WhenIntColumnNotExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Age1",
                    new AttributeValue { N = "30" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<int>(item, "Age", 0);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetNullAbleIntValue_ShouldReturnNull_WhenIntColumnNotExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Age1",
                    new AttributeValue { N = "30" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<int?>(item, "Age", null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetValue_ShouldReturnBool_WhenBoolColumnExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "IsActive",
                    new AttributeValue { BOOL = true }
                },
            };

            // Act
            var result = CommonUtility.GetValue<bool>(item, "IsActive", true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetValue_ShouldReturnBool_WhenBoolColumnNotExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "IsActive1",
                    new AttributeValue { BOOL = true }
                },
            };

            // Act
            var result = CommonUtility.GetValue<bool>(item, "IsActive", true);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetValue_ShouldReturnListOfStrings_WhenSSColumnExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Tags",
                    new AttributeValue
                    {
                        SS = new List<string> { "tag1", "tag2" },
                    }
                },
            };

            // Act
            var result = CommonUtility.GetValue<List<string>>(item, "Tags", []);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("tag1", result);
            Assert.Contains("tag2", result);
        }

        [Fact]
        public void GetValue_ShouldReturnListOfStrings_WhenSSColumnNotExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Tags1",
                    new AttributeValue
                    {
                        SS = new List<string> { "tag1", "tag2" },
                    }
                },
            };

            // Act
            var result = CommonUtility.GetValue<List<string>>(item, "Tags", []);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetValue_ShouldReturnLong_WhenLongColumnExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "TransactionId",
                    new AttributeValue { N = "123456789012345" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<long>(item, "TransactionId", 123456789012345);

            // Assert
            Assert.Equal(123456789012345L, result);
        }

        [Fact]
        public void GetValue_ShouldReturnLong_WhenLongColumnNotExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "TransactionId1",
                    new AttributeValue { N = "123456789012345" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<long>(item, "TransactionId", 0);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetValue_ShouldReturnDouble_WhenDoubleColumnExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Price",
                    new AttributeValue { N = "99.99" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<double>(item, "Price", 1.01);

            // Assert
            Assert.Equal(99.99, result, 2); // 2 decimal precision
        }

        [Fact]
        public void GetValue_ShouldReturnDouble_WhenDoubleColumnNotExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Price1",
                    new AttributeValue { N = "99.99" }
                },
            };

            // Act
            var result = CommonUtility.GetValue<double>(item, "Price", 1.01);

            // Assert
            Assert.Equal(1.01, result, 2); // 2 decimal precision
        }

        [Fact]
        public void GetValue_ShouldReturnListOfInts_WhenNSColumnExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Scores",
                    new AttributeValue
                    {
                        NS = new List<string> { "85", "90", "95" },
                    }
                },
            };

            // Act
            var result = CommonUtility.GetValue<List<int>>(item, "Scores", []);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(85, result);
            Assert.Contains(90, result);
            Assert.Contains(95, result);
        }

        [Fact]
        public void GetValue_ShouldReturnListOfInts_WhenNSColumnNotExists()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Scores1",
                    new AttributeValue
                    {
                        NS = new List<string> { "85", "90", "95" },
                    }
                },
            };

            // Act
            var result = CommonUtility.GetValue<List<int>>(item, "Scores", []);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetValue_ShouldThrowInvalidCastException_WhenTypeMismatch()
        {
            // Arrange
            var item = new Dictionary<string, AttributeValue>
            {
                {
                    "Age",
                    new AttributeValue { S = "30" }
                },
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidCastException>(() => CommonUtility.GetValue<int>(item, "Age", 0));

            Assert.Equal("Unable to cast column 'Age' to the specified type.", exception.Message);
        }
        #endregion Test GetValue
    }
}