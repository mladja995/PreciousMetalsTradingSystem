using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Throw;

namespace PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetDetails
{
    public class GetHedgingAccountDetailsQueryHandler : IRequestHandler<GetHedgingAccountDetailsQuery, GetHedgingAccountDetailsQueryResult>
    {
        private readonly IRepository<HedgingAccount, HedgingAccountId> _repository;

        public GetHedgingAccountDetailsQueryHandler(IRepository
            <HedgingAccount, HedgingAccountId> repository)
        {
            _repository = repository;
        }

        public async Task<GetHedgingAccountDetailsQueryResult> Handle(
            GetHedgingAccountDetailsQuery request, CancellationToken cancellationToken)
        {
            var hedgingAccountId = new HedgingAccountId(request.Id);

            // NOTE: Performance are good with this approach for now.
            //       If we find performance issues this part can be refactored.
            var hedgingAccount = await _repository
                .StartQuery(readOnly: true, asSplitQuery: true)
                .Include(x => x.HedgingItems)
                .Include(x => x.SpotDeferredTrades)
                .ThenInclude(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id.Equals(hedgingAccountId), cancellationToken);

            hedgingAccount.ThrowIfNull(() => new NotFoundException(nameof(HedgingAccount), hedgingAccountId));

            return new GetHedgingAccountDetailsQueryResult
            {
                UnrealizedGainOrLossValue = hedgingAccount.CalculateUnrealizedGainOrLossValue()
            };
        }
    }
}
