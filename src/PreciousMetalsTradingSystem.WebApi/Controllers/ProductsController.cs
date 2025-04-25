using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Create;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Update;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetCollection;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetSingle;
using PreciousMetalsTradingSystem.Application.Trading.Premiums.Queries;
using PreciousMetalsTradingSystem.WebApi.Common.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PreciousMetalsTradingSystem.WebApi.Controllers
{
    public sealed class ProductsController : ApiBaseController
    {
        public ProductsController(IMediator mediator) 
            : base(mediator)
        {
        }

        [HttpPost]
        [AuthorizePermission(Permission.ManageProducts)]
        public async Task<IActionResult> CreateProduct(
            [FromBody] ProductCreateCommand request,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return CreatedResponse(response);
        }

        [HttpPut("{id}")]
        [AuthorizePermission(Permission.ManageProducts)]
        public async Task<IActionResult> UpdateProduct(
            [FromRoute] Guid id, 
            [FromBody] ProductUpdateCommand request,
            CancellationToken cancellationToken)
        {
            await Mediator.Send(request, cancellationToken);
            return OkResponse();
        }

        [HttpGet("{id}")]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetProduct(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var product = await Mediator.Send(new GetProductQuery { Id = id }, cancellationToken);
            return OkResponse(product);
        }

        [HttpGet]
        [AuthorizePermission(Permission.ViewAllData)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] GetProductsQuery request,
            CancellationToken cancellationToken)
        {
            var products = await Mediator.Send(request, cancellationToken);

            return OkResponse(products);
        }

        [HttpGet("premiums")]
        [AuthorizePermission(Permission.Trading)]
        public async Task<IActionResult> GetPremiums(
            [FromQuery] GetPremiumsQuery request,
            CancellationToken cancellationToken)
        {
            var response = await Mediator.Send(request, cancellationToken);
            return OkResponse(response);
        }
    }
}
