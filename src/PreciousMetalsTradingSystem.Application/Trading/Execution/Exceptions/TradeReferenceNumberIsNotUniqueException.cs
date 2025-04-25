using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Exceptions
{
    public class TradeReferenceNumberIsNotUniqueException : ConflictException
    {
        public TradeReferenceNumberIsNotUniqueException(string message, string code = "CONFLICT_ERROR")
            : base(message, code)
        {
        }

        public TradeReferenceNumberIsNotUniqueException(string referenceNumber)
            : base($"A trade with reference number: '{referenceNumber}' already exists.")
        {
        }
    }
}
