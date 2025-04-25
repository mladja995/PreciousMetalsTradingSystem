using FluentValidation;
using PreciousMetalsTradingSystem.Application.Common.Models.Validator;

namespace PreciousMetalsTradingSystem.Application.Trading.Activity.Queries.GetCollection
{
    public class GetActivityQueryValidator : PaginatedQueryValidator<GetActivityQuery>
    {
        public GetActivityQueryValidator()
        {
        }
    }
}
