using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Update
{
    public class HedgingItemUpdateCommandHandler : IRequestHandler<HedgingItemUpdateCommand>
    {
        private readonly IRepository<HedgingItem, HedgingItemId> _repository;
        private readonly IUnitOfWork _unitOfWork;
        
        public HedgingItemUpdateCommandHandler(
            IRepository<HedgingItem, HedgingItemId> repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(HedgingItemUpdateCommand request, CancellationToken cancellationToken)
        {
            var hedgingAccountId = new HedgingAccountId(request.AccountId);

            var hedgingItem = await _repository.GetByIdOrThrowAsync(
                id: new HedgingItemId(request.HedgingItemId),
                cancellationToken: cancellationToken);

            if (hedgingItem.HedgingAccountId != hedgingAccountId)
            {
                throw new NotFoundException(nameof(HedgingAccount), hedgingAccountId);
            }

            hedgingItem.Update(
                DateOnly.FromDateTime(request.Date),
                request.HedgingItemType,
                request.SideType,
                new Money(request.Amount),
                request.Note);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
