using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Common.Models.Validator
{
    public class PaginatedQueryValidator<T> : AbstractValidator<T> where T : PaginatedQuery
    {
        public PaginatedQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than zero.");

            RuleFor(x => x.PageSize)
                .Must(pageSize => pageSize == null || pageSize > 0)
                .WithMessage("Page size must be greater than zero.");
        }
    }
}
