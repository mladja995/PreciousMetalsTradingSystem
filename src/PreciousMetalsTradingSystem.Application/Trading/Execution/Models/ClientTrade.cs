using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using System.Linq.Expressions;
using LinqKit;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Models
{
    public class ClientTrade
    {
        public required Guid Id { get; init; }
        public required string TradeNumber { get; init; }   
        public required DateTime DateUtc { get; init; }
        public required ClientSideType Side { get; init; }
        public required LocationType Location { get; init; }
        public required TradeStatusType Status { get; init; } 
        public required IEnumerable<ClientTradeItem> Items { get; init; }
        public required decimal TotalAmount { get; init; }

        public static readonly Expression<Func<Trade, ClientTrade>> Projection = 
            trade => new ClientTrade
            {
                Id = trade.Id.Value,
                TradeNumber = trade.TradeNumber,
                DateUtc = trade.TimestampUtc,
                Side = trade.Side.ToClientSideType(),
                Location = trade.LocationType,
                Status = trade.ConfirmedOnUtc.HasValue ? TradeStatusType.Confirmed : TradeStatusType.Executed, // TODO: What about cancelled trades?
                TotalAmount = trade.Items.Sum(item => item.TotalEffectivePrice),
                Items = trade.Items.Select(x => ClientTradeItem.Projection.Invoke(x))
            };
    }
}
