using Xunit;
using Web;

namespace Web.Tests
{
    public class CalculatorTests
    {
        private readonly Calculator _calc = new Calculator();

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(-1, -2, -3)]
        [InlineData(0, 5, 5)]
        public void Add_ReturnsSum(int a, int b, int expected)
        {
            Assert.Equal(expected, _calc.Add(a, b));
        }

        [Theory]
        [InlineData(5, 3, 2)]
        [InlineData(-1, -2, 1)]
        [InlineData(0, 5, -5)]
        public void Subtract_ReturnsDifference(int a, int b, int expected)
        {
            Assert.Equal(expected, _calc.Subtract(a, b));
        }

        [Theory]
        [InlineData(2, 3, 6)]
        [InlineData(-2, 3, -6)]
        [InlineData(0, 5, 0)]
        public void Multiply_ReturnsProduct(int a, int b, int expected)
        {
            Assert.Equal(expected, _calc.Multiply(a, b));
        }

        [Theory]
        [InlineData(6, 3, 2.0)]
        [InlineData(1, 2, 0.5)]
        [InlineData(-6, 3, -2.0)]
        public void Divide_ReturnsQuotient(int a, int b, double expected)
        {
            double actual = _calc.Divide(a, b);
            Assert.Equal(expected, actual, 10);
        }

        [Fact]
        public void Divide_ByZero_ReturnsInfinity()
        {
            double result = _calc.Divide(1, 0);
            Assert.True(double.IsInfinity(result));
            Assert.Equal(double.PositiveInfinity, result);
        }
    }
}
