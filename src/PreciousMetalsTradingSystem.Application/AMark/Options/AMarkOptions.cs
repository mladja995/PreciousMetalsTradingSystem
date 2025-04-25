namespace PreciousMetalsTradingSystem.Application.AMark.Options
{
    public class AMarkOptions
    {
        public string Url { get; set; } 
        public List<HedgingAccountCredential> HedgingAccountCredentials { get; set; } = [];  
    }

}
