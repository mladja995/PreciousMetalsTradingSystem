namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class FinancialBalance : ValueObject
    {
        public decimal Value { get; }

        public FinancialBalance(decimal value)
        {
            Value = Math.Round(value, 2); // Ensure precision for monetary values
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => $"{Value:C}";

        public static implicit operator decimal(FinancialBalance x) => x.Value;
    }
}
