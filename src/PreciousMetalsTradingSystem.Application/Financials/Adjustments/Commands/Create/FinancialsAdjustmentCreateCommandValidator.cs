using FluentValidation;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.Application.Financials.Adjustments.Commands.Create
{
    public class FinancialsAdjustmentCreateCommandValidator :  AbstractValidator<FinancialsAdjustmentCreateCommand>
    {
        public FinancialsAdjustmentCreateCommandValidator()
        {
            RuleFor(x => x.Date)
               .NotEmpty().WithMessage("Date cannot be empty.");

            RuleFor(x => x.SideType)
               .IsInEnum().WithMessage("SideType must be a valid enum value.");

            RuleFor(x => x.Amount)
               .NotEmpty().WithMessage("Amount cannot be empty or zero.");
        }
       
    }
}
