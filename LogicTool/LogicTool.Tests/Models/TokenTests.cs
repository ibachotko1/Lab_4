using LogicTool.Core.Enums;
using LogicTool.Core.Models;
using Xunit;

namespace LogicTool.Core.Tests.Models
{
    public class TokenTests
    {
        [Fact]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var token = new Token("x1", TokenType.Variable, 5);

            // Assert
            Assert.Equal("x1", token.Value);
            Assert.Equal(TokenType.Variable, token.Type);
            Assert.Equal(5, token.Position);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var token = new Token("∧", TokenType.Operator, 2);

            // Act
            var result = token.ToString();

            // Assert
            Assert.Contains("∧", result);
            Assert.Contains("Operator", result);
            Assert.Contains("2", result);
        }
    }
}