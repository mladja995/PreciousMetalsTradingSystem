using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Exceptions;
using PreciousMetalsTradingSystem.Domain.DomainServices;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Cancel
{
    public class CancelTradeCommandHandler : IRequestHandler<CancelTradeCommand>
    {
        private readonly IRepository<Trade, TradeId> _tradeRepository;
        private readonly IRepository<FinancialTransaction, FinancialTransactionId> _financialTransactionRepository;
        private readonly IRepository<ProductLocationPosition, ProductLocationPositionId> _productLocationPositionRespository;
        private readonly IInventoryService _inventoryService;
        private readonly IFinancialsService _financialsService;
        private readonly IHedgingService _hedgingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITradeCancellationService _tradeCancellationService;

        public CancelTradeCommandHandler(
            IRepository<Trade, TradeId> tradeRepository, 
            IRepository<FinancialTransaction, FinancialTransactionId> financialTransactionRepository, 
            IRepository<ProductLocationPosition, ProductLocationPositionId> productLocationPositionRespository, 
            IInventoryService inventoryService, 
            IFinancialsService financialsService, 
            IHedgingService hedgingService, 
            IUnitOfWork unitOfWork,
            ITradeCancellationService tradeCancellationService)
        {
            _tradeRepository = tradeRepository;
            _financialTransactionRepository = financialTransactionRepository;
            _productLocationPositionRespository = productLocationPositionRespository;
            _inventoryService = inventoryService;
            _financialsService = financialsService;
            _hedgingService = hedgingService;
            _unitOfWork = unitOfWork;
            _tradeCancellationService = tradeCancellationService;
        }

        public async Task Handle(CancelTradeCommand request, CancellationToken cancellationToken)
        {
            var trade = await GetTradeByTradeNumberOrThrow(request.TradeNumber, cancellationToken);
            
            if (!trade.IsCancellationAllowed)
            {
                throw new TradeCancellationIsNotAllowedException();
            }

            var offsetTrade = _tradeCancellationService.CancelWithOffset(trade);

            await _tradeRepository.AddAsync(offsetTrade);

            await CreateAndSubmitOppositePositionsAsync(offsetTrade.Id, trade.Id, cancellationToken);

            await CreateAndSubmitOppositeTransactionsAsync(offsetTrade.Id, trade.Id, cancellationToken);

            // TODO: Unhedge if requst.AutoHedge is true
            // This will be implemented later in Post MVP phase
            // For now, traders will unhedge manually from Portal

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Trade> GetTradeByTradeNumberOrThrow(
            string tradeNumber, 
            CancellationToken cancellationToken = default)
        {
            var trade = await _tradeRepository
                .StartQuery(readOnly: false, asSplitQuery: true)
                .Include(x => x.Items)
                .ThenInclude(y => y.Product)
                .SingleOrDefaultAsync(x => x.TradeNumber.Equals(tradeNumber), cancellationToken);

            trade.ThrowIfNull(() => new NotFoundException(nameof(Trade), tradeNumber));

            return trade;
        }

        private async Task<IEnumerable<ProductLocationPosition>> CreateAndSubmitOppositePositionsAsync(
            TradeId offsetTradeId,
            TradeId orignalTradeId,
            CancellationToken cancellationToken)
        {
            List<ProductLocationPosition> positions = [];

            // Get positions related to original trade
            var originalTradePositions = await _productLocationPositionRespository
                .StartQuery(readOnly: true, asSplitQuery: false)
                .Where(x => x.TradeId.Equals(orignalTradeId))
                .ToListAsync(cancellationToken);

            // Iterate through original trade positions,
            // create opposite positions for offset trade,
            // submit new positions to repository
            foreach (var originalTradePosition in originalTradePositions)
            {
                var oppositePosition = await _inventoryService.CreatePositionAsync(
                    originalTradePosition.ProductId,
                    offsetTradeId,
                    originalTradePosition.LocationType,
                    originalTradePosition.Type,
                    originalTradePosition.SideType.ToOppositeSide(),
                    originalTradePosition.QuantityUnits,
                    cancellationToken);

                await _productLocationPositionRespository.AddAsync(oppositePosition, cancellationToken);
                positions.Add(oppositePosition);
            }

            return positions;
        }

        private async Task<IEnumerable<FinancialTransaction>> CreateAndSubmitOppositeTransactionsAsync(
            TradeId offsetTradeId,
            TradeId originalTradeId,
            CancellationToken cancellationToken)
        {
            List<FinancialTransaction> transactions = [];

            // Get transactions related to original trade
            var originalTradeTransactions = await _financialTransactionRepository
                .StartQuery(readOnly: true, asSplitQuery: false)
                .Where(x => x.TradeId!.Equals(originalTradeId))
                .ToListAsync(cancellationToken);

            // Iterate through original trade transactions,
            // create opposite transactions for offset trade,
            // submit new transactions to repository
            foreach (var originalTradeTransaction in originalTradeTransactions)
            {
                var oppositeTransaction = await _financialsService.CreateFinancialTransactionAsync(
                    ActivityType.OffsetTrade,
                    originalTradeTransaction.SideType.ToOppositeSide(),
                    originalTradeTransaction.BalanceType,
                    originalTradeTransaction.Amount,
                    offsetTradeId,
                    cancellationToken);

                await _financialTransactionRepository.AddAsync(oppositeTransaction, cancellationToken);
                transactions.Add(oppositeTransaction);
            }

            return transactions;
        }
    }
}
