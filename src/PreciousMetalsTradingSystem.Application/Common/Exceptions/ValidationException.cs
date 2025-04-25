using FluentValidation.Results;
using PreciousMetalsTradingSystem.Domain.Primitives.Exceptions;

namespace PreciousMetalsTradingSystem.Application.Common.Exceptions
{
    /// <summary>
    /// Exception for validation failures, typically thrown by FluentValidation in the ValidationBehaviour.
    /// Collects multiple validation errors and maps them to a dictionary by property name.
    /// </summary>
    public class ValidationException : TradingSystemApplicationException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException()
            : base("One or more validation failures have occurred.", "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }

        public override string ToString()
        {
            return base.ToString() +
                Environment.NewLine +
                string.Join(Environment.NewLine, Errors.Select(x => $"{x.Key}: {string.Join("; ", x.Value)}"));
        }
    }
}
