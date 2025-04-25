using PreciousMetalsTradingSystem.Application.Trading.Activity.Queries.GetCollection;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ExecuteQuote;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.RequestQuote;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Queries.GetOrder;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Queries.GetPrices;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Cancel;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Settle;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Queries.GetSingle;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PreciousMetalsTradingSystem.WebApi.Controllers
{
    public class TradingController : ApiBaseController
    {
        public TradingController(IMediator mediator) 
            : base(mediator)
        {
        }

        #region Trades
        [HttpGet("trades")] //TODO: Consider to change route to /activity
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetTrades(
            [FromQuery] GetActivityQuery request,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);

            return OkResponse(response);
        }

        [HttpGet("trades/{id}")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetTrade(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var request = new GetTradeDetailsQuery { Id = id };
            var response = await Mediator.Send(request, cancellationToken);

            return OkResponse(response);
        }

        [HttpPatch("trades/{id}/settle")]
        [AuthorizePermission(Permission.ManagePositions)]
        public async Task<IActionResult> SettleTrade(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            await Mediator.Send(new TradeSettleCommand() { Id = id }, cancellationToken);
            return OkResponse();
        }

        [HttpPost("trades/{tradeNumber}/cancel")]
        [AuthorizePermission(Permission.ManageTrades)]
        public async Task<IActionResult> CancelTrade(
            [FromRoute] string tradeNumber,
            [FromBody] CancelTradeCommand request,
            CancellationToken cancellationToken)
        {
            await Mediator.Send(request, cancellationToken);
            return OkResponse();
        }


        [HttpPost("dealer-trades")]
        [AuthorizePermission(Permission.ManageTrades)]
        public async Task<IActionResult> SubmitDealerTrade(
            [FromBody] DealerTradeCreateCommand request,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return CreatedResponse(response);
        }

        [AuthorizePermission(Permission.ManageTrades)]
        [HttpPost("client-trades")]
        public async Task<IActionResult> SubmitClientTrade(
            [FromBody] ClientTradeCreateCommand request,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return CreatedResponse(response);
        }

        #endregion

        #region Execution
        [HttpGet("prices")]
        [AuthorizePermission(Permission.Trading)]
        public async Task<IActionResult> GetPrices(
            [FromQuery] GetPricesQuery request,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return OkResponse(response);
        }

        [HttpPost("quotes")]
        [AuthorizePermission(Permission.Trading)]
        public async Task<IActionResult> RequestQuote(
            [FromBody] QuoteRequestCommand request,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return CreatedResponse(response);
        }

        [HttpPatch("quotes/{id}")]
        [AuthorizePermission(Permission.Trading)]
        public async Task<IActionResult> ExecuteQuote(
            [FromRoute] Guid id,
            [FromBody] QuoteExecuteCommand quoteExecuteCommand,
            CancellationToken cancellationToken)
        {
            quoteExecuteCommand.Id = id;
            var response = await Mediator.Send(quoteExecuteCommand, cancellationToken);
            return CreatedResponse(response);
        }

        [HttpGet("client-trades/{tradeNumber}")]
        [AuthorizePermission(Permission.Trading)]
        public async Task<IActionResult> GetClientTrade(
            [FromRoute] string tradeNumber,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(new GetOrderQuery { OrderNumber = tradeNumber }, cancellationToken);
            return OkResponse(response);
        }

        #endregion
    }
}
