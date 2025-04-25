using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using Moq;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Domain.Enums;
using MockQueryable;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Settle;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Trades.Handlers.Commands
{
    public class TradeSettleCommandHandlerTests
    {
        private readonly Mock<IRepository<Trade, TradeId>> _tradingRepositoryMock;
        private readonly Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>> _productLocationPositionRepositoryMock;
        private readonly Mock<IInventoryService> _inventoryServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly TradeSettleCommandHandler _handler;

        public TradeSettleCommandHandlerTests()
        {
            _tradingRepositoryMock = new Mock<IRepository<Trade, TradeId>>();
            _productLocationPositionRepositoryMock = new Mock<IRepository<ProductLocationPosition, ProductLocationPositionId>>();         
            _inventoryServiceMock = new Mock<IInventoryService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new TradeSettleCommandHandler(
                _unitOfWorkMock.Object,
                _tradingRepositoryMock.Object,
                _productLocationPositionRepositoryMock.Object,
                _inventoryServiceMock.Object                
            );
        }

        [Fact]
        public async Task Handle_ShouldMarkTradeAsSettledAndCreatePositions_WhenCommandIsValid()
        {
            // Arrange
            var product = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(32.148m), MetalType.XAU, true);
            var locationConfiguraionOriginalproduct = ProductLocationConfiguration.Create(
               locationType: LocationType.SLC,
               premiumUnitType: PremiumUnitType.Dollars,
               buyPremium: new Premium(2.0m),
               sellPremium: new Premium(2.0m),
               isAvailableForBuy: true,
               isAvailableForSell: true
            );
            product.AddLocationConfiguration(locationConfiguraionOriginalproduct);

            var tradeItem = TradeItem.Create(
                SideType.Buy,
                product.Id,
                new Weight(10),
                new QuantityUnits(2),
                new Money(22),
                new Money(100),
                new Premium(2),
                new Money(200)
            );
            var trade = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC,
                DateOnly.MinValue,
                DateOnly.MinValue,
                "Test note"
            );
            trade.Items.Add(tradeItem);

            var command = new TradeSettleCommand { Id = trade.Id.Value };

            _tradingRepositoryMock
                .Setup(repo => repo.StartQuery(false, false))
                .Returns(new List<Trade> { trade }.AsQueryable().BuildMock()
            );

            var mockPosition = ProductLocationPosition.Create(
                product.Id,
                trade.Id,
                LocationType.SLC,
                PositionSideType.In,
                PositionType.Settled,
                tradeItem.QuantityUnits,
                new PositionQuantityUnits(30)
            );

            _inventoryServiceMock
                .Setup(service => service.CreatePositionAsync(
                    product.Id,
                    trade.Id,
                    trade.LocationType,
                    PositionType.Settled,
                    PositionSideType.In,
                    tradeItem.QuantityUnits,
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockPosition);

            _productLocationPositionRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<ProductLocationPosition>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            //Act
            await _handler.Handle(command, CancellationToken.None);

            _tradingRepositoryMock.Verify(
                repo => repo.StartQuery(It.IsAny<bool>(), It.IsAny<bool>()),
                Times.Once);

            _inventoryServiceMock.Verify(
                service => service.CreatePositionAsync(
                    product.Id,
                    trade.Id,
                    trade.LocationType,
                    PositionType.Settled,
                    PositionSideType.In,
                    It.Is<QuantityUnits>(q => q == tradeItem.QuantityUnits),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _productLocationPositionRepositoryMock.Verify(
                repo => repo.AddAsync(mockPosition, It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            //Assert
            Assert.True(trade.IsPositionSettled, "Trade should be marked as settled.");
        }
        [Fact]
        public async Task Handle_ShouldFail_WhenTradeNotFound()
        {
            // Arrange
            var product = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(32.148m), MetalType.XAU, true);
            var locationConfiguraionOriginalproduct = ProductLocationConfiguration.Create(
               locationType: LocationType.SLC,
               premiumUnitType: PremiumUnitType.Dollars,
               buyPremium: new Premium(2.0m),
               sellPremium: new Premium(2.0m),
               isAvailableForBuy: true,
               isAvailableForSell: true
            );
            product.AddLocationConfiguration(locationConfiguraionOriginalproduct);
            var tradeItem = TradeItem.Create(
                SideType.Buy,
                product.Id,
                new Weight(10),
                new QuantityUnits(2),
                new Money(22),
                new Money(100),
                new Premium(2),
                new Money(200)
            );
            var trade = Trade.Create(
                TradeType.ClientTrade,
                SideType.Buy,
                LocationType.SLC,
                DateOnly.MinValue,
                DateOnly.MinValue,
                "Test note"
            );
            trade.Items.Add(tradeItem);

            var command = new TradeSettleCommand { Id = Guid.NewGuid() };

            _tradingRepositoryMock
                .Setup(repo => repo.StartQuery(false, false))
                .Returns(new List<Trade> { trade }.AsQueryable().BuildMock()
            );

            var mockPosition = ProductLocationPosition.Create(
                product.Id,
                trade.Id,
                LocationType.SLC,
                PositionSideType.In,
                PositionType.Settled,
                tradeItem.QuantityUnits,
                new PositionQuantityUnits(30)
            );
            _inventoryServiceMock
            .Setup(service => service.CreatePositionAsync(
                product.Id,
                trade.Id,
                trade.LocationType,
                PositionType.Settled,
                PositionSideType.In,
                tradeItem.QuantityUnits,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(mockPosition);

            _productLocationPositionRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<ProductLocationPosition>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            //Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, It.IsAny<CancellationToken>()));

            //Assert
            Assert.Equal(exception.Message, $"Entity Trade with key '{command.Id}' was not found.");       
        }
    }
}
