
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Inventory.Queries.GetPositionsHistory;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Inventory.Handlers.Queries
{
    public class GetInventoryPositionsHistoryQueryTests
    {
        private readonly Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>> _repositoryMock;
        private readonly GetInventoryPositionsHistoryQueryHandler _handler;

        public GetInventoryPositionsHistoryQueryTests()
        {
            _repositoryMock = new Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>>();
            _handler = new GetInventoryPositionsHistoryQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_ExpectedResult()
        {
            // Arrange
            var productSKU = "SKU123";
            var productName = "Test Product";
            var request = new GetInventoryPositionsHistoryQuery
            {
                ProductSKU = productSKU,
                Location = LocationType.NY,
                PositionType = PositionType.AvailableForTrading,
                PageNumber = 1,
                PageSize = 10
            };

            var product = PreciousMetalsTradingSystem.Domain.Entities.Product.Create(
                productName,
                new SKU(productSKU),
                new Weight(10m),
                MetalType.XAU,
                true);

            var trade = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.NY,
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                DateTime.UtcNow.ConvertUtcToEstDateOnly().AddDays(2),
                "Test note");

            var productLocationPosition = ProductLocationPosition.Create(
                product.Id,
                trade.Id,
                LocationType.NY,
                PositionSideType.In,
                PositionType.AvailableForTrading,
                new QuantityUnits(100),
            new PositionQuantityUnits(50)
            );

            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(productLocationPosition, product);
            typeof(ProductLocationPosition).GetProperty("Trade")!.SetValue(productLocationPosition, trade);

            var mockData = new List<ProductLocationPosition> { productLocationPosition };
            var mockQueryable = mockData.AsQueryable().BuildMock(); // Mock IQueryable

            _repositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(mockQueryable);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockData.Count, result.TotalCount);
            Assert.Equal(request.PageNumber, result.PageNumber);
            Assert.Equal(request.PageSize, result.PageSize);
            Assert.Equal(productSKU, result.Items.First().ProductSKU);
            Assert.Equal(productName, result.Items.First().ProductName);
        }

        [Fact]
        public async Task Handle_ShoulReturn_EmptyResult_WhenNoMatchingData()
        {
            // Arrange
            var request = new GetInventoryPositionsHistoryQuery
            {
                ProductSKU = "NonExistingSKU",
                Location = LocationType.NY,
                PositionType = PositionType.AvailableForTrading,
                PageNumber = 1,
                PageSize = 10
            };
            var mockData = new List<ProductLocationPosition>();
            var mockQueryable = mockData.AsQueryable().BuildMock(); // Mock IQueryable

            _repositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(mockQueryable);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task Handle_ShouldFilterByPositionType_WhenSpecified()
        {
            // Arrange
            var productSKU = "SKU123";
            var productName = "Test Product";
            var request = new GetInventoryPositionsHistoryQuery
            {
                ProductSKU = productSKU,
                Location = LocationType.NY,
                PositionType = PositionType.AvailableForTrading,
                PageNumber = 1,
                PageSize = 10
            };

            var product = PreciousMetalsTradingSystem.Domain.Entities.Product.Create(
                productName,
                new SKU(productSKU),
                new Weight(10m),
                MetalType.XAU,
                true);

            var trade = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.NY,
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                DateTime.UtcNow.ConvertUtcToEstDateOnly().AddDays(2),
                "Test note");

            var matchingPosition = ProductLocationPosition.Create(
                product.Id,
                trade.Id,
                LocationType.NY,
                PositionSideType.In,
                PositionType.AvailableForTrading,
                new QuantityUnits(100),
                new PositionQuantityUnits(50)
            );

            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(matchingPosition, product);
            typeof(ProductLocationPosition).GetProperty("Trade")!.SetValue(matchingPosition, trade);

            var nonMatchingPosition = ProductLocationPosition.Create(
                product.Id,
                trade.Id,
                LocationType.NY,
                PositionSideType.In,
                PositionType.Settled,
                new QuantityUnits(100),
                new PositionQuantityUnits(50)
            );

            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(nonMatchingPosition, product);
            typeof(ProductLocationPosition).GetProperty("Trade")!.SetValue(nonMatchingPosition, trade);

            var mockData = new List<ProductLocationPosition> { matchingPosition, nonMatchingPosition };
            var mockQueryable = mockData.AsQueryable().BuildMock(); // Mock IQueryable

            _repositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(mockQueryable);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Items);
            Assert.Single(result.Items);
            Assert.Equal(request.PositionType, result.Items.First().PositionType);
        }

        [Fact]
        public async Task Handle_ShouldFilterByLocationType_WhenSpecified()
        {
            // Arrange
            var productSKU = "SKU123";
            var productName = "Test Product";
            var request = new GetInventoryPositionsHistoryQuery
            {
                ProductSKU = productSKU,
                Location = LocationType.SLC,
                PositionType = PositionType.AvailableForTrading,
                PageNumber = 1,
                PageSize = 10
            };

            var product = PreciousMetalsTradingSystem.Domain.Entities.Product.Create(
                productName,
                new SKU(productSKU),
                new Weight(10m),
                MetalType.XAU,
                true);

            var tradeNY = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.NY,
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                DateTime.UtcNow.ConvertUtcToEstDateOnly().AddDays(2),
                "NY Trade");

            var tradeSLC = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC,
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                DateTime.UtcNow.ConvertUtcToEstDateOnly().AddDays(2),
                "SLC Trade");

            var positionInNY = ProductLocationPosition.Create(
                product.Id,
                tradeNY.Id,
                LocationType.NY,
                PositionSideType.In,
                PositionType.AvailableForTrading,
                new QuantityUnits(50),
                new PositionQuantityUnits(25)
            );

            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(positionInNY, product);
            typeof(ProductLocationPosition).GetProperty("Trade")!.SetValue(positionInNY, tradeNY);

            var positionInSLC = ProductLocationPosition.Create(
                product.Id,
                tradeSLC.Id,
                LocationType.SLC,
                PositionSideType.In,
                PositionType.AvailableForTrading,
                new QuantityUnits(50),
                new PositionQuantityUnits(25)
            );

            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(positionInSLC, product);
            typeof(ProductLocationPosition).GetProperty("Trade")!.SetValue(positionInSLC, tradeSLC);

            var mockData = new List<ProductLocationPosition> { positionInNY, positionInSLC };
            var mockQueryable = mockData.AsQueryable().BuildMock(); // Mock IQueryable

            _repositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(mockQueryable);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Items);
            Assert.Single(result.Items); // Only one item matches the LocationType filter
            Assert.Equal(request.Location, result.Items.First().Location);
        }

        [Fact]
        public async Task Handle_ShouldFilterByProductSku_WhenSpecified()
        {
            // Arrange
            var productSKU = "SKU123";
            var productSKUNonFiltered = "SKU456";
            var productName = "Test Product";

            var request = new GetInventoryPositionsHistoryQuery
            {
                ProductSKU = productSKU,
                Location = LocationType.SLC,
                PositionType = PositionType.AvailableForTrading,
                PageNumber = 1,
                PageSize = 10
            };

            var product = PreciousMetalsTradingSystem.Domain.Entities.Product.Create(
                productName,
                new SKU(productSKU),
                new Weight(10m),
                MetalType.XAU,
                true);

            var productNonFiltered = PreciousMetalsTradingSystem.Domain.Entities.Product.Create(
                productName,
                new SKU(productSKUNonFiltered),
                new Weight(10m),
                MetalType.XAU,
                true);

            var trade = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC,
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                DateTime.UtcNow.ConvertUtcToEstDateOnly().AddDays(2),
                "Test Trade");

            var matchingPosition = ProductLocationPosition.Create(
                product.Id,
                trade.Id,
                LocationType.SLC,
                PositionSideType.In,
                PositionType.AvailableForTrading,
                new QuantityUnits(50),
                new PositionQuantityUnits(25)
            );

            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(matchingPosition, product);
            typeof(ProductLocationPosition).GetProperty("Trade")!.SetValue(matchingPosition, trade);

            var nonMatchingPosition = ProductLocationPosition.Create(
                productNonFiltered.Id,
                trade.Id,
                LocationType.SLC,
                PositionSideType.In,
                PositionType.AvailableForTrading,
                new QuantityUnits(50),
                new PositionQuantityUnits(25)
            );

            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(nonMatchingPosition, productNonFiltered);
            typeof(ProductLocationPosition).GetProperty("Trade")!.SetValue(nonMatchingPosition, trade);

            var mockData = new List<ProductLocationPosition> { matchingPosition, nonMatchingPosition };
            var mockQueryable = mockData.AsQueryable().BuildMock(); // Mock IQueryable

            _repositoryMock
                .Setup(repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(mockQueryable);
            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Items);
            Assert.Single(result.Items); // Only the matching SKU should be returned
            Assert.Equal(productSKU, result.Items.First().ProductSKU);
        }

    }
}
