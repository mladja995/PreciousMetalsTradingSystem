using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using PreciousMetalsTradingSystem.Domain.Primitives;
using System.Linq.Expressions;

namespace PreciousMetalsTradingSystem.Application.Database
{
    /// <summary>
    /// Generic repository interface for working with aggregate roots
    /// </summary>
    /// <typeparam name="TEntity">The type of the aggregate entity</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier</typeparam>
    public interface IRepository<TEntity, TEntityId>
        where TEntity : AggregateRoot<TEntityId>
        where TEntityId : ValueObject, IEntityId
    {
        Task<(IEnumerable<TEntity> items, int? totalCount)> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            bool readOnly = false,
            int? pageNumber = null,
            int? pageSize = null,
            string? sort = null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> GetByIdAsync(
            TEntityId id,
            bool readOnly = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity> GetByIdOrThrowAsync(
            TEntityId id,
            bool readOnly = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        void Remove(TEntity entity);

        IQueryable<TEntity> StartQuery(bool readOnly = false, bool asSplitQuery = false);
    }
}
