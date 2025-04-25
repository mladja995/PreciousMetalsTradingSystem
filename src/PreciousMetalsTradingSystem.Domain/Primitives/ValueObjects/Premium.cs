namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class Premium : ValueObject
    {
        public decimal Value { get; }

        public Premium(decimal value)
        {
            Value = Math.Round(value, 2);  
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => $"{Value:C2}";

        public static implicit operator decimal(Premium x) => x.Value;
    }
}
