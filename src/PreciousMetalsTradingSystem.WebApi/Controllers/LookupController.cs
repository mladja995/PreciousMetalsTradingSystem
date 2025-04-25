using PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetProductsInfo;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PreciousMetalsTradingSystem.WebApi.Controllers
{
    public class LookupController : ApiBaseController
    {
        public LookupController(IMediator mediator) 
            : base(mediator)
        {
        }

        [HttpGet("products")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
        {
            var products = await Mediator.Send(
                new GetProductsInfoQuery(), cancellationToken);

            return OkResponse(products);
        }
    }
}
