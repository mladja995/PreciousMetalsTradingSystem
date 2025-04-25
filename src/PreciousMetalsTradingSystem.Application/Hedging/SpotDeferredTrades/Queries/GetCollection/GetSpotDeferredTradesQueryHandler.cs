using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetCollection
{
    public class GetSpotDeferredTradesQueryHandler : IRequestHandler<GetSpotDeferredTradesQuery, GetSpotDeferredTradesQueryResult>
    {
        private readonly IRepository<SpotDeferredTrade, SpotDeferredTradeId> _repository;
        public GetSpotDeferredTradesQueryHandler(IRepository<SpotDeferredTrade, SpotDeferredTradeId> repository)
        {
            _repository = repository;
        }

        public async Task<GetSpotDeferredTradesQueryResult> Handle(GetSpotDeferredTradesQuery request, CancellationToken cancellationToken)
        {
            var fromDate = DateOnly.FromDateTime(request.FromDate ?? DateTime.MinValue);
            var toDate = DateOnly.FromDateTime(request.ToDate ?? DateTime.MaxValue);
            var spotDeferredTradesQuery = _repository.StartQuery(readOnly: true)
                .Include(x => x.Items)
                .Include(x => x.Trades)
                .Where(t =>
                    t.Items.Any(item => item.Metal == request.MetalType) &&
                    t.HedgingAccountId.Equals(new HedgingAccountId(request.AccountId)) &&
                    t.SpotDeferredTradeDate >= fromDate &&
                    t.SpotDeferredTradeDate <= toDate
                )
                .SelectMany(t => t.Items
                    .Where(item => item.Metal == request.MetalType)
                    .Select(item => new SpotDeferredTradeItemResult
                    {
                        DateUtc = t.TimestampUtc,
                        TradeConfirmationNumber = t.TradeConfirmationReference,
                        TradeType = t.Trades.Any() ? t.Trades.First().Type : null,
                        TradeReference = t.Trades.Count > 1 ? "N/A" : (t.Trades.Any() ? t.Trades.First().TradeNumber : null),
                        SideType = t.Side,
                        MetalType = item.Metal,
                        SpotPricePerOz = item.PricePerOz,
                        QuantityOz = item.QuantityOz,
                        TotalAmount = item.TotalAmount
                    })); 

            // Fetch the total count
            int totalCount = await spotDeferredTradesQuery.CountAsync(cancellationToken);

            // Apply pagination and fetch the result
            var itemsDto = await spotDeferredTradesQuery
                .Sort(!string.IsNullOrWhiteSpace(request.Sort) ? request.Sort : "-DateUtc") // TODO: Refactor default sort param
                .Skip((request.PageNumber - 1) * request.PageSize!.Value)
                .Take(request.PageSize!.Value)
                .ToListAsync(cancellationToken);

            // Return the result
            return new GetSpotDeferredTradesQueryResult(itemsDto, totalCount, request.PageNumber, request.PageSize!.Value);
        }
    }
}
