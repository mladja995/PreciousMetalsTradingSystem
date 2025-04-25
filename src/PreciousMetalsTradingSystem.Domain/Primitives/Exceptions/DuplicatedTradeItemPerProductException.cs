using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class DuplicatedTradeItemPerProductException : DomainRuleViolationException
    {
        public DuplicatedTradeItemPerProductException(ProductId productId) 
            : base($"Item for product with Id '{productId}' already exists.")
        {
        }
    }
}
