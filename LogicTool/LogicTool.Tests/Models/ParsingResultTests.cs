using LogicTool.Core.Enums;
using LogicTool.Core.Models;
using System.Collections.Generic;
using Xunit;

namespace LogicTool.Core.Tests.Models
{
    public class ParsingResultTests
    {
        [Fact]
        public void SuccessConstructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var tokens = new List<Token> { new Token("x1", TokenType.Variable, 0) };
            var rpn = new List<string> { "x1" };

            // Act
            var result = new ParsingResult("x1", tokens, rpn);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("x1", result.Formula);
            Assert.Single(result.Tokens);
            Assert.Single(result.RPN);
            Assert.Empty(result.ErrorMessage);
            Assert.Equal(ErrorSeverity.Info, result.ErrorSeverity);
        }

        [Fact]
        public void ErrorConstructor_ShouldSetPropertiesCorrectly()
        {
            // Act
            var result = new ParsingResult("invalid", "Syntax error", ErrorSeverity.Error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("invalid", result.Formula);
            Assert.Equal("Syntax error", result.ErrorMessage);
            Assert.Equal(ErrorSeverity.Error, result.ErrorSeverity);
            Assert.Empty(result.Tokens);
            Assert.Empty(result.RPN);
        }

        [Fact]
        public void SuccessStaticMethod_ShouldCreateSuccessResult()
        {
            // Arrange
            var tokens = new List<Token>();
            var rpn = new List<string>();

            // Act
            var result = ParsingResult.Success("test", tokens, rpn);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("test", result.Formula);
        }

        [Fact]
        public void ErrorStaticMethod_ShouldCreateErrorResult()
        {
            // Act
            var result = ParsingResult.Error("test", "Error message");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("test", result.Formula);
            Assert.Equal("Error message", result.ErrorMessage);
        }

        [Fact]
        public void ToString_Success_ShouldReturnSuccessMessage()
        {
            // Arrange
            var result = ParsingResult.Success("x1", new List<Token>(), new List<string> { "x1" });

            // Act
            var str = result.ToString();

            // Assert
            Assert.Contains("Успешный парсинг", str);
            Assert.Contains("x1", str);
        }

        [Fact]
        public void ToString_Error_ShouldReturnErrorMessage()
        {
            // Arrange
            var result = ParsingResult.Error("invalid", "Test error");

            // Act
            var str = result.ToString();

            // Assert
            Assert.Contains("Ошибка парсинга", str);
            Assert.Contains("Test error", str);
        }
    }
}