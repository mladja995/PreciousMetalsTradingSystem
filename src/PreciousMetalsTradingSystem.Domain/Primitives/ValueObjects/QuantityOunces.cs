namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class QuantityOunces : ValueObject
    {
        public decimal Value { get; }

        public QuantityOunces(decimal value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Quantity must be a positive value.");
            }

            Value = Math.Round(value, 4);
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();

        public static implicit operator decimal(QuantityOunces x) => x.Value;
    }
}
