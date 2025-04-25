using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Application.AMark.Exceptions
{
    public class AMarkHedgingAccountCredentialsNotConfiguredException : Exception
    {
        public AMarkHedgingAccountCredentialsNotConfiguredException(HedgingAccountId accountId)
            : base($"Credentials for Hedging account '{accountId}' was not configured.")
        {
        }
    }
}
