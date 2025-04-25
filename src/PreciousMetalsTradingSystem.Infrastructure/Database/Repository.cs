using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;
using PreciousMetalsTradingSystem.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using PreciousMetalsTradingSystem.Application.Common.Extensions;

namespace PreciousMetalsTradingSystem.Infrastructure.Database
{
    /// <summary>
    /// Generic repository implementation for working with aggregate roots
    /// </summary>
    /// <typeparam name="TEntity">The type of the aggregate entity</typeparam>
    /// <typeparam name="TEntityId">The type of the entity identifier</typeparam>
    public class Repository<TEntity, TEntityId> : IRepository<TEntity, TEntityId>
        where TEntity : AggregateRoot<TEntityId>
        where TEntityId : ValueObject, IEntityId
    {
        private readonly TradingSystemDbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(TradingSystemDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        public async Task<(IEnumerable<TEntity> items, int? totalCount)> GetAllAsync(
            Expression<Func<TEntity, bool>>? filter = null,
            bool readOnly = false,
            int? pageNumber = null,
            int? pageSize = null,
            string? sort= null,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;
            int? totalCount = null;

            if (readOnly)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            query = query.Sort(sort);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                totalCount = await query.CountAsync(cancellationToken);
                query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            var items = await query.ToListAsync(cancellationToken);

            return (items,totalCount);
        }

        public async Task<TEntity?> GetByIdAsync(
            TEntityId id,
            bool readOnly = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            if (readOnly)
            {
                query = query.AsNoTracking();
            }

            if (includes != null && includes.Length > 0)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        }

        public async Task<TEntity> GetByIdOrThrowAsync(
            TEntityId id,
            bool readOnly = false,
            CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var entity = await GetByIdAsync(id, readOnly, cancellationToken, includes);

            return entity is null ? throw new NotFoundException(typeof(TEntity).Name, id) : entity;
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public IQueryable<TEntity> StartQuery(bool readOnly = false, bool asSplitQuery = false)
        {
            IQueryable<TEntity> query = _dbSet;

            if (readOnly)
            {
                query = query.AsNoTracking();
            }

            if (asSplitQuery)
            {
                query = query.AsSplitQuery();
            }

            return query;
        }

        public void Remove(TEntity entity)
            => _dbSet.Remove(entity);
    }
}
