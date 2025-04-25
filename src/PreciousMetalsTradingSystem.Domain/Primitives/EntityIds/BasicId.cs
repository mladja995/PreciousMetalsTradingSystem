using PreciousMetalsTradingSystem.Domain.Primitives.Interfaces;

namespace PreciousMetalsTradingSystem.Domain.Primitives.EntityIds
{
    public abstract class BasicId : ValueObject, IEntityId
    {
        public Guid Value { get; }

        protected BasicId(Guid value)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException("Id cannot be empty", nameof(value));
            }

            Value = value;
        }

        public override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public static implicit operator Guid(BasicId x) => x.Value;

        public override string ToString() => Value.ToString();
    }

}
