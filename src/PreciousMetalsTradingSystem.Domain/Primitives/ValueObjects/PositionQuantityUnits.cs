namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class PositionQuantityUnits : ValueObject
    {
        public int Value { get; }

        public PositionQuantityUnits(int value)
        {
            Value = value;
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => Value.ToString();

        public static implicit operator int(PositionQuantityUnits x) => x.Value;
    }
}
