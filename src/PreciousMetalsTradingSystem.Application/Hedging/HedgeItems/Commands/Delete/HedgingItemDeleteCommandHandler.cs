using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Delete
{
    public class HedgingItemDeleteCommandHandler : IRequestHandler<HedgingItemDeleteCommand>
    {
        private readonly IRepository<HedgingItem, HedgingItemId> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public HedgingItemDeleteCommandHandler(
            IRepository<HedgingItem, HedgingItemId> repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(HedgingItemDeleteCommand request, CancellationToken cancellationToken)
        {
            var hedgingAccountId = new HedgingAccountId(request.AccountId);

            var hedgingItem = await _repository.GetByIdOrThrowAsync(
                id: new HedgingItemId(request.Id),
                cancellationToken: cancellationToken);

            if (hedgingItem.HedgingAccountId != hedgingAccountId)
            {
                throw new NotFoundException(nameof(HedgingAccount), hedgingAccountId);
            }

            _repository.Remove(hedgingItem);
            hedgingItem.AddDomainEvent(HedgingItemDeletedEvent.FromEntity(hedgingItem));

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
