using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.Entities
{
    public class Trade : AggregateRoot<TradeId>
    {
        private readonly List<TradeType> _allowedTradeTypesForCancellation = [TradeType.ClientTrade, TradeType.DealerTrade];
        private readonly List<TradeType> _allowedTradeTypesForPositionSettlement = [TradeType.ClientTrade, TradeType.DealerTrade];
        private readonly List<TradeType> _allowedTradeTypesForFinacialSettlement = [TradeType.ClientTrade, TradeType.DealerTrade];

        public bool IsCancellationAllowed =>
            _allowedTradeTypesForCancellation.Contains(Type) && !CancelledOnUtc.HasValue;

        public TradeQuoteId? TradeQuoteId { get; private set; }
        public SpotDeferredTradeId? SpotDeferredTradeId { get; private set; }
        public TradeId? OffsetTradeId { get; private set; }
        public string TradeNumber { get; private set; }
        public TradeType Type { get; private set; }
        public SideType Side { get; private set; }
        public LocationType LocationType { get; private set; }
        public DateOnly TradeDate { get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public bool IsPositionSettled { get; private set; } = false;
        public DateTime? PositionSettledOnUtc { get; private set; }
        public bool IsFinancialSettled { get; private set; } = false;
        public DateTime? FinancialSettledOnUtc { get; private set; }
        public DateOnly FinancialSettleOn { get; private set; }
        public DateTime? ConfirmedOnUtc {  get; internal set; }
        public DateTime? CancelledOnUtc { get; private set; }
        public string? Note { get; private set; }
        public DateTime LastUpdatedOnUtc { get; private set; }
        public string? ReferenceNumber { get; private set; }

        public virtual ICollection<TradeItem> Items { get; } = [];
        public virtual ICollection<FinancialTransaction> FinancialTransactions { get; } = [];
        public virtual Trade? OffsetTrade { get; internal set; }

        public static Trade Create(
            TradeType tradeType,
            SideType sideType,
            LocationType locationType,
            DateOnly tradeDate,
            DateOnly financialsSettleOn,
            string? note = null,
            string? referenceNumber = null)
        {
            var tradeId = TradeId.New();

            var trade = new Trade
            {
                Id = tradeId,
                Type = tradeType,
                Side = sideType,
                LocationType = locationType,
                TradeDate = tradeDate,
                FinancialSettleOn = financialsSettleOn,
                TimestampUtc = DateTime.UtcNow,
                Note = note,
                TradeNumber = GenerateTradeNumber(tradeDate, locationType),
                LastUpdatedOnUtc = DateTime.UtcNow,
                ReferenceNumber = referenceNumber
            };

            trade.AddDomainEvent(TradeCreatedEvent.FromEntity(trade));

            return trade;
        }

        public void AddItem(TradeItem item)
        {
            EnsureOnlyOneItemPerProduct(item.ProductId);
            Items.Add(item);
        }

        public void AddFinancialTransaction(FinancialTransaction transaction)
        {
            EnsureOnlyOneTransactionPerBalanceType(transaction.BalanceType);
            FinancialTransactions.Add(transaction);
        }

        public void SetTradeQuote(TradeQuoteId tradeQuoteId)
        {
            tradeQuoteId.ThrowIfNull();
            TradeQuoteId = tradeQuoteId;
        }

        public void SetSpotDeferredTrade(SpotDeferredTradeId spotDeferredTradeId)
        {
            spotDeferredTradeId.ThrowIfNull();
            SpotDeferredTradeId = spotDeferredTradeId;
        }

        private void SetOffsetTrade(Trade offsetTrade)
        {
            offsetTrade.ThrowIfNull();
            if (offsetTrade.Type != TradeType.OffsetTrade) 
            {
                throw new WrongTypeForOffsetTradeException();
            }
            OffsetTradeId = offsetTrade.Id;
        }

        public Dictionary<MetalType, QuantityOunces> GetQuantityPerMetalType()
        {
            return Items
                .GroupBy(x => x.Product.MetalType)
                .Select(x => new { MetalType = x.Key, Quantity = x.Sum(y => y.Product.WeightInOz * y.QuantityUnits) })
                .ToDictionary(x => x.MetalType, y => new QuantityOunces(y.Quantity));
        }

        public Money GetTotalAmount()
            => new(Items.Sum(x => x.TotalEffectivePrice));

        public void MarkAsSettled()
        {
            EnsureIsNotAlreadyPositionSettled();
            EnsurePositionSettlementIsAllowed();
            IsPositionSettled = true;
            PositionSettledOnUtc = DateTime.UtcNow;
            LastUpdatedOnUtc = DateTime.UtcNow;

            AddDomainEvent(TradePositionsSettledEvent.FromEntity(this));
        }

        public void MarkAsFinancialSettled()
        {
            EnsureIsNotAlreadyFinancialSettled();
            EnsureFinancialSettlementIsAllowed();
            IsFinancialSettled = true;
            FinancialSettledOnUtc = DateTime.UtcNow;
            LastUpdatedOnUtc = DateTime.UtcNow;

            AddDomainEvent(TradeFinancialSettledEvent.FromEntity(this));
        }

        public void MarkAsConfirmed()
        {
            EnsureIsNotCancelled();
            EnsureIsNotAlreadyConfirmed();
            ConfirmedOnUtc = DateTime.UtcNow;
            LastUpdatedOnUtc = DateTime.UtcNow;

            AddDomainEvent(TradeConfirmedEvent.FromEntity(this));
        }

        internal void MarkAsCancelledWithOffset(Trade offsetTrade)
        {
            EnsureCancellationIsAllowed();
            EnsureIsNotAlreadyCancelled();
            SetOffsetTrade(offsetTrade);
            CancelledOnUtc = DateTime.UtcNow;
            LastUpdatedOnUtc = DateTime.UtcNow;
        }

        #region Private

        private static string GenerateTradeNumber(DateOnly tradeDate, LocationType location)
        {
            var dateStr = tradeDate.ToString("yyMMdd");
            var timeStr = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"))
                .ToString("HHmmss");
            var locationStr = location switch
            {
                LocationType.SLC => "SLC",
                LocationType.NY => "NY",
                LocationType.IDS_DE => "IDS",
                _ => location.ToString()
            };
            
            return $"{dateStr}H{timeStr}{locationStr}";
        }

        #region Invariants

        private void EnsureOnlyOneItemPerProduct(ProductId productId)
        {
            if (Items.Any(x => x.ProductId.Equals(productId)))
            {
                throw new DuplicatedTradeItemPerProductException(productId);
            }
        }

        private void EnsureOnlyOneTransactionPerBalanceType(BalanceType balanceType)
        {
            if (FinancialTransactions.Any(x => x.BalanceType.Equals(balanceType)))
            {
                throw new DuplicatedFinancialTransactionPerBalanceType(balanceType);
            }
        }

        private void EnsureIsNotAlreadyFinancialSettled()
        {
            if (IsFinancialSettled)
            {
                throw new FinancialAlreadySettledException();
            }
        }

        private void EnsureIsNotAlreadyPositionSettled()
        {
            if (IsPositionSettled)
            {
                throw new PositionAlreadySettledException();
            }
        }

        private void EnsureIsNotAlreadyConfirmed()
        {
            if (ConfirmedOnUtc.HasValue)
            {
                throw new TradeAlreadyConfirmedException();
            }
        }

        private void EnsureCancellationIsAllowed()
        {
            if (!_allowedTradeTypesForCancellation.Contains(Type))
            {
                throw new TradeActionIsNotAllowedException(
                    $"Action is only allowed for trade types: " +
                    $"{string.Join(", ", _allowedTradeTypesForCancellation.Select(t => t.ToString()))}.");
            }
        }

        private void EnsurePositionSettlementIsAllowed()
        {
            EnsureIsNotCancelled();
            if (!_allowedTradeTypesForPositionSettlement.Contains(Type))
            {
                throw new TradeActionIsNotAllowedException(
                    $"Action is only allowed for trade types: " +
                    $"{string.Join(", ", _allowedTradeTypesForPositionSettlement.Select(t => t.ToString()))}.");
            }
        }

        private void EnsureFinancialSettlementIsAllowed()
        {
            EnsureIsNotCancelled();
            if (!_allowedTradeTypesForFinacialSettlement.Contains(Type))
            {
                throw new TradeActionIsNotAllowedException(
                    $"Action is only allowed for trade types: " +
                    $"{string.Join(", ", _allowedTradeTypesForFinacialSettlement.Select(t => t.ToString()))}.");
            }
        }

        private void EnsureIsNotAlreadyCancelled()
        {
            if (CancelledOnUtc.HasValue)
            {
                throw new TradeAlreadyCancelledException();
            }
        }

        private void EnsureIsNotCancelled()
        {
            if (CancelledOnUtc.HasValue)
            {
                throw new TradeActionIsNotAllowedException(
                    "Trade is cancelled, action is not allowed.");
            }
        }
        #endregion

        #endregion
    }
}
