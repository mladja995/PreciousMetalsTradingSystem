using MediatR;
using Microsoft.EntityFrameworkCore;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using ApplicationModels = PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetSummary
{
    public class GetSpotDeferredTradesSummaryQueryHandler : IRequestHandler<GetSpotDeferredTradesSummaryQuery, GetSpotDeferredTradesSummaryQueryResult>
    {
        private readonly IRepository<SpotDeferredTradeItem, SpotDeferredTradeItemId> _repository;

        public GetSpotDeferredTradesSummaryQueryHandler(IRepository<SpotDeferredTradeItem, SpotDeferredTradeItemId> repository)
        {
            _repository = repository;
        }

        public async Task<GetSpotDeferredTradesSummaryQueryResult> Handle(GetSpotDeferredTradesSummaryQuery request, CancellationToken cancellationToken)
        {
            var metalTypeSummaryItems = await _repository.StartQuery(readOnly: true)
                .Include(p => p.SpotDeferredTrade)
                .Where(p => p.SpotDeferredTrade.HedgingAccountId == new HedgingAccountId(request.AccountId))
                .GroupBy(p => p.Metal)
                .Select(g => new ApplicationModels.SpotDeferredTradeSummaryItem
                {
                    MetalType = g.Key,
                    ActualTradedBalance = g.Sum(x => (int)x.SpotDeferredTrade.Side * x.QuantityOz),
                    NetAmount = g.Sum(x => (int)x.SpotDeferredTrade.Side * x.TotalAmount),
                    LastHedgingDate = g.Max(x => x.SpotDeferredTrade.SpotDeferredTradeDate),
                })
                .ToListAsync(cancellationToken);

            return new GetSpotDeferredTradesSummaryQueryResult(metalTypeSummaryItems.ToList().AsReadOnly());
        }
    }
}
