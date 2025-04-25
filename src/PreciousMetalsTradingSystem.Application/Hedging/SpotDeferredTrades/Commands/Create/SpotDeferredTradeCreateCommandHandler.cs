using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using LinqKit;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Commands.Create
{
    public class SpotDeferredTradeCreateCommandHandler : IRequestHandler<SpotDeferredTradeCreateCommand, Guid>
    {
        private readonly IRepository<HedgingAccount, HedgingAccountId> _hedgingAccountRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SpotDeferredTradeCreateCommandHandler(
            IRepository<HedgingAccount, HedgingAccountId> hedgingAccountRepository,
            IUnitOfWork unitOfWork)
        {
            _hedgingAccountRepository = hedgingAccountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(SpotDeferredTradeCreateCommand request, CancellationToken cancellationToken)
        {
            var hedgingAccountId = new HedgingAccountId(request.AccountId);
            
            var hedgingAccount = await _hedgingAccountRepository.GetByIdOrThrowAsync(
                id: hedgingAccountId,  
                cancellationToken: cancellationToken);

            var newSpotDeferredTrade = SpotDeferredTrade.Create(
                hedgingAccountId,
                request.TradeConfirmationNumber,
                request.SideType,
                DateOnly.FromDateTime(request.Date),
                true);

            request.Items.ForEach(item =>
            {
                newSpotDeferredTrade.AddItem(SpotDeferredTradeItem.Create(
                    item.MetalType,
                    new Money(item.SpotPricePerOz),
                    new QuantityOunces(item.QuantityOz)));
            });

            hedgingAccount.AddSpotDeferredTrade(newSpotDeferredTrade);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newSpotDeferredTrade.Id.Value;
        }
    }
}
