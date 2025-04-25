namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class HedgingAccountCode : ValueObject
    {
        public const int MaxLength = 20;

        public string Value { get; }

        public HedgingAccountCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Hedging account code cannot be empty.");
            }

            if (value.Length > MaxLength)
            {
                throw new ArgumentException($"Hedging account code cannot exceed {MaxLength} characters.");
            }

            Value = value;
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(HedgingAccountCode code) => code.Value;
    }
}
