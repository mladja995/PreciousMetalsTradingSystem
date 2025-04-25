using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.QuoteExpiration
{
    public class MarkTradeQuotesAsExpiredCommandHandler : IRequestHandler<MarkTradeQuotesAsExpiredCommand>
    {
        private readonly IRepository<TradeQuote, TradeQuoteId> _tradeQuoteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkTradeQuotesAsExpiredCommandHandler(
            IRepository<TradeQuote, TradeQuoteId> tradeQuoteRepository,
            IUnitOfWork unitOfWork)
        {
            _tradeQuoteRepository = tradeQuoteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(MarkTradeQuotesAsExpiredCommand request, CancellationToken cancellationToken)
        {
            var (tradeQuotes, totalCount) = await _tradeQuoteRepository.GetAllAsync(
                filter: x => x.Status == Domain.Enums.TradeQuoteStatusType.Pending
                    && x.ExpiresAtUtc < DateTime.UtcNow,
                readOnly: false,
                cancellationToken: cancellationToken);

            foreach (var tradeQuote in tradeQuotes)
            {
                //STEP: mark Trade Quote as Expired
                tradeQuote.MarkAsExpired();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
