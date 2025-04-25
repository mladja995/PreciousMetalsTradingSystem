using PreciousMetalsTradingSystem.Application.AMark.Exceptions;
using PreciousMetalsTradingSystem.Application.AMark.Models;
using PreciousMetalsTradingSystem.Application.AMark.Options;
using PreciousMetalsTradingSystem.Application.AMark.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Json;
using Throw;

namespace PreciousMetalsTradingSystem.Infrastructure.Services
{
    public class AMarkTradingService : IAMarkTradingService
    {
        private const string REQUEST_ONLINE_QUOTE_ROUTE = "RequestOnlineQuote";
        private const string REQUEST_ONLINE_TRADE_ROUTE = "RequestOnlineTrade";

        private readonly HttpClient _httpClient;
        private readonly AMarkOptions _options;
        private HedgingAccountCredential _credentials;

        public AMarkTradingService(
            HttpClient httpClient,
            IOptions<AMarkOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            _httpClient.BaseAddress = new Uri(_options.Url);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<QuoteResponse> RequestOnlineQuoteAsync(
            OnlineQuoteRequest request,
            CancellationToken cancellationToken)
        {
            var requestPayload = BuildQuoteRequestPayload(request);

            var response = await _httpClient.PostAsJsonAsync(REQUEST_ONLINE_QUOTE_ROUTE, requestPayload, cancellationToken);
            
            return await ProcessResponse<QuoteResponse>(response, cancellationToken);
        }

        public async Task<TradeResponse> RequestOnlineTradeAsync(
            OnlineTradeRequest request,
            CancellationToken cancellationToken)
        {
            var requestPayload = BuildTradeRequestPayload(request);

            var response = await _httpClient.PostAsJsonAsync(REQUEST_ONLINE_TRADE_ROUTE, requestPayload, cancellationToken);
            
            return await ProcessResponse<TradeResponse>(response, cancellationToken);
        }


        #region Private

        private object BuildQuoteRequestPayload(OnlineQuoteRequest request)
        {
            _credentials.ThrowIfNull(() => new AMarkApiCredentialsNotSetException());

            return new
            {
                sAPIKey = _credentials.ApiKey,
                iTradingPartnerID = _credentials.TradingPartnerId,
                sEmailAddress = _credentials.EmailAddress,
                sOrderType = request.OrderType,
                lstProductQuoteItems = request.ProductQuoteItems.Select(item => new
                {
                    sProductCode = item.ProductCode,
                    decProductQuantity = item.ProductQuantity
                }).ToList(),
                bHFIFlag = request.HFIFlag,
                sShippingType = request.ShippingType,
                sShippingName1 = request.ShippingName1,
                sShippingName2 = request.ShippingName2,
                sShippingAddress1 = request.ShippingAddress1,
                sShippingAddress2 = request.ShippingAddress2,
                sShippingCity = request.ShippingCity,
                sShippingState = request.ShippingState,
                sShippingZipCode = request.ShippingZipCode,
                sShippingCountry = request.ShippingCountry,
                sShippingPhoneNumber = request.ShippingPhoneNumber,
                sTP_Confirm_No = request.TPConfirmNo,
                sSpecialInstructions = request.SpecialInstructions
            };
        }

        private object BuildTradeRequestPayload(OnlineTradeRequest request)
        {
            _credentials.ThrowIfNull(() => new AMarkApiCredentialsNotSetException());

            return new
            {
                sAPIKey = _credentials.ApiKey,
                iTradingPartnerID = _credentials.TradingPartnerId,
                sEmailAddress = _credentials.EmailAddress,
                sQuoteKey = request.QuoteKey,
                bHFIFlag = request.HFIFlag,
                sShippingType = request.ShippingType,
                sShippingName1 = request.ShippingName1,
                sShippingName2 = request.ShippingName2,
                sShippingAddress1 = request.ShippingAddress1,
                sShippingAddress2 = request.ShippingAddress2,
                sShippingCity = request.ShippingCity,
                sShippingState = request.ShippingState,
                sShippingZipCode = request.ShippingZipCode,
                sShippingCountry = request.ShippingCountry,
                sShippingPhoneNumber = request.ShippingPhoneNumber,
                sTP_Confirm_No = request.TPConfirmNo
            };
        }

        private static void HandleErrorIfPresent(string responseContent)
        {
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

            if (responseObject?.RequestStatus != 0)
            {
                var errorDataArray = responseObject?.ErrorData as Newtonsoft.Json.Linq.JArray;
                var firstError = errorDataArray?.FirstOrDefault();
                var errorCode = firstError?["ErrorCode"]?.ToString();
                var errorMessage = firstError?["ErrorDescription"]?.ToString();

                throw new AMarkApiException(
                    errorCode ?? "UnknownError", 
                    errorMessage ?? "Unknown error occurred in A-Mark API.");
            }
        }

        private static async Task<TResponse> ProcessResponse<TResponse>(
            HttpResponseMessage response, 
            CancellationToken cancellationToken)
        {
            response.EnsureSuccessStatusCode();
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            HandleErrorIfPresent(responseContent);

            var successData = JsonConvert.DeserializeObject<dynamic>(responseContent)?.SuccessData;

            return JsonConvert.DeserializeObject<TResponse>(successData?.ToString());
        }

        public void SetCredentials(HedgingAccountCredential credentials)
            => _credentials = credentials;
    }

    #endregion
}

