using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Create;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.HedgingItems.Handlers.Commands
{
    public class HedgingItemCreateCommandHandlerTests
    {
        private readonly Mock<IRepository<HedgingAccount, HedgingAccountId>> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly HedgingItemCreateCommandHandler _handler;

        public HedgingItemCreateCommandHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<HedgingAccount, HedgingAccountId>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new HedgingItemCreateCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldAddHedgingItem_AndSaveChanges()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var command = new HedgingItemCreateCommand
            {
                AccountId = accountId,
                Date = DateTime.UtcNow.ConvertUtcToEst(),
                HedgingItemType = HedgingItemType.ProfitLosses,
                SideType = HedgingItemSideType.WireIn,
                Amount = 100,
                Note = "Test Note"
            };
            var hedgingAccount = HedgingAccount.Create(new HedgingAccountName("Test Account"), new HedgingAccountCode("Code"));
            _repositoryMock.Setup(r => r.GetByIdOrThrowAsync(It.IsAny<HedgingAccountId>(), false, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(hedgingAccount);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Contains(hedgingAccount.HedgingItems, item => item.Id.Value == result);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenHedgingAccountNotFound()
        {
            // Arrange
            var command = new HedgingItemCreateCommand
            {
                AccountId = Guid.NewGuid(),
                Date = DateTime.UtcNow.ConvertUtcToEst(),
                HedgingItemType = HedgingItemType.ProfitLosses,
                SideType = HedgingItemSideType.WireIn,
                Amount = 100,
                Note = "Test Note"
            };

            _repositoryMock.Setup(r => r.GetByIdOrThrowAsync(It.IsAny<HedgingAccountId>(), false, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new NotFoundException(nameof(HedgingAccount), command.AccountId));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

    }
}
