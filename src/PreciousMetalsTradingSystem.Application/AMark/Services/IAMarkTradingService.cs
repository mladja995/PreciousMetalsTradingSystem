using PreciousMetalsTradingSystem.Application.AMark.Models;
using PreciousMetalsTradingSystem.Application.AMark.Options;

namespace PreciousMetalsTradingSystem.Application.AMark.Services
{
    public interface IAMarkTradingService
    {
        Task<QuoteResponse> RequestOnlineQuoteAsync( 
            OnlineQuoteRequest request,
            CancellationToken cancellationToken = default);
        Task<TradeResponse> RequestOnlineTradeAsync(
            OnlineTradeRequest request,
            CancellationToken cancellationToken = default);

        void SetCredentials(HedgingAccountCredential credentials);
    }
}
