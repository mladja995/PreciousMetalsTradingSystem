using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.Application.Common.Exceptions
{
    public class FinancialSettlementJobException : TradingSystemApplicationException
    {
        public FinancialSettlementJobException(string message, string code = "FINANCIAL_SETTLEMENT_JOB")
            : base(message, code)
        {
        }
    }
}
