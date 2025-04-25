using FluentValidation;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetCollection
{
    public class GetSpotDeferredTradesQueryValidator : AbstractValidator<GetSpotDeferredTradesQuery>
    {
        public GetSpotDeferredTradesQueryValidator()
        {
            RuleFor(x => x.AccountId).NotEmpty();
            RuleFor(x => x.MetalType).NotEmpty();
        }

    }

}
