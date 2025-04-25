using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetSummary;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Microsoft.Identity.Client;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.SpotDeferredTrades.Handlers.Queries
{
    public class GetSpotDefferedTradesSummaryQueryHandlerTests
    {
        private readonly Mock<IRepository<SpotDeferredTradeItem, SpotDeferredTradeItemId>> _repositoryMock;
        private readonly GetSpotDeferredTradesSummaryQueryHandler _handler;

        public GetSpotDefferedTradesSummaryQueryHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<SpotDeferredTradeItem, SpotDeferredTradeItemId>>();
            _handler = new GetSpotDeferredTradesSummaryQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenEntitiesExist()
        {
            // Arrange
            var query = new GetSpotDeferredTradesSummaryQuery
            {
                AccountId = new Guid("1EB3F5C3-7890-4FEF-BE70-5F311C6F9F0C")
            };

            var trade1 = SpotDeferredTrade.Create(new HedgingAccountId(query.AccountId), "TC-123", SideType.Buy, new DateOnly(2022, 12, 1), true);
            var item1 = SpotDeferredTradeItem.Create(MetalType.XAU, new Money(54.05m), new QuantityOunces(32.148m));
            item1.SetTrade(trade1);

            var trade2 = SpotDeferredTrade.Create(new HedgingAccountId(query.AccountId), "TC-167", SideType.Buy, new DateOnly(2023, 2, 10), true);
            var item2 = SpotDeferredTradeItem.Create(MetalType.XAU, new Money(54.35m), new QuantityOunces(96.444m));
            item2.SetTrade(trade2);

            var trade3 = SpotDeferredTrade.Create(new HedgingAccountId(query.AccountId), "TC-564", SideType.Sell, new DateOnly(2023, 3, 5), true);
            var item3 = SpotDeferredTradeItem.Create(MetalType.XAU, new Money(53.65m), new QuantityOunces(64.296m));
            item3.SetTrade(trade3);

            var trade4 = SpotDeferredTrade.Create(new HedgingAccountId(query.AccountId), "TC-812", SideType.Buy, new DateOnly(2023, 1, 15), true);
            var item4 = SpotDeferredTradeItem.Create(MetalType.XAG, new Money(11m), new QuantityOunces(10m));
            item4.SetTrade(trade4);

            var trade5 = SpotDeferredTrade.Create(new HedgingAccountId(query.AccountId), "TC-978", SideType.Sell, new DateOnly(2023, 2, 20), true);
            var item5 = SpotDeferredTradeItem.Create(MetalType.XAG, new Money(12m), new QuantityOunces(4m));
            item5.SetTrade(trade5);

            var mockData = new List<SpotDeferredTradeItem> { item1, item2, item3, item4, item5 };

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(new DateOnly(2023, 3, 5), result.Items.First(x => x.MetalType == MetalType.XAU).LastHedgingDate);
            Assert.Equal(3529.85m, result.Items.First(x => x.MetalType == MetalType.XAU).NetAmount);
            Assert.Equal(new DateOnly(2023, 2, 20), result.Items.First(x => x.MetalType == MetalType.XAG).LastHedgingDate);
            Assert.Equal(62m, result.Items.First(x => x.MetalType == MetalType.XAG).NetAmount);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyResult_WhenNoEntitiesExist()
        {
            // Arrange
            var query = new GetSpotDeferredTradesSummaryQuery
            {
                AccountId = new Guid("1EB3F5C3-7890-4FEF-BE70-5F311C6F9F0C")
            };

            var mockData = new List<SpotDeferredTradeItem>();

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.Items?.Count ?? 0);
        }
    }
}
