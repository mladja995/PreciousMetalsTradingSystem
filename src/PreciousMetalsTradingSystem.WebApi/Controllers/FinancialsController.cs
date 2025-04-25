using PreciousMetalsTradingSystem.Application.Financials.Adjustments.Commands.Create;
using PreciousMetalsTradingSystem.Application.Financials.Queries.GetCurrentBalance;
using PreciousMetalsTradingSystem.Application.Financials.Queries.GetTransactions;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PreciousMetalsTradingSystem.WebApi.Controllers
{
    public class FinancialsController : ApiBaseController
    {
        public FinancialsController(IMediator mediator)
            : base(mediator)
        {
        }

        [HttpGet("balance")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetCurrentBalance(
            CancellationToken cancellationToken)
        {
            var balance = await Mediator.Send(new GetCurrentBalanceQuery(),cancellationToken);
            return OkResponse(balance);
        }

        [HttpGet("transactions")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] GetTransactionsQuery request,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return OkResponse(response);
        }

        [HttpPost("adjustments")]
        [AuthorizePermission(Permission.ManageFinancialAdjustments)]
        public async Task<IActionResult> SubmitAdjustment(
            [FromBody] FinancialsAdjustmentCreateCommand request,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return CreatedResponse(response);
        }
    }
}
