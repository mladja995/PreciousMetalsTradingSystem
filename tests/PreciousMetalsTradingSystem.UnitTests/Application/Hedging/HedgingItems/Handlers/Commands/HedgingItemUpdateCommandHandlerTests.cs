using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Update;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.HedgingItems.Handlers.Commands
{
    public class HedgingItemUpdateCommandHandlerTests
    {
        private readonly Mock<IRepository<HedgingItem, HedgingItemId>> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly HedgingItemUpdateCommandHandler _handler;

        public HedgingItemUpdateCommandHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<HedgingItem, HedgingItemId>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new HedgingItemUpdateCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateHedgingItem_AndSaveChanges()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var command = new HedgingItemUpdateCommand
            {
                AccountId = accountId,
                HedgingItemId = itemId,
                Date = DateTime.UtcNow.ConvertUtcToEst(),
                HedgingItemType = HedgingItemType.ProfitLosses,
                SideType = HedgingItemSideType.WireOut,
                Amount = 200,
                Note = "Updated Note"
            };

            var hedgingItem = HedgingItem.Create(
                new HedgingAccountId(accountId),
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                HedgingItemType.ProfitLosses,
                HedgingItemSideType.WireIn,
                new Money(100),
                "Old Note"
            );

            _repositoryMock.Setup(r => r.GetByIdOrThrowAsync(It.IsAny<HedgingItemId>(), false, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(hedgingItem);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(DateOnly.FromDateTime(command.Date), hedgingItem.HedgingItemDate);
            Assert.Equal(command.HedgingItemType, hedgingItem.Type);
            Assert.Equal(command.SideType, hedgingItem.SideType);
            Assert.Equal(command.Amount, hedgingItem.Amount.Value);
            Assert.Equal(command.Note, hedgingItem.Note);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenAccountIdMismatch()
        {
            // Arrange
            var command = new HedgingItemUpdateCommand
            {
                AccountId = Guid.NewGuid(),
                HedgingItemId = Guid.NewGuid(),
                Date = DateTime.UtcNow.ConvertUtcToEst(),
                HedgingItemType = HedgingItemType.ProfitLosses,
                SideType = HedgingItemSideType.WireOut,
                Amount = 200,
                Note = "Updated Note"
            };

            var hedgingItem = HedgingItem.Create(
                new HedgingAccountId(Guid.NewGuid()),
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                HedgingItemType.ProfitLosses,
                HedgingItemSideType.WireIn,
                new Money(100),
                "Old Note"
            );

            _repositoryMock.Setup(r => r.GetByIdOrThrowAsync(It.IsAny<HedgingItemId>(), false, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(hedgingItem);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenHedgingItemNotFound()
        {
            // Arrange
            var command = new HedgingItemUpdateCommand
            {
                AccountId = Guid.NewGuid(),
                HedgingItemId = Guid.NewGuid(),
                Date = DateTime.UtcNow.ConvertUtcToEst(),
                HedgingItemType = HedgingItemType.ProfitLosses,
                SideType = HedgingItemSideType.WireOut,
                Amount = 200,
                Note = "Updated Note"
            };

            _repositoryMock.Setup(r => r.GetByIdOrThrowAsync(It.IsAny<HedgingItemId>(), false, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new NotFoundException(nameof(HedgingItem), command.HedgingItemId));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

    }
}
