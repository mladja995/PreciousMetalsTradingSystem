using MediatR;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using DomainEntities = PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Application.Common.Exceptions;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetSingle
{
    public class GetHedgingItemQueryHandler : IRequestHandler<GetHedgingItemQuery, HedgingItem>
    {
        private readonly IRepository<DomainEntities.HedgingItem, HedgingItemId> _repository;

        public GetHedgingItemQueryHandler(IRepository<DomainEntities.HedgingItem, HedgingItemId> repository)
        {
            _repository = repository;
        }

        public async Task<HedgingItem> Handle(GetHedgingItemQuery request, CancellationToken cancellationToken)
        {
            var hedgingAccountId = new HedgingAccountId(request.AccountId);

            var hedgingItem = await _repository.GetByIdOrThrowAsync(
                id: new HedgingItemId(request.HedingItemId),
                readOnly: true,
                cancellationToken: cancellationToken);

            if (hedgingItem.HedgingAccountId != hedgingAccountId)
            {
                throw new NotFoundException(nameof(DomainEntities.HedgingAccount), hedgingAccountId);
            }

            return HedgingItem.Projection(hedgingItem);
        }
    }
}
