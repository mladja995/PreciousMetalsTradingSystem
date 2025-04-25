using MediatR;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetSingle
{
    public class GetProductQuery : IRequest<Product>
    {
        public required Guid Id { get; init; }
    }
}
