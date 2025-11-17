using LogicTool.Business.Services;
using LogicTool.Core.Models;
using System;
using Xunit;

namespace LogicTool.Business.Tests.Services
{
    public class BooleanFunctionServiceTests
    {
        private readonly BooleanFunctionService _service;

        public BooleanFunctionServiceTests()
        {
            _service = new BooleanFunctionService();
        }

        [Fact]
        public void CreateFromNumber_WithValidParameters_ShouldReturnFunction()
        {
            // Act
            var function = _service.CreateFromNumber(2, 3);

            // Assert
            Assert.NotNull(function);
            Assert.Equal(2, function.VariableCount);
            Assert.Equal(3, function.FunctionNumber);
        }

        [Fact]
        public void CreateFromNumber_WithInvalidVariableCount_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.CreateFromNumber(0, 1));
            Assert.Throws<ArgumentException>(() => _service.CreateFromNumber(15, 1));
        }

        [Fact]
        public void CreateFromFormula_WithValidFormula_ShouldReturnFunction()
        {
            // Act
            var function = _service.CreateFromFormula("x1 ∧ x2");

            // Assert
            Assert.NotNull(function);
            Assert.Equal("x1 ∧ x2", function.OriginalFormula);
        }

        [Fact]
        public void CreateFromFormula_WithEmptyFormula_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.CreateFromFormula(""));
        }

        [Fact]
        public void CalculateMetrics_ShouldReturnValidMetrics()
        {
            // Act
            var metrics = _service.CalculateMetrics("x1 ∧ x2");

            // Assert
            Assert.NotNull(metrics);
            Assert.True(metrics.LiteralCount > 0);
        }

        [Fact]
        public void ValidateFormula_WithValidFormula_ShouldReturnSuccess()
        {
            // Act
            var result = _service.ValidateFormula("x1 ∧ x2");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Tokens);
        }

        [Fact]
        public void ValidateFormula_WithInvalidFormula_ShouldReturnError()
        {
            // Act
            var result = _service.ValidateFormula("x1 ∧ (x2"); // Несбалансированные скобки

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.ErrorMessage);
        }
    }
}