using PreciousMetalsTradingSystem.Domain.Entities;

namespace PreciousMetalsTradingSystem.Domain.DomainServices
{
    public interface ITradeFactory
    {
        Trade CreateOffsetTrade(Trade originalTrade);
    }
}
