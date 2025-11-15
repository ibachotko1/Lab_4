using LogicTool.Core.Enums;
using Xunit;

namespace LogicTool.Core.Tests.Enums
{
    public class EnumTests
    {
        [Fact]
        public void ComparisonResultType_ShouldHaveCorrectValues()
        {
            Assert.Equal(0, (int)ComparisonResultType.Equivalent);
            Assert.Equal(1, (int)ComparisonResultType.NotEquivalent);
            Assert.Equal(2, (int)ComparisonResultType.Error);
        }

        [Fact]
        public void ComplexityLevel_ShouldHaveCorrectValues()
        {
            Assert.Equal(0, (int)ComplexityLevel.Low);
            Assert.Equal(1, (int)ComplexityLevel.Medium);
            Assert.Equal(2, (int)ComplexityLevel.High);
            Assert.Equal(3, (int)ComplexityLevel.VeryHigh);
            Assert.Equal(4, (int)ComplexityLevel.Critical);
        }

        [Fact]
        public void NormalFormType_ShouldHaveCorrectValues()
        {
            Assert.Equal(0, (int)NormalFormType.DNF);
            Assert.Equal(1, (int)NormalFormType.KNF);
            Assert.Equal(2, (int)NormalFormType.PerfectDNF);
            Assert.Equal(3, (int)NormalFormType.PerfectKNF);
        }

        [Fact]
        public void ErrorSeverity_ShouldHaveCorrectValues()
        {
            Assert.Equal(0, (int)ErrorSeverity.Info);
            Assert.Equal(1, (int)ErrorSeverity.Warning);
            Assert.Equal(2, (int)ErrorSeverity.Error);
            Assert.Equal(3, (int)ErrorSeverity.Critical);
        }

        [Fact]
        public void TokenType_ShouldHaveCorrectValues()
        {
            Assert.Equal(0, (int)TokenType.Variable);
            Assert.Equal(1, (int)TokenType.Operator);
            Assert.Equal(2, (int)TokenType.Constant);
            Assert.Equal(3, (int)TokenType.LeftParenthesis);
            Assert.Equal(4, (int)TokenType.RightParenthesis);
        }

        [Fact]
        public void OperatorType_ShouldHaveCorrectValues()
        {
            Assert.Equal(0, (int)OperatorType.Unary);
            Assert.Equal(1, (int)OperatorType.Binary);
            Assert.Equal(2, (int)OperatorType.Special);
        }
    }
}