namespace PreciousMetalsTradingSystem.Application.Common.Models
{
    public abstract class ListQueryResult<T>
        where T : class
    {
        public IReadOnlyCollection<T> Items { get; }

        protected ListQueryResult(IReadOnlyCollection<T> items) => Items = items;
    }
}
