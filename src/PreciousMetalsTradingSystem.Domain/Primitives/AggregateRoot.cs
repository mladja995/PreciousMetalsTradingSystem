using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;

namespace PreciousMetalsTradingSystem.Domain.Primitives
{
    public abstract class AggregateRoot<TEntityId> : Entity<TEntityId>, IAggregate
        where TEntityId : ValueObject, IEntityId
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        protected AggregateRoot(TEntityId id) : base(id)
        {
        }

        protected AggregateRoot() : base()
        {
        }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
