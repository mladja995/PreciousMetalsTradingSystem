using PreciousMetalsTradingSystem.Application.Inventory.Queries.GetPositionsHistory;
using PreciousMetalsTradingSystem.Application.Inventory.Queries.GetState;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PreciousMetalsTradingSystem.WebApi.Controllers
{
    public class InventoryController : ApiBaseController
    {
        public InventoryController(IMediator mediator) 
            : base(mediator)
        {
        }

        [HttpGet("{location}/state")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetState(
            [FromRoute] LocationType location,
            [FromQuery] GetInventoryStateQuery request,
            CancellationToken cancellationToken)
        {
            var positions = await Mediator.Send(request, cancellationToken);

            return OkResponse(positions);
        }

        [HttpGet("{location}/products/{productSKU}/positions/history")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetPositionsHistory(
            [FromRoute] LocationType location,
            [FromRoute] string productSKU,
            [FromQuery] GetInventoryPositionsHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var positions = await Mediator.Send(request, cancellationToken);

            return OkResponse(positions);
        }
    }
}
