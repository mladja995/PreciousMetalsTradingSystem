namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class QuantityUnits : ValueObject
    {
        public int Value { get; }

        public QuantityUnits(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Quantity must be a positive value.");
            }

            Value = value;
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();

        public static implicit operator int(QuantityUnits x) => x.Value;
    }
}
