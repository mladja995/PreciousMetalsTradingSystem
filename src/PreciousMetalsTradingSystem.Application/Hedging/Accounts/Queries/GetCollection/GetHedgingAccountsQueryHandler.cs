using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DomainEntities = PreciousMetalsTradingSystem.Domain.Entities;

namespace PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetCollection
{
    public class GetHedgingAccountsQueryHandler : IRequestHandler<GetHedgingAccountsQuery, GetHedgingAccountsQueryResult>
    {
        private readonly IRepository<DomainEntities.HedgingAccount, HedgingAccountId> _repository;

        public GetHedgingAccountsQueryHandler(
            IRepository<DomainEntities.HedgingAccount, HedgingAccountId> repository)
        {
            _repository = repository;
        }

        public async Task<GetHedgingAccountsQueryResult> Handle(GetHedgingAccountsQuery request, CancellationToken cancellationToken)
        {
            var (hedgingAccounts, totalCount) = await _repository.GetAllAsync(
                readOnly: true,
                includes: x => x.LocationHedgingAccountConfigurations,
                cancellationToken: cancellationToken);

            return new GetHedgingAccountsQueryResult(hedgingAccounts.Select(Models.HedgingAccount.Projection).ToList());
        }
    }
}
