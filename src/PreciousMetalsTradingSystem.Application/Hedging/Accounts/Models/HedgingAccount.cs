using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.Accounts.Models
{
    public class HedgingAccount
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public IEnumerable<LocationType> Locations { get; set; } = [];

        public static readonly Func<Domain.Entities.HedgingAccount, HedgingAccount> Projection =
          entity => new HedgingAccount
          {
              Id = entity.Id,
              Name = entity.Name,
              Code = entity.Code,
              Locations = entity.LocationHedgingAccountConfigurations.Select(x => x.Id.LocationType).ToList()
          };
    }
}
