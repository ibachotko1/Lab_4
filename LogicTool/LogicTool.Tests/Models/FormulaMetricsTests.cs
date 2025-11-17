using LogicTool.Core.Models;
using Xunit;

namespace LogicTool.Core.Tests.Models
{
    public class FormulaMetricsTests
    {
        [Fact]
        public void Constructor_ShouldCalculateMetricsCorrectly()
        {
            // Arrange
            string dnf = "(x1 ∧ ¬x2) ∨ (x3 ∧ x4)";
            string knf = "(x1 ∨ x2) ∧ (x3 ∨ ¬x4)";

            // Act
            var metrics = new FormulaMetrics(dnf, knf);

            // Assert
            Assert.Equal(8, metrics.LiteralCount); // x1, ¬x2, x3, x4, x1, x2, x3, ¬x4
            Assert.Equal(2, metrics.ConjunctionCount); // 2 ∧ in DNF
            Assert.Equal(2, metrics.DisjunctionCount); // 2 ∨ in KNF
            Assert.Equal(12, metrics.TotalCost); // 8 + 2 + 2
        }

        [Fact]
        public void Constructor_WithEmptyFormulas_ShouldReturnZeroMetrics()
        {
            // Act
            var metrics = new FormulaMetrics("", "");

            // Assert
            Assert.Equal(0, metrics.LiteralCount);
            Assert.Equal(0, metrics.ConjunctionCount);
            Assert.Equal(0, metrics.DisjunctionCount);
            Assert.Equal(0, metrics.TotalCost);
        }

        [Fact]
        public void Constructor_WithConstants_ShouldHandleCorrectly()
        {
            // Act
            var metrics = new FormulaMetrics("0", "1");

            // Assert
            Assert.Equal(0, metrics.LiteralCount);
            Assert.Equal(0, metrics.ConjunctionCount);
            Assert.Equal(0, metrics.DisjunctionCount);
            Assert.Equal(0, metrics.TotalCost);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var metrics = new FormulaMetrics("(x1 ∧ x2)", "(x1 ∨ x2)");

            // Act
            string result = metrics.ToString();

            // Assert
            Assert.Contains("Литералы: 4", result);
            Assert.Contains("Конъюнкции: 1", result);
            Assert.Contains("Дизъюнкции: 1", result);
            Assert.Contains("Общая стоимость: 6", result);
        }

        [Fact]
        public void CountLiterals_ShouldCountNegatedVariables()
        {
            // Arrange
            string formula = "¬x1 ∧ x2 ∨ ¬x3";

            // Act
            var metrics = new FormulaMetrics(formula, "");
            int literalCount = metrics.LiteralCount;

            // Assert
            Assert.Equal(3, literalCount); // ¬x1, x2, ¬x3
        }
    }
}
