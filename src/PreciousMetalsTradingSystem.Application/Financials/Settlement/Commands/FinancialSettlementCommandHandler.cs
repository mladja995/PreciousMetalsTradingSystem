using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Application.Financials.Settlement.Commands
{
    public class FinancialSettlementCommandHandler : IRequestHandler<FinancialSettlementCommand>
    {
        private readonly IRepository<Trade, TradeId> _tradeRepository;
        private readonly IFinancialsService _financialService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FinancialSettlementCommand> _logger;

        public FinancialSettlementCommandHandler (
            IRepository<Trade, TradeId> repository,
            IFinancialsService financialService, 
            IUnitOfWork unitOfWork,
            ILogger<FinancialSettlementCommand> logger)
        {
            _tradeRepository= repository; 
            _financialService = financialService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(FinancialSettlementCommand request, CancellationToken cancellationToken)
        {
            var failedTradeIds = new List<Guid>();

            var nowInEst = DateTimeExtensions.ConvertUtcToEst(DateTime.UtcNow);

            // TODO: Create more safer filter based on allowed trade types for financial settlement
            var tradesToSettle = await _tradeRepository.StartQuery(asSplitQuery: true)
                .Include(x => x.Items)
                .Include(x => x.FinancialTransactions)
                .Where(x => x.FinancialSettleOn <= DateOnly.FromDateTime(nowInEst) 
                    && !x.IsFinancialSettled
                    && (x.Type == TradeType.ClientTrade || x.Type == TradeType.DealerTrade)
                    && !x.CancelledOnUtc.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var trade in tradesToSettle)
            {
                try
                {
                    var transaction = await _financialService.CreateFinancialTransactionAsync(
                        trade.Type.ToActivityType(),
                        trade.Side.ToTransactionSideType(),
                        BalanceType.Actual,
                        trade.GetTotalAmount(),
                        trade.Id,
                        cancellationToken
                    );

                    trade.AddFinancialTransaction(transaction);
                    trade.MarkAsFinancialSettled();

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    failedTradeIds.Add(trade.Id);
                    _logger.LogError(ex, $"Error processing trade with ID: {trade.Id}. Skipping this trade.");
                }                
            }

            if (failedTradeIds.Any())
            {
                throw new FinancialSettlementJobException($"The following trades failed to process: [{string.Join(", ", failedTradeIds)}]");
            }
        }
    }
}
