using PreciousMetalsTradingSystem.Application.Common.Models;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetCollection
{
    public class GetProductsQuery : PaginatedQuery, IRequest<GetProductsQueryResult>
    {
    }
}
