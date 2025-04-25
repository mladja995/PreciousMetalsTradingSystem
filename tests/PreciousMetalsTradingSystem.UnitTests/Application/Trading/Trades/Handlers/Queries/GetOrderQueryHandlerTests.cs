using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Queries.GetOrder;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Trades.Handlers.Queries
{
    public class GetOrderQueryHandlerTests
    {

        private readonly Mock<IRepository<Trade, TradeId>> _repositoryMock;
        private readonly GetOrderQueryHandler _handler;

        public GetOrderQueryHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<Trade, TradeId>>();
            _handler = new GetOrderQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnClientTrade_WhenTradeExists()
        {
            // Arrange

            var tradeDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var financialSettleOn = tradeDate.AddDays(5);

            var trade =Trade.Create(
                tradeType: TradeType.ClientTrade,
                sideType: SideType.Buy,
                locationType: LocationType.SLC,
                tradeDate: tradeDate,
                financialsSettleOn: financialSettleOn,
                note: "Test Trade"
            );

            var product = Product.Create(
            name: "Gold Bar",
            sku: new SKU("PROD123"),
            weightInOz: new Weight(1.5m),
            metalType: MetalType.XAU,
            isAvailable: true
            );

            var tradeItem = TradeItem.Create(
            tradeSide: SideType.Buy,
            productId: new ProductId(Guid.NewGuid()),
            productWeightInOz: product.WeightInOz,
            quantityUnits: new QuantityUnits(10),
            spotPricePerOz: new Money(1800),
            tradePricePerOz: new Money(1850),
            premiumPerOz: new Premium(50),
            effectivePricePerOz: new Money(1900)
            ).SetProduct(product);

            trade.AddItem(tradeItem);

            _repositoryMock
            .Setup(repo => repo.StartQuery(true, true))
            .Returns(new[] { trade }.AsQueryable().BuildMock());

            var query = new GetOrderQuery { OrderNumber = trade.TradeNumber };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(query.OrderNumber, result.TradeNumber);
            Assert.Single(result.Items);
            Assert.Equal("PROD123", result.Items.First().ProductSKU);
        }

        [Fact]
        public async Task Handle_ShouldNotReturnClientTrade_WhenTradeDoesNotExists()
        {
            // Arrange
            var orderNumber = "080921-H1060";

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, true))
                .Returns(Enumerable.Empty<Trade>().AsQueryable().BuildMock()); // Prazna lista

            var query = new GetOrderQuery { OrderNumber = orderNumber };

            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));

            // Assert
            Assert.Equal(nameof(Trade), exception.Message); 
            Assert.Equal(orderNumber, exception.Code); 
        }

    }
}
