using PreciousMetalsTradingSystem.Application.AMark.Exceptions;
using PreciousMetalsTradingSystem.Application.AMark.Options;
using PreciousMetalsTradingSystem.Application.Common.Options;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using Microsoft.Extensions.Options;
using Throw;

namespace PreciousMetalsTradingSystem.Application.AMark.Services
{
    public class AMarkTradingServiceFactory : IAMarkTradingServiceFactory
    {
        private readonly IAMarkTradingService _service;
        private readonly AMarkOptions _options;
        private readonly IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId> _repository;
        private readonly ApiSettingsOptions _apiSettingsOptions;

        public AMarkTradingServiceFactory(
            IAMarkTradingService service, 
            IOptions<AMarkOptions> options,
            IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId> repository,
            IOptions<ApiSettingsOptions> apiSettingsOptions)
        {
            _service = service;
            _options = options.Value;
            _repository = repository;
            _apiSettingsOptions = apiSettingsOptions.Value;
        }

        public async Task<IAMarkTradingService> CreateAsync(
            LocationType locationType, CancellationToken cancellationToken)
        {
            if (!_apiSettingsOptions.UseMockAMarkTradingService)
            {
                var credentials = await GetCredentitalsOrThrow(locationType, cancellationToken);
                _service.SetCredentials(credentials);
            }
            return _service;
        }

        private async Task<HedgingAccountCredential> GetCredentitalsOrThrow(
            LocationType locationType,
            CancellationToken cancellationToken)
        {
            var locationHedgingAccountConfiguration = await _repository
                .GetByIdOrThrowAsync(
                    id: new LocationHedgingAccountConfigurationId(locationType),
                    readOnly: true,
                    cancellationToken: cancellationToken,
                    includes: x => x.HedgingAccount);

            var credentials = _options
                .HedgingAccountCredentials
                .FirstOrDefault(c => c.TradingPartnerId.Equals(locationHedgingAccountConfiguration.HedgingAccount.Code));

            credentials.ThrowIfNull(() => new AMarkHedgingAccountCredentialsNotConfiguredException(locationHedgingAccountConfiguration.HedgingAccount.Id));

            return credentials;
        }
    }
}
