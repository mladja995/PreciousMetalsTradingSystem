using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Financials.Adjustments.Commands.Create
{
    public class FinancialsAdjustmentCreateCommandHandler : IRequestHandler<FinancialsAdjustmentCreateCommand, Guid>
    {
        private readonly IRepository<FinancialAdjustment, FinancialAdjustmentId> _financialAdjustmentRepository;
        private readonly IFinancialsService _financialService;
        private readonly IUnitOfWork _unitOfWork;

        public FinancialsAdjustmentCreateCommandHandler(IRepository<FinancialAdjustment, FinancialAdjustmentId> repository, 
            IFinancialsService financialService,
            IUnitOfWork unitOfWork)
        {
            _financialAdjustmentRepository = repository;
            _financialService = financialService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(FinancialsAdjustmentCreateCommand request, CancellationToken cancellationToken)
        {
            //Create Financial Adjustment 

            var financialAdjustment = FinancialAdjustment.Create(
                DateOnly.FromDateTime(request.Date),
                request.SideType,
                new Money(request.Amount),
                request.Note
                );

            //Create Financial Transaction for both type

            var transactionEffective = await _financialService.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    request.SideType,
                    BalanceType.Effective,
                    new Money(request.Amount),
                    financialAdjustment.Id,
                    cancellationToken);

            var transactionActual = await _financialService.CreateFinancialTransactionAsync(
                    ActivityType.Adjustment,
                    request.SideType,
                    BalanceType.Actual,
                    new Money(request.Amount),
                    financialAdjustment.Id,
                    cancellationToken);

            financialAdjustment.AddFinancialTransaction(transactionEffective);

            financialAdjustment.AddFinancialTransaction(transactionActual);

            await _financialAdjustmentRepository.AddAsync(financialAdjustment, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return financialAdjustment.Id;

        }

    }
}
