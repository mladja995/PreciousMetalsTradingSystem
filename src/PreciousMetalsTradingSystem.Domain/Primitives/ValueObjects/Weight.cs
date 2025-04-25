namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class Weight : ValueObject
    {
        public decimal Value { get; }

        public Weight(decimal value)
        {
            if (value <= 0 || value > 10000)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), 
                    "Weight must be a positive value and less than or equal to 1000.");
            }

            Value = Math.Round(value, 4); 
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => $"{Value} oz";

        public static implicit operator decimal(Weight x) => x.Value;
    }
}
