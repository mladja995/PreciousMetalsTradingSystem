namespace PreciousMetalsTradingSystem.Application.Emailing.Models
{
    public class EmailAddress
    {
        public string? Caption { get; private set; }
        public string Address { get; private set; }

        public EmailAddress(string address, string? caption = null)
        {
            Address = address;
            Caption = caption;
        }
    }
}
