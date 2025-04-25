using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Activity.Queries.GetCollection;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Database;
using MockQueryable;
using Moq;
using System.Linq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Trades.Handlers.Queries
{
    public class GetTradesQueryHandlerTests
    {
        private readonly Mock<IRepository<Trade, TradeId>> _repositoryMock;
        private readonly GetActivityQueryHandler _handler;

        public GetTradesQueryHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<Trade, TradeId>>();
            _handler = new GetActivityQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenEntitiesExist()
        {
            // Arrange
            var query = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                PageNumber = 1,
                PageSize = 10,
            };

            var mockData = _MockTrades;

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(9, result.Items.Count);
            Assert.Equal(5, result.Items.DistinctBy(x => x.TradeNumber).Count());
            Assert.Equal(3, result.Items.DistinctBy(x => x.ProductSKU).Count());
            Assert.Equal(2, result.Items.DistinctBy(x => x.Location).Count());
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyResult_WhenNoEntitiesExist()
        {
            // Arrange
            var query = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                PageNumber = 1,
                PageSize = 10,
            };

            var mockData = new List<Trade>();

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByLocation()
        {
            // Arrange
            var query = new GetActivityQuery
            {
                //FromDate
                //ToDate
                Location = LocationType.SLC,
                //ProductSKU
                PageNumber = 1,
                PageSize = 10,
            };

            var mockData = _MockTrades;

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6, result.Items.Count);
            Assert.Equal(3, result.Items.DistinctBy(x => x.TradeNumber).Count());
            Assert.Equal(3, result.Items.DistinctBy(x => x.ProductSKU).Count());
            Assert.Single(result.Items.DistinctBy(x => x.Location));
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByProductSKU()
        {
            // Arrange
            var query = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                ProductSKU = "TST-2",
                PageNumber = 1,
                PageSize = 10,
            };

            var mockData = _MockTrades;

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Items.Count);
            Assert.Equal(3, result.Items.DistinctBy(x => x.TradeNumber).Count());
            Assert.Single(result.Items.DistinctBy(x => x.ProductSKU));
            Assert.Equal(2, result.Items.DistinctBy(x => x.Location).Count());
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByFromDate()
        {
            // Arrange
            var query = new GetActivityQuery
            {
                FromDate = new DateTime(2022, 3, 12),
                //ToDate
                //Location
                //ProductSKU
                PageNumber = 1,
                PageSize = 10,
            };

            var mockData = _MockTrades;

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Items.Count);
            Assert.Equal(3, result.Items.DistinctBy(x => x.TradeNumber).Count());
            Assert.Equal(3, result.Items.DistinctBy(x => x.ProductSKU).Count());
            Assert.Equal(2, result.Items.DistinctBy(x => x.Location).Count());
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByTradeNumber()
        {
            // Arrange
            var query = new GetActivityQuery
            {
                TradeNumber = "SLC",
                PageNumber = 1,
                PageSize = 10,
            };

            var mockData = _MockTrades;

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6, result.Items.Count);
            Assert.Equal(3, result.Items.DistinctBy(x => x.TradeNumber).Count());
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByLocationAndProductSKU()
        {
            // Arrange
            var query = new GetActivityQuery
            {
                //FromDate
                //ToDate
                Location = LocationType.SLC,
                ProductSKU = "TST-2",
                PageNumber = 1,
                PageSize = 10,
            };

            var mockData = _MockTrades;

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.Items.DistinctBy(x => x.TradeNumber).Count());
            Assert.Single(result.Items.DistinctBy(x => x.ProductSKU));
            Assert.Single(result.Items.DistinctBy(x => x.Location));
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyResult_WhenFilteredByProductSKU()
        {
            // Arrange
            var query = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                ProductSKU = "TST-0",
                PageNumber = 1,
                PageSize = 10,
            };

            var mockData = new List<Trade>();

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByIsPositionSettledTrue()
        {
            // Arrange
            var request = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                IsPositionSettled = true, 
                PageNumber = 1,
                PageSize = 10 
            };

            var trades = _MockTrades;
            var expectedTradingActivityItems = _MockTrades.Where(x => x.IsPositionSettled).Sum(t => t.Items.Count);

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(trades.AsQueryable().BuildMock());

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(request, cancellationToken);

            // Assert
            Assert.Equal(expectedTradingActivityItems, result.TotalCount);
            Assert.All(result.Items, item => Assert.True(item.IsPositionSettled));
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByIsPositionSettledFalse()
        {
            // Arrange
            var request = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                IsPositionSettled = false,
                PageNumber = 1,
                PageSize = 10
            };

            var trades = _MockTrades;
            var expectedTradingActivityItems = _MockTrades.Where(x => !x.IsPositionSettled).Sum(t => t.Items.Count);

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(trades.AsQueryable().BuildMock());

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(request, cancellationToken);

            // Assert
            Assert.Equal(expectedTradingActivityItems, result.TotalCount);
            Assert.All(result.Items, item => Assert.False(item.IsPositionSettled));
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredBySideTypeBuy()
        {
            // Arrange
            var request = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                //IsPositionSettled
                SideType = SideType.Buy,
                PageNumber = 1,
                PageSize = 10
            };

            var trades = _MockTrades;
            var expectedTradingActivityItems = _MockTrades.Where(x => x.Side == SideType.Buy).Sum(t => t.Items.Count);

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(trades.AsQueryable().BuildMock());

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(request, cancellationToken);

            // Assert
            Assert.Equal(expectedTradingActivityItems, result.TotalCount);
            Assert.All(result.Items, item => Assert.Equal(SideType.Buy.ToString(), item.SideType.ToString()));
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredBySideTypeSell()
        {
            // Arrange
            var request = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                //IsPositionSettled
                SideType = SideType.Sell,
                PageNumber = 1,
                PageSize = 10
            };

            var trades = _MockTrades;
            var expectedTradingActivityItems = _MockTrades.Where(x => x.Side == SideType.Sell).Sum(t => t.Items.Count);

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(trades.AsQueryable().BuildMock());
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(request, cancellationToken);

            // Assert
            Assert.Equal(expectedTradingActivityItems, result.TotalCount);
            Assert.All(result.Items, item => Assert.Equal(SideType.Sell.ToString(), item.SideType.ToString()));
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByFromLastUpdatedUtc()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var trades = _MockTrades;
            var queryFromLastUpdatedUtc = trades.OrderBy(x => x.LastUpdatedOnUtc).Select(y => y.LastUpdatedOnUtc).Take(2).LastOrDefault();
            var request = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                //IsPositionSettled
                //SideType
                FromLastUpdatedUtc = queryFromLastUpdatedUtc,
                PageNumber = 1,
                PageSize = 10
            };

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(trades.AsQueryable().BuildMock());

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(request, cancellationToken);

            // Assert
            Assert.Equal(6, result.TotalCount);
            Assert.All(result.Items, item => Assert.True(item.LastUpdatedOnUtc >= queryFromLastUpdatedUtc));
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByToLastUpdatedUtc()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var trades = _MockTrades;
            var queryToLastUpdatedUtc = trades.OrderBy(x => x.LastUpdatedOnUtc).Select(y => y.LastUpdatedOnUtc).Take(2).LastOrDefault();
            var request = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                //IsPositionSettled
                //SideType
                //FromLastUpdatedUtc = queryFromLastUpdatedUtc,
                ToLastUpdatedUtc = queryToLastUpdatedUtc,
                PageNumber = 1,
                PageSize = 10
            };

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(trades.AsQueryable().BuildMock());

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(request, cancellationToken);

            // Assert
            Assert.Equal(5, result.TotalCount);
            Assert.All(result.Items, item => Assert.True(item.LastUpdatedOnUtc <= queryToLastUpdatedUtc));
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByFromFinancialSettleOnDate()
        {
            // Arrange            
            var request = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                //IsPositionSettled
                //SideType
                //FromLastUpdatedUtc = queryFromLastUpdatedUtc,
                //ToLastUpdatedUtc = queryToLastUpdatedUtc,
                FromFinancialSettleOnDate = new DateTime(2022, 3, 13),
                PageNumber = 1,
                PageSize = 10
            };

            var fromFinancialSettledDate = DateOnly.FromDateTime(request.FromFinancialSettleOnDate ?? DateTime.MinValue);
            var expectedTradingActivityItems = _MockTrades.Where(x => x.FinancialSettleOn >= fromFinancialSettledDate).Sum(t => t.Items.Count);

            var trades = _MockTrades;

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(trades.AsQueryable().BuildMock());

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(request, cancellationToken);

            // Assert
            Assert.Equal(expectedTradingActivityItems, result.TotalCount);
            Assert.All(result.Items, item => Assert.True(item.FinancialSettledOn >= fromFinancialSettledDate));
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectResult_WhenFilteredByToFinancialSettleOnDate()
        {
            // Arrange
            var request = new GetActivityQuery
            {
                //FromDate
                //ToDate
                //Location
                //ProductSKU
                //IsPositionSettled
                //SideType
                //FromLastUpdatedUtc 
                //ToLastUpdatedUtc 
                //FromFinancialSettleOnDate 
                ToFinancialSettleOnDate = new DateTime(2022, 3, 13),
                PageNumber = 1,
                PageSize = 10
            };

            var toFinancialSettledDate = DateOnly.FromDateTime(request.ToFinancialSettleOnDate ?? DateTime.MaxValue);
            var expectedTradingActivityItems = _MockTrades.Where(x => x.FinancialSettleOn <= toFinancialSettledDate).Sum(t => t.Items.Count);
            var trades = _MockTrades;

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(trades.AsQueryable().BuildMock());

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _handler.Handle(request, cancellationToken);

            // Assert
            Assert.Equal(expectedTradingActivityItems, result.TotalCount);
            Assert.All(result.Items, item => Assert.True(item.FinancialSettledOn <= toFinancialSettledDate));
        }

        // Mock Trades
        List<Trade> _MockTrades
        {
            get
            {
                var product1 = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(100.0m), MetalType.XAG, true);
                var product2 = Product.Create("Test Product 2", new SKU("TST-2"), new Weight(1.0m), MetalType.XAU, true);
                var product3 = Product.Create("Test Product 3", new SKU("TST-3"), new Weight(32.148m), MetalType.XAG, true);

                var trade1 = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.SLC, new DateOnly(2022, 3, 6), new DateOnly(2022, 3, 8), "Test note 1");
                trade1.MarkAsSettled();
                var item11 = TradeItem.Create(trade1.Side, product1.Id, product1.WeightInOz, new QuantityUnits(12), new Money(54.35m), new Money(56.35m), new Premium(0.35m), new Money(56.35m + 0.35m)).SetProduct(product1);
                var item12 = TradeItem.Create(trade1.Side, product2.Id, product2.WeightInOz, new QuantityUnits(15), new Money(14.55m), new Money(16.25m), new Premium(0.45m), new Money(16.25m + 0.45m)).SetProduct(product2);
                var item13 = TradeItem.Create(trade1.Side, product3.Id, product3.WeightInOz, new QuantityUnits(3), new Money(55.55m), new Money(57.25m), new Premium(0.72m), new Money(57.25m + 0.72m)).SetProduct(product3);
                trade1.AddItem(item11);
                trade1.AddItem(item12);
                trade1.AddItem(item13);

                var trade2 = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.NY, new DateOnly(2022, 3, 11), new DateOnly(2022, 3, 13), "Test note 2");
                var item21 = TradeItem.Create(trade2.Side, product1.Id, product1.WeightInOz, new QuantityUnits(12), new Money(54.35m), new Money(56.35m), new Premium(0.35m), new Money(56.35m + 0.35m)).SetProduct(product1);
                var item22 = TradeItem.Create(trade2.Side, product2.Id, product2.WeightInOz, new QuantityUnits(8), new Money(14.40m), new Money(16.85m), new Premium(0.34m), new Money(16.85m + 0.34m)).SetProduct(product2);
                trade2.AddItem(item21);
                trade2.AddItem(item22);

                var trade3 = Trade.Create(TradeType.ClientTrade, SideType.Sell, LocationType.NY, new DateOnly(2022, 3, 15), new DateOnly(2022, 3, 17), "Test note 3");
                var item31 = TradeItem.Create(trade3.Side, product1.Id, product1.WeightInOz, new QuantityUnits(3), new Money(54.03m), new Money(56.65m), new Premium(0.30m), new Money(56.65m + 0.30m)).SetProduct(product1);
                trade3.AddItem(item31);

                var trade4 = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.SLC, new DateOnly(2022, 3, 17), new DateOnly(2022, 3, 19), "Test note 4");
                var item41 = TradeItem.Create(trade4.Side, product1.Id, product1.WeightInOz, new QuantityUnits(7), new Money(54.90m), new Money(56.25m), new Premium(0.39m), new Money(56.25m + 0.39m)).SetProduct(product1);
                var item42 = TradeItem.Create(trade4.Side, product2.Id, product2.WeightInOz, new QuantityUnits(21), new Money(14.40m), new Money(16.85m), new Premium(0.34m), new Money(16.85m + 0.34m)).SetProduct(product2);
                trade4.AddItem(item41);
                trade4.AddItem(item42);

                var trade5 = Trade.Create(TradeType.ClientTrade, SideType.Sell, LocationType.SLC, new DateOnly(2022, 3, 18), new DateOnly(2022, 3, 20), "Test note 5");
                var item51 = TradeItem.Create(trade5.Side, product3.Id, product3.WeightInOz, new QuantityUnits(10), new Money(54.10m), new Money(56.75m), new Premium(0.36m), new Money(56.75m + 0.36m)).SetProduct(product3);
                trade5.AddItem(item51);

                return new List<Trade> { trade1, trade2, trade3, trade4, trade5 };
            }
        }
    }
}
