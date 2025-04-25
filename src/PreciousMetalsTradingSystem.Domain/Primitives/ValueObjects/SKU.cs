namespace PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects
{
    public class SKU : ValueObject
    {
        public const int MaxLength = 50;

        public string Value { get; }

        public SKU(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > MaxLength)
            {
                throw new ArgumentException($"SKU must not be empty and cannot exceed {MaxLength} characters.");
            }

            Value = value;
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(SKU x) => x.Value;
    }
}
