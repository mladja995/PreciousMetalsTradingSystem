using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.WebApi.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PreciousMetalsTradingSystem.WebApi.Controllers
{

    // TODO: Consider to add Produce attributes on controllers methods
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class ApiBaseController : ControllerBase
    {
        protected readonly IMediator Mediator;

        public ApiBaseController(IMediator mediator)
        {
            Mediator = mediator;
        }

        protected IActionResult CreatedResponse(object result)
        {
            var response = ApiResponse.Success(result, StatusCodes.Status201Created); 
            return StatusCode(StatusCodes.Status201Created, response);
        }

        protected IActionResult OkResponse(object? result = null, int statusCode = StatusCodes.Status200OK)
        {
            var pagedApiResponse = TryToHandlePagedQueryResult(result);
            if (pagedApiResponse != null)
            {
                return StatusCode(statusCode, pagedApiResponse);
            }

            var listApiResponse = TryToHandleListQueryResult(result);
            if (listApiResponse != null)
            {
                return StatusCode(statusCode, listApiResponse);
            }

            var apiResponse = ApiResponse.Success(result, statusCode);
            return StatusCode(statusCode, apiResponse);
        }

        private static object? TryToHandlePagedQueryResult(object? result)
        {
            if (result == null)
            {
                return null;
            }

            var resultType = result.GetType();
            var paginatedBaseType = resultType.BaseType;

            while (paginatedBaseType != null)
            {
                if (paginatedBaseType.IsGenericType && paginatedBaseType.GetGenericTypeDefinition() == typeof(PaginatedQueryResult<>))
                {
                    var itemType = paginatedBaseType.GetGenericArguments()[0];
                    return typeof(PagedApiResponse)
                        .GetMethod(nameof(PagedApiResponse.Success))
                        ?.MakeGenericMethod(itemType)
                        .Invoke(null, [result]);
                }
                paginatedBaseType = paginatedBaseType.BaseType;
            }

            return null;
        }

        private static object? TryToHandleListQueryResult(object? result)
        {
            if (result == null)
            {
                return null;
            }

            var resultType = result.GetType();
            var listBaseType = resultType.BaseType;

            while (listBaseType != null)
            {
                if (listBaseType.IsGenericType && listBaseType.GetGenericTypeDefinition() == typeof(ListQueryResult<>))
                {
                    var items = listBaseType.GetProperty(nameof(ListQueryResult<object>.Items))?.GetValue(result);
                    return ApiResponse.Success(items);
                }
                listBaseType = listBaseType.BaseType;
            }

            return null;
        }
    }
}
