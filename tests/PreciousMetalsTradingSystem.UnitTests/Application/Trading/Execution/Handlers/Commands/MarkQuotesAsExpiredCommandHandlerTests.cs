using System.Linq.Expressions;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.QuoteExpiration;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Execution.Handlers.Commands
{
    public class MarkQuotesAsExpiredCommandHandlerTests
    {
        private readonly Mock<IRepository<TradeQuote, TradeQuoteId>> _tradeQuoteRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private MarkTradeQuotesAsExpiredCommandHandler _handler;

        public MarkQuotesAsExpiredCommandHandlerTests()
        {
            _tradeQuoteRepositoryMock = new Mock<IRepository<TradeQuote, TradeQuoteId>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new MarkTradeQuotesAsExpiredCommandHandler(
                _tradeQuoteRepositoryMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldMarkQuoteAsExpired_WhenCommandIsValid()
        {
            // Arrange
            var product1 = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(100.0m), MetalType.XAG, true);
            var tradeQuote = TradeQuote.Create("ABC-123456", DateTime.UtcNow.AddMinutes(-2), DateTime.UtcNow.AddMinutes(3), SideType.Buy, LocationType.SLC, "testing quote");
            var item1 = TradeQuoteItem.Create(product1, new QuantityUnits(12), new Money(105.75m), new Premium(3.25m), new Money(107.2m));
            tradeQuote.AddItem(item1);
            var mockData = new List<TradeQuote> { tradeQuote };

            var command = new MarkTradeQuotesAsExpiredCommand();

            _tradeQuoteRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<TradeQuote, bool>>>(), It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((mockData, mockData.Count));

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            //Act
            await _handler.Handle(command, CancellationToken.None);

            _tradeQuoteRepositoryMock.Verify(
                repo => repo.GetAllAsync(It.IsAny<Expression<Func<TradeQuote, bool>>>(), It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            //Assert
            Assert.Equal(TradeQuoteStatusType.Expired, tradeQuote.Status);
        }
    }
}
