using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetCollection;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MockQueryable;
using Moq;
using System.Linq.Expressions;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.SpotDeferredTrades.Handlers.Queries
{
    public class GetSpotDefferedTradesQueryHandlerTests
    {
        private readonly Mock<IRepository<SpotDeferredTrade, SpotDeferredTradeId>> _repositoryMock;
        private readonly GetSpotDeferredTradesQueryHandler _handler;

        public GetSpotDefferedTradesQueryHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<SpotDeferredTrade, SpotDeferredTradeId>>();
            _handler = new GetSpotDeferredTradesQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenTradesExist()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var metalType = new MetalType(); // Initialize appropriately
            var query = new GetSpotDeferredTradesQuery
            {
                AccountId = accountId,
                MetalType = metalType,                
                PageNumber = 1,
                PageSize = 10
            };

            var spotDeferredTrade = SpotDeferredTrade.Create(
                new HedgingAccountId(accountId),
                "TC123",
                SideType.Buy,
                new DateOnly(2022, 12, 1),
                true
            );

            var spotDeferredTradeItem = SpotDeferredTradeItem.Create(metalType, new Money(1500), new QuantityOunces(10));
            spotDeferredTrade.AddItem(spotDeferredTradeItem);

            var trades = new List<SpotDeferredTrade> { spotDeferredTrade };
            var tradesQueryable = trades.AsQueryable().BuildMock(); // Mock IQueryable

            _repositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(tradesQueryable); // Returns the mocked IQueryable

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal("TC123", result.Items.First().TradeConfirmationNumber); 
            Assert.Equal(metalType, result.Items.First().MetalType); 
            Assert.Equal(1500, result.Items.First().SpotPricePerOz); 
            Assert.Equal(10, result.Items.First().QuantityOz); 
            Assert.Equal(15000, result.Items.First().TotalAmount); 
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyResult_WhenNoTradesExist()
        {
            // Arrange
            var query = new GetSpotDeferredTradesQuery
            {
                AccountId = Guid.NewGuid(),
                MetalType = new MetalType(), // Initialize as needed
                FromDate = DateTime.UtcNow.ConvertUtcToEst(),
                ToDate = DateTime.UtcNow.ConvertUtcToEst(),
                PageNumber = 1,
                PageSize = 10
            };

            _repositoryMock
            .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(Enumerable.Empty<SpotDeferredTrade>().AsQueryable().BuildMock());       

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
    }
}
