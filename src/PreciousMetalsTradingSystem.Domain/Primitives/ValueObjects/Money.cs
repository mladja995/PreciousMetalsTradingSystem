namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class Money : ValueObject
    {
        public decimal Value { get; }

        public Money(decimal value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Amount cannot be negative.");
            }

            Value = Math.Round(value, 2); // Ensure precision for monetary values
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => $"{Value:C}";

        public static implicit operator decimal(Money x) => x.Value;
    }
}
