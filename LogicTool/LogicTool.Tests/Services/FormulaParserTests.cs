using LogicTool.Core.Enums;
using LogicTool.Core.Exceptions;
using LogicTool.Core.Models;
using LogicTool.Core.Services;
using System.Collections.Generic;
using Xunit;

namespace LogicTool.Core.Tests.Services
{
    public class FormulaParserTests
    {
        private readonly FormulaParser _parser;

        public FormulaParserTests()
        {
            _parser = new FormulaParser();
        }

        [Fact]
        public void Tokenize_WithSimpleFormula_ShouldReturnCorrectTokens()
        {
            // Act
            var tokens = _parser.Tokenize("x1 ∧ x2");

            // Assert
            Assert.Equal(3, tokens.Count);
            Assert.Equal("x1", tokens[0]);
            Assert.Equal("∧", tokens[1]);
            Assert.Equal("x2", tokens[2]);
        }

        [Fact]
        public void Tokenize_WithComplexFormula_ShouldHandleAllOperators()
        {
            // Act
            var tokens = _parser.Tokenize("¬x1 ∧ (x2 ∨ x3) → x4");

            // Assert
            // ¬x1 разбивается на два токена: "¬" и "x1"
            Assert.Equal(10, tokens.Count); // ¬, x1, ∧, (, x2, ∨, x3, ), →, x4
            Assert.Contains("¬", tokens);
            Assert.Contains("∧", tokens);
            Assert.Contains("∨", tokens);
            Assert.Contains("→", tokens);
        }

        [Fact]
        public void Tokenize_WithSpaces_ShouldIgnoreThem()
        {
            // Act
            var tokens = _parser.Tokenize("  x1   ∧  x2  ");

            // Assert
            Assert.Equal(3, tokens.Count);
            Assert.Equal("x1", tokens[0]);
            Assert.Equal("∧", tokens[1]);
            Assert.Equal("x2", tokens[2]);
        }

        [Fact]
        public void Tokenize_WithInvalidFormula_ShouldThrowException()
        {
            // Act & Assert - несбалансированные скобки должны вызывать исключение
            Assert.Throws<FormulaParseException>(() => _parser.Tokenize("x1 ∧ (x2"));
        }

        [Fact]
        public void ToRPN_WithSimpleExpression_ShouldConvertCorrectly()
        {
            // Arrange
            var tokens = new List<string> { "x1", "∧", "x2" };

            // Act
            var rpn = _parser.ToRPN(tokens);

            // Assert
            Assert.Equal(3, rpn.Count);
            Assert.Equal("x1", rpn[0]);
            Assert.Equal("x2", rpn[1]);
            Assert.Equal("∧", rpn[2]);
        }

        [Fact]
        public void ToRPN_WithParentheses_ShouldRespectPrecedence()
        {
            // Arrange
            var tokens = new List<string> { "(", "x1", "∧", "x2", ")", "∨", "x3" };

            // Act
            var rpn = _parser.ToRPN(tokens);

            // Assert
            Assert.Equal(5, rpn.Count);
            Assert.Equal("x1", rpn[0]);
            Assert.Equal("x2", rpn[1]);
            Assert.Equal("∧", rpn[2]);
            Assert.Equal("x3", rpn[3]);
            Assert.Equal("∨", rpn[4]);
        }

        [Fact]
        public void EvaluateRPN_WithSimpleExpression_ShouldCalculateCorrectly()
        {
            // Arrange
            var rpn = new List<string> { "x1", "x2", "∧" };
            var variables = new Dictionary<string, bool> { { "x1", true }, { "x2", true } };

            // Act
            bool result = _parser.EvaluateRPN(rpn, variables);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EvaluateRPN_WithComplexExpression_ShouldCalculateCorrectly()
        {
            // Arrange
            var rpn = new List<string> { "x1", "¬", "x2", "x3", "∧", "∨" };
            var variables = new Dictionary<string, bool> {
                { "x1", false },
                { "x2", true },
                { "x3", true }
            };

            // Act
            bool result = _parser.EvaluateRPN(rpn, variables);

            // Assert
            Assert.True(result); // ¬false ∨ (true ∧ true) = true ∨ true = true
        }

        [Fact]
        public void TokenizeWithTypes_ShouldReturnTypedTokens()
        {
            // Act
            var tokens = _parser.TokenizeWithTypes("x1 ∧ x2");

            // Assert
            Assert.Equal(3, tokens.Count);
            Assert.Equal(TokenType.Variable, tokens[0].Type);
            Assert.Equal(TokenType.Operator, tokens[1].Type);
            Assert.Equal(TokenType.Variable, tokens[2].Type);
        }

        [Fact]
        public void EvaluateRPN_WithUnknownVariable_ShouldThrowException()
        {
            // Arrange
            var rpn = new List<string> { "x1", "x2", "∧" };
            var variables = new Dictionary<string, bool> { { "x1", true } };

            // Act & Assert
            Assert.Throws<FormulaParseException>(() => _parser.EvaluateRPN(rpn, variables));
        }
    }
}