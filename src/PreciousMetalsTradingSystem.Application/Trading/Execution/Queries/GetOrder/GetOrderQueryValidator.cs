using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Queries.GetOrder
{
    public class GetOrderQueryValidator : AbstractValidator<GetOrderQuery>
    {
        public GetOrderQueryValidator()
        {
            RuleFor(x => x.OrderNumber).NotEmpty();
        }
    }
}
