using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Delete;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.HedgingItems.Handlers.Commands
{
    public class HedgingItemDeleteCommandHandlerTests
    {
        private readonly Mock<IRepository<HedgingItem, HedgingItemId>> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly HedgingItemDeleteCommandHandler _handler;

        public HedgingItemDeleteCommandHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<HedgingItem, HedgingItemId>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new HedgingItemDeleteCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldRemoveHedgingItem_AndSaveChanges()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var command = new HedgingItemDeleteCommand { AccountId = accountId, Id = itemId };
            var hedgingItem = HedgingItem.Create(
                new HedgingAccountId(accountId),
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                HedgingItemType.ProfitLosses,
                HedgingItemSideType.WireOut,
                new Money(100),
                "To be deleted"
            );

            _repositoryMock.Setup(r => r.GetByIdOrThrowAsync(It.IsAny<HedgingItemId>(), false, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(hedgingItem);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repositoryMock.Verify(r => r.Remove(hedgingItem), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenAccountIdMismatch()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var command = new HedgingItemDeleteCommand { AccountId = Guid.NewGuid(), Id = Guid.NewGuid() };
            var hedgingItem = HedgingItem.Create(
                new HedgingAccountId (accountId),
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                HedgingItemType.ProfitLosses,
                HedgingItemSideType.WireIn,
                new Money(100),
                "To be deleted"
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
            var command = new HedgingItemDeleteCommand
            {
                AccountId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };

            _repositoryMock.Setup(r => r.GetByIdOrThrowAsync(It.IsAny<HedgingItemId>(), false, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new NotFoundException(nameof(HedgingItem), command.Id));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
