using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetDetails;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MockQueryable;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Hedging.Accounts.Handlers.Queries;

public class GetHedgingAccountDetailsQueryHandlerTests
{
    private readonly Mock<IRepository<HedgingAccount, HedgingAccountId>> _repositoryMock;
    private readonly GetHedgingAccountDetailsQueryHandler _handler;

    public GetHedgingAccountDetailsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IRepository<HedgingAccount, HedgingAccountId>>();
        _handler = new GetHedgingAccountDetailsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectUnrealizedGainOrLossValue_WhenHedgingAccountIsFound()
    {
        // Arrange
        var query = new GetHedgingAccountDetailsQuery { Id = Guid.NewGuid() };
        var hedgingAccountId = new HedgingAccountId(query.Id);
        var hedgingAccount = new HedgingAccount { Id = hedgingAccountId };

        // Add Hedging Items
        var hedgingItem1 = HedgingItem.Create(
            hedgingAccountId,
            DateTime.UtcNow.ConvertUtcToEstDateOnly(),
            HedgingItemType.ProfitLosses,
            HedgingItemSideType.WireIn,
            new Money(100)
        );
        var hedgingItem2 = HedgingItem.Create(
            hedgingAccountId,
            DateTime.UtcNow.ConvertUtcToEstDateOnly(),
            HedgingItemType.MonthlyFee,
            HedgingItemSideType.WireOut,
            new Money(50)
        );

        hedgingAccount.AddHedgingItem(hedgingItem1);
        hedgingAccount.AddHedgingItem(hedgingItem2);

        // Add Spot Deferred Trade with Item
        var spotDeferredTrade = SpotDeferredTrade.Create(hedgingAccountId, "CONF123", SideType.Buy, DateTime.UtcNow.ConvertUtcToEstDateOnly(), false);
        var spotDeferredTradeItem = SpotDeferredTradeItem.Create(
            MetalType.XAU,
            new Money(50),
            new QuantityOunces(4)
        );
        spotDeferredTrade.AddItem(spotDeferredTradeItem);
        hedgingAccount.AddSpotDeferredTrade(spotDeferredTrade);

        _repositoryMock.Setup(repo => repo.StartQuery(true, true))
            .Returns(new List<HedgingAccount> { hedgingAccount }.AsQueryable().BuildMock());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(350, result.UnrealizedGainOrLossValue); // Expected value based on the provided data
    }


    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenHedgingAccountIsNotFound()
    {
        // Arrange
        var query = new GetHedgingAccountDetailsQuery { Id = Guid.NewGuid() };
        var hedgingAccountId = new HedgingAccountId(query.Id);

        // Set up an empty list to simulate "not found" result
        _repositoryMock.Setup(repo => repo.StartQuery(true, true))
            .Returns(new List<HedgingAccount>().AsQueryable().BuildMock());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryMethods()
    {
        // Arrange
        var query = new GetHedgingAccountDetailsQuery { Id = Guid.NewGuid() };
        var hedgingAccountId = new HedgingAccountId(query.Id);
        var hedgingAccount = new HedgingAccount { Id = hedgingAccountId };

        var mockData = new List<HedgingAccount> { hedgingAccount }.AsQueryable().BuildMock();

        // Set up repository to return the mocked data
        _repositoryMock.Setup(repo => repo.StartQuery(true, true)).Returns(mockData);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - verify that StartQuery and FirstOrDefaultAsync were called
        _repositoryMock.Verify(repo => repo.StartQuery(true, true), Times.Once);
    }
}
