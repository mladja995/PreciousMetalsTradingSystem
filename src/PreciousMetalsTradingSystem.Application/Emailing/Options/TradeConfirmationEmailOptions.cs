namespace PreciousMetalsTradingSystem.Application.Emailing.Options
{
    public class TradeConfirmationEmailOptions
    {
        public bool SendTradeConfirmationEmail { get; set; }
        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public string ToName { get; set; }
        public string ToAddresses { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
    }
}
