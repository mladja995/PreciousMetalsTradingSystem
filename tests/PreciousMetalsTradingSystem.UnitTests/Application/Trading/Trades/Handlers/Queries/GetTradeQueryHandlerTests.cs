using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Queries.GetSingle;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Trades.Handlers.Queries
{
    public class GetTradeQueryHandlerTests
    {
        private readonly Mock<IRepository<Trade, TradeId>> _repositoryMock;
        private readonly GetTradeDetailsQueryHandler _handler;

        public GetTradeQueryHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<Trade, TradeId>>();
            _handler = new GetTradeDetailsQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrade_WhenTradeExists()
        {
            //Arrange
            var product1 = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(100.0m), MetalType.XAG, true);
            var product2 = Product.Create("Test Product 2", new SKU("TST-2"), new Weight(1.0m), MetalType.XAU, true);
            var product3 = Product.Create("Test Product 3", new SKU("TST-3"), new Weight(32.148m), MetalType.XAG, true);

            var expectedTrade = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.SLC, new DateOnly(2022, 3, 6), new DateOnly(2022, 3, 8), "Test note 1");
            var item11 = TradeItem.Create(expectedTrade.Side, product1.Id, product1.WeightInOz, new QuantityUnits(12), new Money(54.35m), new Money(56.35m), new Premium(0.35m), new Money(56.35m + 0.35m)).SetProduct(product1);
            var item12 = TradeItem.Create(expectedTrade.Side, product2.Id, product2.WeightInOz, new QuantityUnits(15), new Money(14.55m), new Money(16.25m), new Premium(0.45m), new Money(16.25m + 0.45m)).SetProduct(product2);
            var item13 = TradeItem.Create(expectedTrade.Side, product3.Id, product3.WeightInOz, new QuantityUnits(3), new Money(55.55m), new Money(57.25m), new Premium(0.72m), new Money(57.25m + 0.72m)).SetProduct(product3);
            expectedTrade.AddItem(item11);
            expectedTrade.AddItem(item12);
            expectedTrade.AddItem(item13);

            var notExpectedTrade = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.SLC, new DateOnly(2022, 3, 6), new DateOnly(2022, 3, 8), "Test note 1");
            var item21 = TradeItem.Create(notExpectedTrade.Side, product1.Id, product1.WeightInOz, new QuantityUnits(12), new Money(54.35m), new Money(56.35m), new Premium(0.35m), new Money(56.35m + 0.35m)).SetProduct(product1);
            var item22 = TradeItem.Create(notExpectedTrade.Side, product2.Id, product2.WeightInOz, new QuantityUnits(15), new Money(14.55m), new Money(16.25m), new Premium(0.45m), new Money(16.25m + 0.45m)).SetProduct(product2);
            notExpectedTrade.AddItem(item21);
            notExpectedTrade.AddItem(item22);

            var query = new GetTradeDetailsQuery
            {
                Id = expectedTrade.Id.Value
            };

            var mockData = new List<Trade> { expectedTrade, notExpectedTrade };

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            //Act
            var result = await _handler.Handle(query, It.IsAny<CancellationToken>());

            //Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTrade.Id.Value, result.Id);
            Assert.Equal(expectedTrade.TradeNumber, result.TradeNumber);
            Assert.Equal(expectedTrade.Items.Count, result.Items.Count());
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenTrade_DoesNotExist()
        {
            //Arrange
            var product1 = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(100.0m), MetalType.XAG, true);
            var product2 = Product.Create("Test Product 2", new SKU("TST-2"), new Weight(1.0m), MetalType.XAU, true);
            var product3 = Product.Create("Test Product 3", new SKU("TST-3"), new Weight(32.148m), MetalType.XAG, true);

            var notExpectedTrade = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.SLC, new DateOnly(2022, 3, 6), new DateOnly(2022, 3, 8), "Test note 1");
            var item1 = TradeItem.Create(notExpectedTrade.Side, product1.Id, product1.WeightInOz, new QuantityUnits(12), new Money(54.35m), new Money(56.35m), new Premium(0.35m), new Money(56.35m + 0.35m)).SetProduct(product1);
            var item2 = TradeItem.Create(notExpectedTrade.Side, product2.Id, product2.WeightInOz, new QuantityUnits(15), new Money(14.55m), new Money(16.25m), new Premium(0.45m), new Money(16.25m + 0.45m)).SetProduct(product2);
            var item3 = TradeItem.Create(notExpectedTrade.Side, product3.Id, product3.WeightInOz, new QuantityUnits(3), new Money(55.55m), new Money(57.25m), new Premium(0.72m), new Money(57.25m + 0.72m)).SetProduct(product3);
            notExpectedTrade.AddItem(item1);
            notExpectedTrade.AddItem(item2);
            notExpectedTrade.AddItem(item3);

            var query = new GetTradeDetailsQuery
            {
                Id = Guid.NewGuid(),
            };

            var mockData = new List<Trade> { notExpectedTrade };

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            //Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, It.IsAny<CancellationToken>()));

            var msg = $"Entity {nameof(Trade)} with key '{query.Id}' was not found.";
            Assert.Equal(msg, exception.Message);
        }
    }
}
