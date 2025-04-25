using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Create
{
    public class HedgingItemCreateCommandHandler : IRequestHandler<HedgingItemCreateCommand, Guid>
    {
        private readonly IRepository<HedgingAccount, HedgingAccountId> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public HedgingItemCreateCommandHandler(
            IRepository<HedgingAccount, HedgingAccountId> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(HedgingItemCreateCommand request, CancellationToken cancellationToken)
        {
            var hedgingAccount = await _repository.GetByIdOrThrowAsync(
                id: new HedgingAccountId(request.AccountId),
                cancellationToken: cancellationToken);

            var newHedgingItem = HedgingItem.Create(
                hedgingAccount.Id,
                DateOnly.FromDateTime(request.Date),
                request.HedgingItemType,
                request.SideType,
                new Money(request.Amount),
                request.Note);

            hedgingAccount.AddHedgingItem(newHedgingItem);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newHedgingItem.Id.Value;
        }
    }
}
