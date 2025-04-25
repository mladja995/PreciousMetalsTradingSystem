using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetSingle
{
    public class GetProductQueryValidator : AbstractValidator<GetProductQuery>
    {
        public GetProductQueryValidator() 
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
