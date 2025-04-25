using System.Linq.Expressions;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ConfirmTrades;
using PreciousMetalsTradingSystem.Application.Emailing.Options;
using PreciousMetalsTradingSystem.Application.Emailing.Services;
using PreciousMetalsTradingSystem.Application.Emailing.Models;
using PreciousMetalsTradingSystem.Application.Emailing.Exceptions;
using Microsoft.Extensions.Options;
using Moq;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Trading.Execution.Handlers.Commands
{
    public class ConfirmTradesCommandHandlerTests
    {
        private readonly Mock<IOptions<TradeConfirmationEmailOptions>> _tradeConfirmationEmailOptionsMock;
        private readonly Mock<IRepository<Trade, TradeId>> _tradingRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private ConfirmTradesCommandHandler _handler;

        public ConfirmTradesCommandHandlerTests()
        {
            _tradeConfirmationEmailOptionsMock = new Mock<IOptions<TradeConfirmationEmailOptions>>();
            _tradeConfirmationEmailOptionsMock
                .Setup(repo => repo.Value)
                .Returns(new TradeConfirmationEmailOptions
                {
                    SendTradeConfirmationEmail = true,
                    FromName = "From Test",
                    FromAddress = "from.test@test.com",
                    ToName = "To Test",
                    ToAddresses = "to.test@test.com",
                    EmailSubject = "Subject Test",
                    EmailBody = "Body Test",
                }
            );
            _tradingRepositoryMock = new Mock<IRepository<Trade, TradeId>>();
            _emailServiceMock = new Mock<IEmailService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new ConfirmTradesCommandHandler(
                _tradeConfirmationEmailOptionsMock.Object,
                _tradingRepositoryMock.Object,
                _emailServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldMarkTradesAsConfirmed_WhenCommandIsValid()
        {
            // Arrange
            var product = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(32.148m), MetalType.XAU, true);
            var trade = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.SLC, DateOnly.MinValue, DateOnly.MinValue, "Test note");
            trade.Items.Add(TradeItem.Create(SideType.Buy, product.Id, new Weight(10), new QuantityUnits(2), new Money(22), new Money(100), new Premium(2), new Money(200)).SetProduct(product));
            var mockData = new List<Trade> { trade };

            var command = new ConfirmTradesCommand();

            _tradingRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((mockData, mockData.Count));

            _emailServiceMock
                .Setup(repo => repo.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            //Act
            await _handler.Handle(command, CancellationToken.None);

            _tradingRepositoryMock.Verify(
                repo => repo.GetAllAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            //Assert
            Assert.NotNull(trade.ConfirmedOnUtc);
        }

        [Fact]
        public async Task Handle_ShouldMarkTradesAsConfirmed_WithoutEmail_WhenCommandIsValid()
        {
            _tradeConfirmationEmailOptionsMock
                .Setup(repo => repo.Value)
                .Returns(new TradeConfirmationEmailOptions
                {
                    SendTradeConfirmationEmail = false,
                }
            );

            _handler = new ConfirmTradesCommandHandler(
                _tradeConfirmationEmailOptionsMock.Object,
                _tradingRepositoryMock.Object,
                _emailServiceMock.Object,
                _unitOfWorkMock.Object
            );

            // Arrange
            var product = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(32.148m), MetalType.XAU, true);
            var trade = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.SLC, DateOnly.MinValue, DateOnly.MinValue, "Test note");
            trade.Items.Add(TradeItem.Create(SideType.Buy, product.Id, new Weight(10), new QuantityUnits(2), new Money(22), new Money(100), new Premium(2), new Money(200)).SetProduct(product));
            var mockData = new List<Trade> { trade };

            var command = new ConfirmTradesCommand();

            _tradingRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((mockData, mockData.Count));

            _emailServiceMock
                .Setup(repo => repo.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            //Act
            await _handler.Handle(command, CancellationToken.None);

            _tradingRepositoryMock.Verify(
                repo => repo.GetAllAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            //Assert
            Assert.NotNull(trade.ConfirmedOnUtc);
        }

        [Fact]
        public async Task Handle_ShouldFail_WhenEmailOptionIsWrong()
        {
            _tradeConfirmationEmailOptionsMock
                .Setup(repo => repo.Value)
                .Returns(new TradeConfirmationEmailOptions
                {
                    SendTradeConfirmationEmail = true,
                    FromName = "Test",
                    FromAddress = "",
                }
            );

            _handler = new ConfirmTradesCommandHandler(
                _tradeConfirmationEmailOptionsMock.Object,
                _tradingRepositoryMock.Object,
                _emailServiceMock.Object,
                _unitOfWorkMock.Object
            );

            // Arrange
            var product = Product.Create("Test Product 1", new SKU("TST-1"), new Weight(32.148m), MetalType.XAU, true);
            var trade = Trade.Create(TradeType.ClientTrade, SideType.Buy, LocationType.SLC, DateOnly.MinValue, DateOnly.MinValue, "Test note");
            trade.Items.Add(TradeItem.Create(SideType.Buy, product.Id, new Weight(10), new QuantityUnits(2), new Money(22), new Money(100), new Premium(2), new Money(200)).SetProduct(product));
            var mockData = new List<Trade> { trade };

            var command = new ConfirmTradesCommand();

            _tradingRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<Trade, bool>>>(), It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((mockData, mockData.Count));

            _emailServiceMock
                .Setup(repo => repo.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true));

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            //Act
            var exception = await Assert.ThrowsAsync<EmailConfigurationException>(() => _handler.Handle(command, It.IsAny<CancellationToken>()));

            //Assert
            Assert.Equal("Sender Email for Trade Confirmation Emails is not set.", exception.Message);
        }
    }
}
