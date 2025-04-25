using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Common.Options;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Exceptions;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Exceptions;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Models;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Services;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Exceptions;
using PreciousMetalsTradingSystem.Application.Trading.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ExecuteQuote
{
    public class QuoteExecuteCommandHandler : IRequestHandler<QuoteExecuteCommand, QuoteExecuteCommandResult>
    {
        private readonly IRepository<TradeQuote, TradeQuoteId> _tradeQuoteRepository;
        private readonly IRepository<Trade, TradeId> _tradeRepository;
        private readonly IRepository<SpotDeferredTrade, SpotDeferredTradeId> _spotDeferredTradeRepository;
        private readonly IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId> _locationHedgingAccountRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IFinancialsService _financialsService;
        private readonly IHedgingService _hedgingService;
        private readonly ICalendarService _calendarService;
        private readonly ITradingService _tradingService;
        private readonly IRepository<FinancialTransaction, FinancialTransactionId> _financialTransactionRepository;
        private readonly IRepository<ProductLocationPosition, ProductLocationPositionId> _productLocationRespository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _tradeDuplicateLookupPeriodInDays;

        public QuoteExecuteCommandHandler(
            IRepository<TradeQuote, TradeQuoteId> tradeQuoteRepository,
            IRepository<Trade, TradeId> tradeRepository,
            IRepository<SpotDeferredTrade, SpotDeferredTradeId> spotDeferredTradeRepository,
            IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId> locationHedgingAccountRepository,
            IInventoryService inventoryService,
            IFinancialsService financialsService,
            IHedgingService hedgingService,
            ICalendarService calendarService,
            ITradingService tradingService,
            IRepository<FinancialTransaction, FinancialTransactionId> financialTransactionRepository,
            IRepository<ProductLocationPosition, ProductLocationPositionId> productLocationRepository,
            IUnitOfWork unitOfWork,
            IOptions<ApiSettingsOptions> options)
        {
            _tradeRepository = tradeRepository;
            _tradeQuoteRepository = tradeQuoteRepository;
            _spotDeferredTradeRepository = spotDeferredTradeRepository;
            _locationHedgingAccountRepository = locationHedgingAccountRepository;
            _inventoryService = inventoryService;
            _financialsService = financialsService;
            _hedgingService = hedgingService;
            _calendarService = calendarService;
            _financialTransactionRepository = financialTransactionRepository;
            _productLocationRespository = productLocationRepository;
            _unitOfWork = unitOfWork;
            _tradeDuplicateLookupPeriodInDays = options.Value.TradeDuplicateLookupPeriodInDays;
            _tradingService = tradingService;
        }

        public async Task<QuoteExecuteCommandResult> Handle(QuoteExecuteCommand request, CancellationToken cancellationToken)
        {
            //TODO: Edge Case handler if persit to DB fails - we have to unhedge what we previously hedged

            await ValidateUniqnesOfTradeReferenceNumberOrThrowAsync(request.ReferenceNumber, cancellationToken);

            var tradeQuote = await GetTradeQuoteOrThrowAsync(request.Id, cancellationToken);

            await ValidateCanWeTradeRequestedQuantityOrThrow(
                tradeQuote.LocationType,
                tradeQuote.Side,
                tradeQuote,
                tradeQuote.GetQuantityPerProduct(),
                cancellationToken);

            var trade = await ExecuteQuoteAndSubmitTradesAsync(tradeQuote, request.ReferenceNumber, cancellationToken);

            // Insert Available for Trading positions
            await CreateAndSubmitPositionsAsync(trade, cancellationToken);

            // Insert Effective Financial Transactions
            await CreateAndSubmitFinancialTransactionAsync(trade, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = new QuoteExecuteCommandResult
            {
                TradeId = trade.Id.Value,
                TradeNumber = trade.TradeNumber,
                ExecutedOnUtc = trade.TimestampUtc,
                Quote = tradeQuote.ToQuote()
            };

            return result;
        }
        private async Task ValidateUniqnesOfTradeReferenceNumberOrThrowAsync(
            string? referenceNumber,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(referenceNumber))
            {
                return;
            }
            else if (await IsReferenceNumberNotUniqueAsync(referenceNumber, cancellationToken))
            {
                throw new TradeReferenceNumberIsNotUniqueException(referenceNumber);
            }
        }
        private async Task<bool> IsReferenceNumberNotUniqueAsync(
                                  string referenceNumber,
                                  CancellationToken cancellationToken)
        {
            var lookbackPeriod = DateTime.UtcNow.AddDays(-_tradeDuplicateLookupPeriodInDays);

            var existingQuote = await _tradeRepository
                .StartQuery(readOnly: false)
                .Where(x => x.ReferenceNumber == referenceNumber &&
                            x.TimestampUtc >= lookbackPeriod)
                .FirstOrDefaultAsync(cancellationToken);

            return existingQuote is not null;
        }

        private async Task<TradeQuote> GetTradeQuoteOrThrowAsync(
            Guid tradeQuoteId,
            CancellationToken cancellationToken)
        {
            var tradeQuote = await _tradeQuoteRepository.StartQuery(readOnly: false)
                .Where(x => x.Id == new TradeQuoteId(tradeQuoteId))
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .SingleOrDefaultAsync(cancellationToken);

            //STEP: Validate Trade Quote
            if (tradeQuote is null)
            {
                throw new NotFoundException(nameof(TradeQuote), tradeQuoteId);
            }
            if (tradeQuote.Status != TradeQuoteStatusType.Pending)
            {
                throw new TradeQuoteInvalidStatusException(tradeQuote.Status);
            }
            if (tradeQuote.ExpiresAtUtc < DateTime.UtcNow)
            {
                throw new TradeQuoteExpiredException();
            }

            return tradeQuote;
        }

        private async Task ValidateCanWeTradeRequestedQuantityOrThrow(
            LocationType location,
            SideType side,
            TradeQuote tradeQuote,
            Dictionary<Product, QuantityUnits> quantityPerProduct,
            CancellationToken cancellationToken)
        {
            if (side == SideType.Buy)
            {
                await ValidateAgainstCashBalanceOrThrowAsync(
                    tradeQuote.Items.Sum(x => x.CalculateTotalAmount(x.Product.WeightInOz)),
                    cancellationToken);
            }
            else
            {
                await ValidateAgainstPositionsBalanceOrThrowAsync(
                    location,
                    quantityPerProduct,
                    cancellationToken);
            }
        }

        private async Task ValidateAgainstCashBalanceOrThrowAsync(
            decimal amountToValidate,
            CancellationToken cancellationToken)
        {
            var balanceType = BalanceType.Effective;

            var hashEnoughCash = await _financialsService.HasEnoughCashForBuyAsync(
                balanceType,
                new Money(amountToValidate),
                cancellationToken);

            hashEnoughCash
                .Throw(() => new NotEnoughCashForBuyException(balanceType))
                .IfFalse();
        }

        private async Task ValidateAgainstPositionsBalanceOrThrowAsync(
            LocationType location,
            Dictionary<Product, QuantityUnits> quantityPerProduct,
            CancellationToken cancellationToken)
        {
            var positionType = PositionType.AvailableForTrading;

            var hasEnoughQuantity = await _inventoryService
                .HasEnoughQuantityForSellAsync(
                    location,
                    PositionType.AvailableForTrading,
                    quantityPerProduct.ToDictionary(x => x.Key.Id, x => x.Value),
                    cancellationToken);

            hasEnoughQuantity
                .Throw(() => new NotEnoughQuantityForSellException(positionType))
                .IfFalse();
        }

        private async Task<Trade> ExecuteQuoteAndSubmitTradesAsync(
            TradeQuote tradeQuote,
            string? referenceNumber,
            CancellationToken cancellationToken)
        {
            var quantitiesPerMetalType = tradeQuote.GetQuantityPerMetalType();
            var spotPricesPerMetalType = tradeQuote.GetSpotPricesPerMetalType();

            var trade = await CreateClientTradeAsync(
                tradeQuote,
                referenceNumber,
                cancellationToken);

            var hedgeResult = await _hedgingService.ExecuteHedgeQuoteAsync(
                tradeQuote.LocationType,
                tradeQuote.DealerQuoteId,
                trade.TradeNumber,
                cancellationToken);

            var spotDeferredTrade = await CreateAndSubmitSpotDeferredTradeAsync(
                tradeQuote.LocationType,
                tradeQuote.Side.ToOppositeSide(),
                hedgeResult.TradeConfirmationNumber,
                quantitiesPerMetalType,
                spotPricesPerMetalType,
                cancellationToken);

            trade.SetSpotDeferredTrade(spotDeferredTrade.Id);

            await _tradeRepository.AddAsync(trade, cancellationToken);

            tradeQuote.MarkAsExecuted();            

            return trade;
        }

        private async Task<IEnumerable<ProductLocationPosition>> CreateAndSubmitPositionsAsync(
            Trade trade,
            CancellationToken cancellationToken)
        {
            List<ProductLocationPosition> positions = [];

            foreach (var item in trade.Items)
            {
                var position = await _inventoryService.CreatePositionAsync(
                    item.ProductId,
                    trade.Id,
                    trade.LocationType,
                    PositionType.AvailableForTrading,
                    trade.Side.ToPositionSideType(),
                    item.QuantityUnits,
                    cancellationToken);

                await _productLocationRespository.AddAsync(position, cancellationToken);
                positions.Add(position);
            }

            return positions;
        }

        private async Task<FinancialTransaction> CreateAndSubmitFinancialTransactionAsync(
            Trade trade,
            CancellationToken cancellationToken)
        {
            var transaction = await _financialsService.CreateFinancialTransactionAsync(
                trade.Type.ToActivityType(),
                trade.Side.ToTransactionSideType(),
                BalanceType.Effective,
                trade.GetTotalAmount(),
                trade.Id,
                cancellationToken);

            await _financialTransactionRepository.AddAsync(transaction, cancellationToken);

            return transaction;
        }

        private async Task<Trade> CreateClientTradeAsync(
            TradeQuote tradeQuote,
            string? referenceNumber,
            CancellationToken cancellationToken = default)
        {
            var settlementDate = await _tradingService.CalculateFinancialSettlementDateAsync(
                DateTime.UtcNow.ConvertUtcToEst(),
                CalendarType.FederalReserve, 
                cancellationToken);

            var trade = Trade.Create(
                TradeType.ClientTrade,
                tradeQuote.Side,
                tradeQuote.LocationType,
                DateTime.UtcNow.ConvertUtcToEstDateOnly(),
                settlementDate,
                tradeQuote.Note,
                referenceNumber);

            tradeQuote.Items
                .ForEach(item =>
                {
                    var tradeItem = TradeItem.Create(
                        tradeSide: tradeQuote.Side,
                        productId: item.Product.Id,
                        productWeightInOz: item.Product.WeightInOz,
                        quantityUnits: item.QuantityUnits,
                        spotPricePerOz: item.SpotPricePerOz,
                        tradePricePerOz: item.SpotPricePerOz,
                        premiumPerOz: item.PremiumPricePerOz,
                        effectivePricePerOz: item.EffectivePricePerOz);

                    tradeItem.SetProduct(item.Product);
                    tradeItem.SetTradeQuoteItem(item);
                    trade.AddItem(tradeItem);
                });

            trade.SetTradeQuote(tradeQuote.Id);

            return trade;
        }

        private async Task<SpotDeferredTrade> CreateAndSubmitSpotDeferredTradeAsync(
            LocationType location,
            SideType side,
            string tradeConfirmationReference,
            Dictionary<MetalType, QuantityOunces> quantitiesPerMetalType,
            Dictionary<MetalType, Money> pricesPerOzPerMetalType,
            CancellationToken cancellationToken)
        {
            var hedgingAccount = await GetHedgingAccountOrThrowAsync(location, cancellationToken);

            var spotDeferredTrade = SpotDeferredTrade.Create(
                hedgingAccount.Id,
                tradeConfirmationReference,
                side,
                DateTime.UtcNow.ConvertUtcToEstDateOnly());

            var spotDeferredTradeItems = quantitiesPerMetalType
                .Join(pricesPerOzPerMetalType,
                    i => i.Key,
                    p => p.Key,
                    (i, p) => new { Quantity = i, Price = p })
                .Select(item => SpotDeferredTradeItem.Create(
                    item.Quantity.Key,
                    item.Price.Value,
                    item.Quantity.Value)
                )
                .ToList();

            spotDeferredTradeItems.ForEach(spotDeferredTrade.AddItem);

            await _spotDeferredTradeRepository.AddAsync(spotDeferredTrade, cancellationToken);

            return spotDeferredTrade;
        }

        private async Task<HedgingAccount> GetHedgingAccountOrThrowAsync(
            LocationType location,
            CancellationToken cancellationToken)
        {
            // TODO: We should cache location hedging account configurations and pull it from there

            var locationHedgingAccountConfiguration = await
                _locationHedgingAccountRepository.GetByIdOrThrowAsync(
                    id: new LocationHedgingAccountConfigurationId(location),
                    cancellationToken: cancellationToken,
                    includes: x => x.HedgingAccount);

            return locationHedgingAccountConfiguration.HedgingAccount;
        }
    }
}
