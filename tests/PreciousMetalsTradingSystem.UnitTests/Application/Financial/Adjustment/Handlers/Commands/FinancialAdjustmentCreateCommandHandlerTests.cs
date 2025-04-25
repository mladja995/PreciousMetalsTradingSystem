using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Adjustments.Commands.Create;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.UnitTests.Application.Financial.Adjustment.Handlers.Commands
{
    public class FinancialAdjustmentCreateCommandHandlerTests
    {
        private readonly Mock<IRepository<FinancialAdjustment, FinancialAdjustmentId>> _financialAdjustmentRepositoryMock;
        private readonly Mock<IFinancialsService> _financialServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly FinancialsAdjustmentCreateCommandHandler _handler;

        public FinancialAdjustmentCreateCommandHandlerTests()
        {
            _financialAdjustmentRepositoryMock = new Mock<IRepository<FinancialAdjustment, FinancialAdjustmentId>>();
            _financialServiceMock = new Mock<IFinancialsService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new FinancialsAdjustmentCreateCommandHandler(
                _financialAdjustmentRepositoryMock.Object,
                _financialServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }
        [Fact]
        public async Task Handle_ShouldCreateFinancialAdjustmentAndTransactions()
        {
            // Arrange
            var command = new FinancialsAdjustmentCreateCommand
            {
                Date = DateTime.UtcNow,
                SideType = TransactionSideType.Credit,
                Amount = 100m,
                Note = "Test adjustment"
            };

            var financialAdjustment = FinancialAdjustment.Create(
                DateOnly.FromDateTime(command.Date),
                command.SideType,
                new Money(command.Amount),
                command.Note
            );

            var currentBalanceEffective = new FinancialBalance(5400m);
            var currentBalanceActual = new FinancialBalance(6000m);

            var effectiveTransaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                BalanceType.Effective,
                ActivityType.Adjustment,
                new Money(command.Amount),
                currentBalanceEffective,
                financialAdjustment.Id);

            var actualTransaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                BalanceType.Actual,
                ActivityType.Adjustment,
                new Money(command.Amount),
                currentBalanceActual,
                financialAdjustment.Id);

            _financialAdjustmentRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<FinancialAdjustment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _financialServiceMock
                .Setup(service => service.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    command.SideType,
                    BalanceType.Effective,
                    It.Is<Money>(m => m.Value == command.Amount),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(effectiveTransaction);

            _financialServiceMock
                .Setup(service => service.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    command.SideType,
                    BalanceType.Actual,
                    It.Is<Money>(m => m.Value == command.Amount),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(actualTransaction);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);


            _financialAdjustmentRepositoryMock.Verify(
                repo => repo.AddAsync(It.IsAny<FinancialAdjustment>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _financialServiceMock.Verify(
                service => service.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    command.SideType,
                    BalanceType.Effective,
                    It.Is<Money>(m => m.Value == command.Amount),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _financialServiceMock.Verify(
                service => service.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    command.SideType,
                    BalanceType.Actual,
                    It.Is<Money>(m => m.Value == command.Amount),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Assert
            Assert.IsType<Guid>(result);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenDatabaseSaveFails()
        {
            // Arrange
            var command = new FinancialsAdjustmentCreateCommand
            {
                Date = DateTime.UtcNow,
                SideType = TransactionSideType.Credit,
                Amount = 100m,
                Note = "Test adjustment"
            };

            var financialAdjustment = FinancialAdjustment.Create(
                DateOnly.FromDateTime(command.Date),
                command.SideType,
                new Money(command.Amount),
                command.Note
            );

            var currentBalanceEffective = new FinancialBalance(5400m);
            var currentBalanceActual = new FinancialBalance(6000m);

            var effectiveTransaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                BalanceType.Effective,
                ActivityType.Adjustment,
                new Money(command.Amount),
                currentBalanceEffective,
                financialAdjustment.Id);

            var actualTransaction = FinancialTransaction.Create(
                TransactionSideType.Credit,
                BalanceType.Actual,
                ActivityType.Adjustment,
                new Money(command.Amount),
                currentBalanceActual,
                financialAdjustment.Id);

            _financialAdjustmentRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<FinancialAdjustment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _financialServiceMock
                .Setup(service => service.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    command.SideType,
                    BalanceType.Effective,
                    It.Is<Money>(m => m.Value == command.Amount),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(effectiveTransaction);

            _financialServiceMock
                .Setup(service => service.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    command.SideType,
                    BalanceType.Actual,
                    It.Is<Money>(m => m.Value == command.Amount),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(actualTransaction);

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database save failed"));

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));



            _financialAdjustmentRepositoryMock.Verify(
                repo => repo.AddAsync(It.IsAny<FinancialAdjustment>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _financialServiceMock.Verify(
                service => service.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    command.SideType,
                    BalanceType.Effective,
                    It.Is<Money>(m => m.Value == command.Amount),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _financialServiceMock.Verify(
                service => service.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    command.SideType,
                    BalanceType.Actual,
                    It.Is<Money>(m => m.Value == command.Amount),
                    It.IsAny<IEntityId>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Assert
            Assert.Equal("Database save failed", exception.Message);
        }
    }
}
