using System.Linq.Expressions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetCollection;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.HedgingItems.Handlers.Queries
{
    public class GetHedgeItemsQueryHandlerTests
    {
        private readonly Mock<IRepository<HedgingItem, HedgingItemId>> _repositoryMock;
        private readonly GetHedgingItemsQueryHandler _handler;

        public GetHedgeItemsQueryHandlerTests()
        {
            _repositoryMock = new Mock<IRepository<HedgingItem, HedgingItemId>>();
            _handler = new GetHedgingItemsQueryHandler(_repositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_ExpectedResult()
        {
            // Arrange
            var query = new GetHedgingItemsQuery
            {
                AccountId = new Guid("1EB3F5C3-7890-4FEF-BE70-5F311C6F9F0C"),
                Type = HedgingItemType.MonthlyFee,
                PageNumber = 1,
                PageSize = 10
            };

            var hedgingItem = HedgingItem.Create(
                new HedgingAccountId (query.AccountId),
                new DateOnly(2022, 12, 1),
                query.Type,
                HedgingItemSideType.WireIn,
                new Money(1234.50m),
                ""
            );


            var mockData = new List<HedgingItem> { hedgingItem };

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal(hedgingItem.Type, result.Items.First().Type);
            Assert.Equal(hedgingItem.SideType, result.Items.First().SideType);
            Assert.Equal(hedgingItem.Amount, result.Items.First().Amount);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyResult_WhenNoMatchingData()
        {
            // Arrange
            var query = new GetHedgingItemsQuery
            {
                AccountId = new Guid("2EB3F5C3-7890-4FEF-BE70-5F311C6F9F0C"),
                Type = HedgingItemType.MonthlyFee,
                PageNumber = 1,
                PageSize = 10
            };

            var mockData = new List<HedgingItem>();

            _repositoryMock
                .Setup(repo => repo.StartQuery(true, false))
                .Returns(mockData.AsQueryable().BuildMock());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
    }
}
