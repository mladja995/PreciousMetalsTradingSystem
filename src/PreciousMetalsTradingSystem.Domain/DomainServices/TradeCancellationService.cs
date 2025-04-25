using PreciousMetalsTradingSystem.Domain.Entities;
using Throw;

namespace PreciousMetalsTradingSystem.Domain.DomainServices
{
    // TODO: Move logic from application layer to Domain related only to aggregates
    public class TradeCancellationService : ITradeCancellationService
    {
        private readonly ITradeFactory _factory;

        public TradeCancellationService(ITradeFactory factory)
        {
            _factory = factory;
        }

        public Trade CancelWithOffset(Trade cancelationTrade)
        {
            cancelationTrade.ThrowIfNull();
            var offsetTrade = _factory.CreateOffsetTrade(cancelationTrade);
            cancelationTrade.MarkAsCancelledWithOffset(offsetTrade);

            return offsetTrade;
        }
    }
}
