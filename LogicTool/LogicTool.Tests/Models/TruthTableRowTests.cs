using LogicTool.Core.Models;
using System.Collections.Generic;
using Xunit;

namespace LogicTool.Core.Tests.Models
{
    public class TruthTableRowTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var values = new Dictionary<string, bool> { { "x1", true }, { "x2", false } };

            // Act
            var row = new TruthTableRow(values, true);

            // Assert
            Assert.Equal(2, row.Values.Count);
            Assert.True(row.Values["x1"]);
            Assert.False(row.Values["x2"]);
            Assert.True(row.Result);
        }

        [Fact]
        public void Matches_ShouldReturnTrueForIdenticalValues()
        {
            // Arrange
            var values = new Dictionary<string, bool> { { "x1", true }, { "x2", false } };
            var row = new TruthTableRow(values, true);
            var testValues = new Dictionary<string, bool> { { "x1", true }, { "x2", false } };

            // Act
            bool matches = row.Matches(testValues);

            // Assert
            Assert.True(matches);
        }

        [Fact]
        public void Matches_ShouldReturnFalseForDifferentValues()
        {
            // Arrange
            var values = new Dictionary<string, bool> { { "x1", true }, { "x2", false } };
            var row = new TruthTableRow(values, true);
            var testValues = new Dictionary<string, bool> { { "x1", true }, { "x2", true } };

            // Act
            bool matches = row.Matches(testValues);

            // Assert
            Assert.False(matches);
        }

        [Fact]
        public void Matches_ShouldReturnTrueForMissingVariable_WhenRowHasMoreVariables()
        {
            // Arrange
            var values = new Dictionary<string, bool> { { "x1", true }, { "x2", false } };
            var row = new TruthTableRow(values, true);
            var testValues = new Dictionary<string, bool> { { "x1", true } };

            // Act
            bool matches = row.Matches(testValues);

            // Assert
            // Метод Matches проверяет только наличие и совпадение переменных из testValues
            // Если в row есть дополнительные переменные - это не считается несовпадением
            Assert.True(matches);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var values = new Dictionary<string, bool> { { "x1", true }, { "x2", false } };
            var row = new TruthTableRow(values, true);

            // Act
            string result = row.ToString();

            // Assert
            Assert.Contains("x1=True", result);
            Assert.Contains("x2=False", result);
            Assert.Contains("→ True", result);
        }
    }
}