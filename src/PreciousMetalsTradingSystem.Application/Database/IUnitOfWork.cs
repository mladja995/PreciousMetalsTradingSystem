namespace PreciousMetalsTradingSystem.Application.Database
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
