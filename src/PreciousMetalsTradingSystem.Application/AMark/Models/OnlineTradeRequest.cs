namespace PreciousMetalsTradingSystem.Application.AMark.Models
{
    public class OnlineTradeRequest
    {
        public required string QuoteKey { get; init; }
        public bool HFIFlag { get; init; } = false;        
        public string ShippingType { get; init; } = string.Empty;
        public string ShippingName1 { get; init; } = string.Empty;
        public string ShippingName2 { get; init; } = string.Empty;
        public string ShippingAddress1 { get; init; } = string.Empty;
        public string ShippingAddress2 { get; init; } = string.Empty;
        public string ShippingCity { get; init; } = string.Empty;
        public string ShippingState { get; init; } = string.Empty;
        public string ShippingZipCode { get; init; } = string.Empty;
        public string ShippingCountry { get; init; } = string.Empty;
        public string ShippingPhoneNumber { get; init; } = string.Empty;
        public required string TPConfirmNo { get; init; }
    }

}
