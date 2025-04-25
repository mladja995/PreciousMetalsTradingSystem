using PreciousMetalsTradingSystem.Domain.DomainServices;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.UnitTests.Domain.Services
{
    public class FinancialBalanceValidatorTests
    {
        private readonly FinancialBalanceValidator _validator;

        public FinancialBalanceValidatorTests()
        {
            _validator = new FinancialBalanceValidator();
        }

        [Theory]
        [InlineData(100, 50, true)]
        [InlineData(50, 50, true)]
        [InlineData(50, 100, false)]
        [InlineData(0, 0, true)]
        [InlineData(0, 0.01, false)]
        public void IsSufficientForDebit_WithVariousValues_ReturnsExpectedResult(
            decimal balanceAmount,
            decimal debitAmount,
            bool expectedResult)
        {
            // Arrange
            var currentBalance = new FinancialBalance(balanceAmount);
            var debit = new Money(debitAmount);

            // Act
            var result = _validator.IsSufficientForDebit(debit, currentBalance);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void IsSufficientForDebit_WhenDebitAmountIsNull_ThrowsException()
        {
            // Arrange
            var currentBalance = new FinancialBalance(1000m);
            Money debitAmount = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _validator.IsSufficientForDebit(debitAmount, currentBalance));
        }

        [Fact]
        public void IsSufficientForDebit_WhenCurrentBalanceIsNull_ThrowsException()
        {
            // Arrange
            FinancialBalance currentBalance = null!;
            var debitAmount = new Money(500m);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _validator.IsSufficientForDebit(debitAmount, currentBalance));
        }
    }
}
