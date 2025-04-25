using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Commands.Create;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using Models = PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using Moq;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.SpotDeferredTrades.Handlers.Commands
{
    public class SpotDeferredTradeCreateCommandHandlerTests
    {
        private readonly Mock<IRepository<HedgingAccount, HedgingAccountId>> _hedgingAccountRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly SpotDeferredTradeCreateCommandHandler _handler;

        public SpotDeferredTradeCreateCommandHandlerTests()
        {
            _hedgingAccountRepositoryMock = new Mock<IRepository<HedgingAccount, HedgingAccountId>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new SpotDeferredTradeCreateCommandHandler(
                _hedgingAccountRepositoryMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateAndSaveSpotDeferredTrade_WhenCommandIsValid()
        {
            // Arrange
            var command = new SpotDeferredTradeCreateCommand
            {
                AccountId = Guid.NewGuid(),
                TradeConfirmationNumber = "TCN123",
                Date = DateTime.UtcNow.ConvertUtcToEst(),
                SideType = SideType.Buy,
                Items =
                [
                    new Models.SpotDeferredTradeItem
                    {
                        MetalType = MetalType.XAU,
                        SpotPricePerOz = 1500.0m,
                        QuantityOz = 10.0m
                    }
                ]
            };

            _hedgingAccountRepositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(It.IsAny<HedgingAccountId>(), false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(HedgingAccount.Create(new HedgingAccountName("TestAccount"), new HedgingAccountCode("34025")));

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _hedgingAccountRepositoryMock.Verify(repo => repo.GetByIdOrThrowAsync(It.IsAny<HedgingAccountId>(), false, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsType<Guid>(result);
            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenHedgingAccountDoesNotExist()
        {
            // Arrange
            var command = new SpotDeferredTradeCreateCommand
            {
                AccountId = Guid.NewGuid(),
                TradeConfirmationNumber = "TCN123",
                Date = DateTime.UtcNow.ConvertUtcToEst(),
                SideType = SideType.Buy,
                Items =
                [
                    new Models.SpotDeferredTradeItem
                    {
                        MetalType = MetalType.XAU,
                        SpotPricePerOz = 1500.0m,
                        QuantityOz = 10.0m
                    }
                ]
            };

            _hedgingAccountRepositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(It.IsAny<HedgingAccountId>(), false, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException("HedgingAccount not found."));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            _hedgingAccountRepositoryMock.Verify(repo => repo.GetByIdOrThrowAsync(It.IsAny<HedgingAccountId>(), false, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenDuplicateMetalTypeInItems()
        {
            // Arrange
            var command = new SpotDeferredTradeCreateCommand
            {
                AccountId = Guid.NewGuid(),
                TradeConfirmationNumber = "TCN123",
                Date = DateTime.UtcNow.ConvertUtcToEst(),
                SideType = SideType.Buy,
                Items =
                [
                    new Models.SpotDeferredTradeItem
                    {
                        MetalType = MetalType.XAU,
                        SpotPricePerOz = 1500.0m,
                        QuantityOz = 10.0m
                    },
                    new Models.SpotDeferredTradeItem
                    {
                        MetalType = MetalType.XAU,
                        SpotPricePerOz = 1500.0m,
                        QuantityOz = 5.0m
                    }
                ]
            };

            _hedgingAccountRepositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(It.IsAny<HedgingAccountId>(), false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HedgingAccount());

            // Act & Assert
            await Assert.ThrowsAsync<DuplicatedSpotDeferredTradeItemPerMetalTypeException>(() => _handler.Handle(command, CancellationToken.None));
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
