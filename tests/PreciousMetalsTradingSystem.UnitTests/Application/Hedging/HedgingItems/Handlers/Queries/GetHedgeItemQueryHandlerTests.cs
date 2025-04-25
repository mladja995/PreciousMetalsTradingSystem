using System.Linq.Expressions;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetSingle;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.HedgingItems.Handlers.Queries
{
    public class GetHedgeItemQueryHandlerTests
    {
        private readonly Mock<IRepository<HedgingItem, HedgingItemId>> _repositoryMock;
        private readonly GetHedgingItemQueryHandler _handler;

        public GetHedgeItemQueryHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<HedgingItem, HedgingItemId>>();
            _handler = new GetHedgingItemQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_ExpectedResult()
        {
            // Arrange
            var query = new GetHedgingItemQuery
            {
                AccountId = new Guid("1EB3F5C3-7890-4FEF-BE70-5F311C6F9F0C"),
                HedingItemId = Guid.NewGuid(),
            };

            var mockData = HedgingItem.Create(
                new HedgingAccountId(query.AccountId),
                new DateOnly(2022, 12, 1),
                HedgingItemType.MonthlyFee,
                HedgingItemSideType.WireOut,
                new Money(1234.50m),
                ""
            );


            _repositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(
                    It.IsAny<HedgingItemId>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Expression<Func<HedgingItem, object>>[]>()
                ))
                .ReturnsAsync(mockData);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockData.Type, result.Type);
            Assert.Equal(mockData.SideType, result.SideType);
            Assert.Equal(mockData.Amount, result.Amount);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenHedgingItemNotFound()
        {
            // Arrange
            var query = new GetHedgingItemQuery
            {
                AccountId = new Guid("1EB3F5C3-7890-4FEF-BE70-5F311C6F9F0C"),
                HedingItemId = Guid.NewGuid(),
            };

            _repositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(
                    It.IsAny<HedgingItemId>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Expression<Func<HedgingItem, object>>[]>()
                ))
                .ThrowsAsync(new NotFoundException(nameof(HedgingItem), query.HedingItemId));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenHedgingAccountIdDoesNotMatch()
        {
            // Arrange
            var query = new GetHedgingItemQuery
            {
                AccountId = new Guid("1EB3F5C3-7890-4FEF-BE70-5F311C6F9F0C"),
                HedingItemId = Guid.NewGuid(),
            };

            var mockData = HedgingItem.Create(
                new HedgingAccountId(Guid.NewGuid()),
                new DateOnly(2022, 12, 1),
                HedgingItemType.MonthlyFee,
                HedgingItemSideType.WireOut,
                new Money(1234.50m),
                ""
            );

            _repositoryMock
                .Setup(repo => repo.GetByIdOrThrowAsync(
                    It.IsAny<HedgingItemId>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>(),
                    It.IsAny<Expression<Func<HedgingItem, object>>[]>()
                ))
                .ReturnsAsync(mockData);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }
    }
}
