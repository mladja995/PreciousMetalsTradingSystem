using MediatR;
using PreciousMetalsTradingSystem.Application.Common.Models;
using Microsoft.Extensions.Options;
using PreciousMetalsTradingSystem.Application.Common.Options;

namespace PreciousMetalsTradingSystem.Application.Common.Behaviors
{
    public class PaginationPipelineBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly int _defaultPageSize;

        public PaginationPipelineBehavior(IOptions<ApiSettingsOptions> options)
        {
            _defaultPageSize = options.Value.DefaultPaginationPageSize;
        }

        public async Task<TResponse> Handle(TRequest request, 
            RequestHandlerDelegate<TResponse> next, 
            CancellationToken cancellationToken)
        {
            if (request is PaginatedQuery paginatedQuery 
                && (!paginatedQuery.PageSize.HasValue || paginatedQuery.PageSize <= 0))
            {
                paginatedQuery.PageSize = _defaultPageSize;
            }

            return await next();
        }
    }
}
