using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Production.ValueObjects;

public sealed class Quantity : ValueObject
{
    public int Value { get; }

    private Quantity(int value) => Value = value;

    public static Quantity Zero => new(0);

    public static Quantity From(int value)
    {
        if (value < 0)
            throw new DomainException($"Quantity cannot be negative. Got: {value}.");
        return new(value);
    }

    public Quantity Add(int amount) => From(Value + amount);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator int(Quantity q) => q.Value;
    public override string ToString() => Value.ToString();
}
