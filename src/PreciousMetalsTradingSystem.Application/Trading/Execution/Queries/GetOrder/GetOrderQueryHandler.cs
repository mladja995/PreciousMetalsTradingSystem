using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using DomainEntities = PreciousMetalsTradingSystem.Domain.Entities;
using MediatR;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Throw;
using LinqKit;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Queries.GetOrder
{
    public  class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, ClientTrade>
    {
        private readonly IRepository<DomainEntities.Trade, TradeId> _repository;

        public GetOrderQueryHandler(IRepository<DomainEntities.Trade, TradeId> repository)
        {
            _repository = repository;
        }

        public async Task<ClientTrade> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            // TODO: Should we return cancelled trades?
            var trade = await _repository
                .StartQuery(readOnly: true, asSplitQuery: true)
                .AsExpandable()
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .Where(x => x.TradeNumber == request.OrderNumber && x.Type == Domain.Enums.TradeType.ClientTrade)
                .Select(ClientTrade.Projection)
                .SingleOrDefaultAsync();

            trade.ThrowIfNull(() => new NotFoundException(nameof(DomainEntities.Trade), request.OrderNumber));
            
            return trade;
        }
    }
}
