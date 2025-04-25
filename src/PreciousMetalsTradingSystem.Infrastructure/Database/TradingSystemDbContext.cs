using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Infrastructure.Database
{
    /// <summary>
    /// The main database context for the TradingSystem domain
    /// </summary>
    public sealed class TradingSystemDbContext : DbContext, IUnitOfWork
    {
        private readonly IDomainEventQueue _domainEventQueue;
        private readonly ILogger<TradingSystemDbContext> _logger;

        /// <summary>
        /// Initializes a new instance of the TradingSystemDbContext
        /// </summary>
        /// <param name="options">The database context options</param>
        /// <param name="domainEventQueue">Queue for domain events</param>
        /// <param name="logger">Logger for the database context</param>
        public TradingSystemDbContext(
            DbContextOptions<TradingSystemDbContext> options,
            IDomainEventQueue domainEventQueue,
            ILogger<TradingSystemDbContext> logger)
            : base(options)
        {
            _domainEventQueue = domainEventQueue ?? throw new ArgumentNullException(nameof(domainEventQueue));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Configures the database model
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TradingSystemDbContext).Assembly);
        }

        /// <summary>
        /// Saves all changes made in this context to the database and dispatches domain events
        /// </summary>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete</param>
        /// <returns>The number of state entries written to the database</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Collect domain events before saving changes
            var domainEvents = CollectDomainEvents();

            // Save changes to the database
            var result = await base.SaveChangesAsync(cancellationToken);

            // Dispatch domain events after successful save
            if (result > 0 && domainEvents.Any())
            {
                _logger.LogDebug("Successfully saved {Count} changes to database. Dispatching {EventCount} domain events.",
                    result, domainEvents.Count);

                // Enqueue domain events instead of processing them immediately
                _domainEventQueue.EnqueueEvents(domainEvents);
            }

            return result;
        }

        /// <summary>
        /// Saves all changes made in this context to the database and dispatches domain events
        /// </summary>
        /// <returns>The number of state entries written to the database</returns>
        public override int SaveChanges()
        {
            // For consistency, route all SaveChanges calls through the async version
            return SaveChangesAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Collects all domain events from tracked aggregate roots
        /// </summary>
        /// <returns>List of domain events to be dispatched</returns>
        private List<IDomainEvent> CollectDomainEvents()
        {
            // Find all aggregates that have domain events
            var aggregatesWithEvents = ChangeTracker.Entries<IAggregate>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            if (!aggregatesWithEvents.Any())
            {
                return [];
            }

            // Collect all domain events
            var allDomainEvents = aggregatesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear domain events from aggregates
            foreach (var aggregate in aggregatesWithEvents)
            {
                aggregate.ClearDomainEvents();
            }

            _logger.LogDebug("Collected {EventCount} domain events from {AggregateCount} aggregates",
                allDomainEvents.Count, aggregatesWithEvents.Count);

            return allDomainEvents;
        }
    }
}
