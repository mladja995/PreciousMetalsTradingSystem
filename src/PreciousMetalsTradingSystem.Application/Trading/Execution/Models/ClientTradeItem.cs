using PreciousMetalsTradingSystem.Domain.Entities;
using System.Linq.Expressions;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Models
{
    public class ClientTradeItem
    {
        public required string ProductSKU { get; init; }
        public required double QuantityUnits { get; init; }
        public required decimal PricePerOz { get; init; }
        public required decimal TotalAmount { get; init; }

        public static readonly Expression<Func<TradeItem, ClientTradeItem>> Projection = 
            item => new ClientTradeItem
            {
                ProductSKU = item.Product.SKU,
                QuantityUnits = item.QuantityUnits,
                PricePerOz = item.EffectivePricePerOz,
                TotalAmount = item.TotalEffectivePrice
            };
    }
}
