namespace PreciousMetalsTradingSystem.Application.AMark.Exceptions
{
    public class AMarkApiCredentialsNotSetException : Exception
    {
        public AMarkApiCredentialsNotSetException() :
            base("Credentials must be set in order to consume API.")
        { 
        }
    }
}
