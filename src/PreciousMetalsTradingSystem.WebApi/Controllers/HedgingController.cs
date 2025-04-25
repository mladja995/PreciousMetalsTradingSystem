using MediatR;
using Microsoft.AspNetCore.Mvc;
using PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetCollection;
using PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetDetails;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Create;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Update;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetCollection;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetSingle;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Commands.Create;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetCollection;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetSummary;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Delete;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization;

namespace PreciousMetalsTradingSystem.WebApi.Controllers
{
    public class HedgingController : ApiBaseController
    {
        public HedgingController(IMediator mediator)
            : base(mediator)
        {

        }

        #region Accounts
        [HttpGet("accounts")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetAccounts(
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(new GetHedgingAccountsQuery(), cancellationToken);
            return OkResponse(response);
        }

        [HttpGet("accounts/{id}/details")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetAccountDetails(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(
                new GetHedgingAccountDetailsQuery { Id = id }, 
                cancellationToken);

            return OkResponse(response);
        }
        #endregion

        #region SpotDeferredTrades
        [HttpPost("accounts/{id}/spot-deferred-trades")]
        [AuthorizePermission(Permission.ManageSpotDeferredTrades)]
        public async Task<IActionResult> SubmitSpotDeferredTrade(
            [FromRoute] Guid id,
            [FromBody] SpotDeferredTradeCreateCommand request,
            CancellationToken cancellationToken)
        {
            request.AccountId = id;
            var response = await Mediator.Send(request, cancellationToken);

            return CreatedResponse(response);
        }

        [HttpGet("accounts/{id}/spot-deferred-trades")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetSpotDeferredTrades(
            [FromRoute] Guid id,
            [FromQuery] GetSpotDeferredTradesQuery request,
            CancellationToken cancellationToken)
        {
            request.AccountId = id;
            var response = await Mediator.Send(request, cancellationToken);

            return OkResponse(response);
        }

        [HttpGet("accounts/{id}/spot-deferred-trades/summary")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetSpotDeferredTradesSummary(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var request = new GetSpotDeferredTradesSummaryQuery { AccountId = id };
            
            var summary = await Mediator.Send(request, cancellationToken);

            return OkResponse(summary);
        }
        #endregion

        #region HedgeItems
        [HttpPost("accounts/{id}/hedge-items")]
        [AuthorizePermission(Permission.ManageHedgingItems)]
        public async Task<IActionResult> SubmitHedgingItem(
            [FromRoute] Guid id,
            [FromBody] HedgingItemCreateCommand request,
            CancellationToken cancellationToken)
        {
            request.AccountId = id;
            var response = await Mediator.Send(request, cancellationToken);

            return OkResponse(response);
        }

        [HttpGet("accounts/{id}/hedge-items")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetHedgingItems(
            [FromRoute] Guid id,
            [FromQuery] GetHedgingItemsQuery request,
            CancellationToken cancellationToken)
        {
            request.AccountId = id;
            var hedgingItems = await Mediator.Send(request, cancellationToken);

            return OkResponse(hedgingItems);
        }

        [HttpGet("accounts/{accountId}/hedge-items/{hedgingItemId}/details")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetHedgingItem(
            [FromRoute] Guid accountId,
            [FromRoute] Guid hedgingItemId,
            CancellationToken cancellationToken)
        {
            var request = new GetHedgingItemQuery
            {
                AccountId = accountId,
                HedingItemId = hedgingItemId,
            };
            var hedgingItem = await Mediator.Send(request, cancellationToken);
            
            return OkResponse(hedgingItem);
        }

        [HttpDelete("accounts/{accountId}/hedge-items/{hedgingItemId}")]
        [AuthorizePermission(Permission.ManageHedgingItems)]
        public async Task<IActionResult> DeleteHedgingItem(
            [FromRoute] Guid accountId,
            [FromRoute] Guid hedgingItemId,
            CancellationToken cancellationToken)
        {
            await Mediator.Send(
                new HedgingItemDeleteCommand { AccountId = accountId, Id = hedgingItemId },
                cancellationToken);

            return OkResponse();
        }

        [HttpPut("accounts/{accountId}/hedge-items/{hedgingItemId}")]
        [AuthorizePermission(Permission.ManageHedgingItems)]
        public async Task<IActionResult> UpdateHedgingItem(
            [FromRoute] Guid accountId,
            [FromRoute] Guid hedgingItemId,
            [FromBody] HedgingItemUpdateCommand request,
            CancellationToken cancellationToken)
        {
            await Mediator.Send(request, cancellationToken);
            return OkResponse();
        }
        #endregion
    }
}
