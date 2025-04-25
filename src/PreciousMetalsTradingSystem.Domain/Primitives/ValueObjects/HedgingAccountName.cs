namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class HedgingAccountName : ValueObject
    {
        public const int MaxLength = 100;

        public string Value { get; }

        public HedgingAccountName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Hedging account name cannot be empty.");
            }

            if (value.Length > MaxLength)
            {
                throw new ArgumentException($"Hedging account name cannot exceed {MaxLength} characters.");
            }

            Value = value;
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(HedgingAccountName name) => name.Value;
    }
}
