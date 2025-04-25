using PreciousMetalsTradingSystem.Domain.DomainServices;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.UnitTests.Domain.Services
{
    public class InventoryPositionValidatorTests
    {
        private readonly InventoryPositionValidator _validator;

        public InventoryPositionValidatorTests()
        {
            _validator = new InventoryPositionValidator();
        }

        [Theory]
        [InlineData(100, 50, true)]
        [InlineData(50, 50, true)]
        [InlineData(50, 100, false)]
        [InlineData(1, 1, true)]
        [InlineData(0, 1, false)]
        public void IsSufficientForSell_WithVariousValues_ReturnsExpectedResult(
            int positionAmount,
            int requestedAmount,
            bool expectedResult)
        {
            // Arrange
            var currentPosition = new PositionQuantityUnits(positionAmount);
            var requestedQuantity = new QuantityUnits(requestedAmount);

            // Act
            var result = _validator.IsSufficientForSell(requestedQuantity, currentPosition);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void IsSufficientForSell_WhenRequestedQuantityIsNull_ThrowsException()
        {
            // Arrange
            var currentPosition = new PositionQuantityUnits(1000);
            QuantityUnits requestedQuantity = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _validator.IsSufficientForSell(requestedQuantity, currentPosition));
        }

        [Fact]
        public void IsSufficientForSell_WhenCurrentPositionIsNull_ThrowsException()
        {
            // Arrange
            PositionQuantityUnits currentPosition = null!;
            var requestedQuantity = new QuantityUnits(500);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                _validator.IsSufficientForSell(requestedQuantity, currentPosition));
        }
    }
}
