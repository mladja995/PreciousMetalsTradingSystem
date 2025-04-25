using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetDetails;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models.Notifications;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetSummary;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Events;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.EventHandlers
{
    /// <summary>
    /// Handler for SpotDeferredTradeCreatedEvent
    /// </summary>
    public class SpotDeferredTradeCreatedEventHandler :
        SignalRBaseNotificationHandler,
        INotificationHandler<DomainEventNotification<SpotDeferredTradeCreatedEvent>>
    {
        private readonly IRepository<SpotDeferredTrade, SpotDeferredTradeId> _repository;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new instance of SpotDeferredTradeCreatedEventHandler
        /// </summary>
        /// <param name="logger">Logger for this handler</param>
        /// <param name="publisher">Real-time notification publisher</param>
        /// <param name="repository">Repository for spot deferred trades</param>
        /// <param name="mediator">Mediator for sending operations</param>
        public SpotDeferredTradeCreatedEventHandler(
            ILogger<SpotDeferredTradeCreatedEventHandler> logger,
            IRealTimeNotificationPublisher publisher,
            IRepository<SpotDeferredTrade, SpotDeferredTradeId> repository,
            IMediator mediator)
            : base(logger, publisher)
        {
            _repository = repository;
            _mediator = mediator;
        }

        /// <summary>
        /// Handles the SpotDeferredTradeCreatedEvent
        /// </summary>
        /// <param name="notification">The notification containing the event</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        public async Task Handle(
            DomainEventNotification<SpotDeferredTradeCreatedEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;
            Logger.LogInformation("Spot deferred trade created: {SpotDeferredTradeId}",
                domainEvent.SpotDeferredTradeId);

            // Fetch all required data
            var data = await FetchRequiredDataAsync(domainEvent, cancellationToken);
            if (data == null)
            {
                return; // Error occurred, warning already logged
            }

            // Create the notification
            var signalRNotification = CreateNotification(domainEvent, data.Value);

            // Publish to Hedging hub
            await PublishNotification(HubType.Hedging, signalRNotification, cancellationToken);
        }

        /// <summary>
        /// Fetches all required data for the notification
        /// </summary>
        private async Task<(GetHedgingAccountDetailsQueryResult AccountDetails,
                            GetSpotDeferredTradesSummaryQueryResult TradesSummary,
                            SpotDeferredTrade SpotDeferredTrade)?> FetchRequiredDataAsync(
            SpotDeferredTradeCreatedEvent domainEvent,
            CancellationToken cancellationToken)
        {
            var accountDetails = await _mediator.Send(
                new GetHedgingAccountDetailsQuery { Id = domainEvent.HedgingAccountId.Value },
                cancellationToken);

            var tradesSummary = await _mediator.Send(
                new GetSpotDeferredTradesSummaryQuery { AccountId = domainEvent.HedgingAccountId.Value },
                cancellationToken);

            var spotDeferredTrade = await _repository.GetByIdAsync(
                id: domainEvent.SpotDeferredTradeId,
                readOnly: true,
                cancellationToken: cancellationToken,
                includes: [x => x.Items, x => x.Trades]);

            // Check if any of the results are default values (indicating an error occurred)
            if (accountDetails == default || tradesSummary == default || spotDeferredTrade is null)
            {
                Logger.LogWarning("One or more operations failed when preparing SpotDeferredTradeCreatedNotification. " +
                    "Skipping notification.");
                return null;
            }

            return (accountDetails, tradesSummary, spotDeferredTrade);
        }

        /// <summary>
        /// Creates a notification from the domain event and fetched data
        /// </summary>
        private static SpotDeferredTradeCreatedNotification CreateNotification(
            SpotDeferredTradeCreatedEvent domainEvent,
            (GetHedgingAccountDetailsQueryResult AccountDetails,
             GetSpotDeferredTradesSummaryQueryResult TradesSummary,
             SpotDeferredTrade SpotDeferredTrade) data)
        {
            var items = MapSpotDeferredTradeItems(data.SpotDeferredTrade, data.TradesSummary);

            return new SpotDeferredTradeCreatedNotification(
                HedgingAccountId: domainEvent.HedgingAccountId.ToString(),
                UnrealizedGainOrLossValue: data.AccountDetails.UnrealizedGainOrLossValue,
                Items: items
            );
        }

        /// <summary>
        /// Maps SpotDeferredTradeItems to notification items, joining with summary data by metal type
        /// </summary>
        private static IEnumerable<SpotDeferredTradeItemData> MapSpotDeferredTradeItems(
            SpotDeferredTrade spotDeferredTrade,
            GetSpotDeferredTradesSummaryQueryResult tradesSummary)
        {
            return spotDeferredTrade.Items.Select(item => {
                // Find matching summary data for this metal type
                // TODO: Possible exception when new Hedging Account without data introduced
                var summaryStat = tradesSummary.Items.Single(s => s.MetalType == item.Metal);

                // Get trade reference and type
                var (reference, tradeType) = GetTradeReferenceAndType(spotDeferredTrade);

                return new SpotDeferredTradeItemData(
                    TradeConfirmationReference: spotDeferredTrade.TradeConfirmationReference,
                    TradeReference: reference,
                    TradeType: tradeType,
                    TimestampUtc: spotDeferredTrade.TimestampUtc,
                    SideType: spotDeferredTrade.Side,
                    MetalType: item.Metal,
                    PricePerOz: item.PricePerOz,
                    QuantityOz: item.QuantityOz,
                    TotalAmount: item.TotalAmount,
                    SummaryActualTradedBalance: summaryStat.ActualTradedBalance,
                    SummaryNetAmount: summaryStat.NetAmount,
                    SummaryLastHedgingDate: summaryStat.LastHedgingDate
                );
            });
        }

        /// <summary>
        /// Gets the trade reference and type based on the number of related trades
        /// </summary>
        private static (string? Reference, TradeType? TradeType) GetTradeReferenceAndType(SpotDeferredTrade spotDeferredTrade)
        {
            string? reference = null;
            TradeType? tradeType = null;

            if (spotDeferredTrade.Trades.Count == 1)
            {
                var trade = spotDeferredTrade.Trades.First();
                reference = trade.TradeNumber;
                tradeType = trade.Type;
            }
            else if (spotDeferredTrade.Trades.Count > 1)
            {
                reference = "N/A";
            }

            return (reference, tradeType);
        }
    }
}