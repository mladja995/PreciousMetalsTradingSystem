using PreciousMetalsTradingSystem.Application.Inventory.Queries.GetState;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using LinqKit;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Inventory.Handlers.Queries
{
    public class GetInventoryStateQueryTests
    {
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly GetInventoryStateQueryHandler _handler;

        public GetInventoryStateQueryTests()
        {
            _inventoryServiceMock = new Mock<IInventoryService>();
            _handler = new GetInventoryStateQueryHandler(_inventoryServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_ExpectedResult()
        {
            // Arrange
            var request = new GetInventoryStateQuery
            {
                Location = LocationType.NY,
                OnDate = null,
                PageNumber = 1,
                PageSize = 10
            };

            var product1 = Product.Create(
                name: "Test Product 1",
                sku: new SKU("TEST-SKU-1"),
                weightInOz: new Weight(100.0m),
                metalType: MetalType.XAG,
                isAvailable: true
            );

            var product2 = Product.Create(
                name: "Test Product 2",
                sku: new SKU("TEST-SKU-2"),
                weightInOz: new Weight(5.0m),
                metalType: MetalType.XAU,
                isAvailable: true
            );

            var mockData = new List<ProductLocationPosition>
            {
                ProductLocationPosition.Create(
                    productId: product1.Id,
                    relatedTradeId: TradeId.New(),
                    location: LocationType.NY,
                    sideType: PositionSideType.In,
                    type: PositionType.AvailableForTrading,
                    quantityUnits: new QuantityUnits(12),
                    currentPositionQuantityUnits: new PositionQuantityUnits(148)
                ),
                ProductLocationPosition.Create(
                    productId: product2.Id,
                    relatedTradeId: TradeId.New(),
                    location: LocationType.NY,
                    sideType: PositionSideType.In,
                    type: PositionType.AvailableForTrading,
                    quantityUnits: new QuantityUnits(8),
                    currentPositionQuantityUnits: new PositionQuantityUnits(44)
                ),
                ProductLocationPosition.Create(
                    productId: product2.Id,
                    relatedTradeId: TradeId.New(),
                    location: LocationType.NY,
                    sideType: PositionSideType.Out,
                    type: PositionType.Settled,
                    quantityUnits: new QuantityUnits(3),
                    currentPositionQuantityUnits: new PositionQuantityUnits(39)
                )
            };

            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(mockData[0], product1);
            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(mockData[1], product2);
            typeof(ProductLocationPosition).GetProperty("Product")!.SetValue(mockData[2], product2);

            _inventoryServiceMock
                .Setup(svc => svc.GetRunningPositionsAsync(
                    request.Location,
                    null,
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockData);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockData.Count(x => x.Type == PositionType.AvailableForTrading), result.TotalCount);
            Assert.Equal(request.PageNumber, result.PageNumber);
            Assert.Equal(request.PageSize, result.PageSize);
            result.Items.ForEach(item => Assert.Equal(LocationType.NY, item.Location));
        }

        [Fact]
        public async Task Handle_ShouldReturn_EmptyResult_WhenNoMatchingData()
        {
            // Arrange
            var request = new GetInventoryStateQuery
            {
                Location = LocationType.IDS_DE,
                OnDate = null,
                PageNumber = 1,
                PageSize = 10
            };

            var mockData = new List<ProductLocationPosition>();

            _inventoryServiceMock
                .Setup(svc => svc.GetRunningPositionsAsync(
                    request.Location,
                    null,
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockData);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
    }
}

