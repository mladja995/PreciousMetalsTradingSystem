using MediatR;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Models;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using DomainEntities = PreciousMetalsTradingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using LinqKit;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Queries.GetSingle
{
    public class GetTradeDetailsQueryHandler : IRequestHandler<GetTradeDetailsQuery, TradeDetailsResult>
    {
        private readonly IRepository<DomainEntities.Trade, TradeId> _repository;

        public GetTradeDetailsQueryHandler(IRepository<DomainEntities.Trade, TradeId> repository)
        {
            _repository = repository;
        }

        public async Task<TradeDetailsResult> Handle(GetTradeDetailsQuery request, CancellationToken cancellationToken)
        {
            var trade = await _repository
                .StartQuery(readOnly: true)
                .AsExpandable()
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .Where(x => x.Id == new TradeId(request.Id))
                .Select(TradeDetailsResult.Projection)
                .SingleOrDefaultAsync();

            return trade is null ? throw new NotFoundException(nameof(DomainEntities.Trade), request.Id) : trade;
        }
    }
}
