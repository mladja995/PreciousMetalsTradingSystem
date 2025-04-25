using System.Linq.Expressions;
using DomainEntities = PreciousMetalsTradingSystem.Domain.Entities;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Models
{
    public class TradeItemDetailsResult
    {
        public required Guid Id { get; init; }
        public required string ProductSKU { get; init; }
        public required string ProductName { get; init; }
        public required int UnitQuantity { get; init; }
        public required decimal PricePerOz { get; init; }
        public required decimal PremiumPerOz { get; init; }
        public required decimal EffectivePricePerOz { get; init; }
        public required decimal Amount { get; init; }
        public required decimal Revenue { get; init; }

        public static readonly Expression<Func<DomainEntities.TradeItem, TradeItemDetailsResult>> Projection = 
            item => new TradeItemDetailsResult
            {
                Id = item.Id,
                ProductSKU = item.Product.SKU,
                ProductName = item.Product.Name,
                UnitQuantity = item.QuantityUnits,
                PricePerOz = item.SpotPricePerOz,
                PremiumPerOz = item.PremiumPerOz,
                EffectivePricePerOz = item.EffectivePricePerOz,
                Amount = item.TotalEffectivePrice,
                Revenue = item.TotalRevenue
            };
    }
}
