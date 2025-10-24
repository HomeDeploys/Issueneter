using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Issueneter.Domain.ValueObjects;

[DebuggerDisplay("{Value}")]
public readonly struct WorkerId
{
    private WorkerId(long value, bool bypassValidation)
    {
        if (!bypassValidation)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        }
        
        Value = value;
    }

    public WorkerId(long value) : this(value, false) { }
        
    public long Value { get; }
        
    public static WorkerId Empty => new(-1, true);

    public WorkerId ThrowIfEmpty()
    {
        if (Value < 0)
        {
            throw new ArgumentException("Worker Id is not set");
        }

        return this;
    }

    public bool IsEmpty() => Value < 0;

    public static bool operator ==(WorkerId left, WorkerId right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(WorkerId left, WorkerId right)
    {
        return left.Value != right.Value;
    }
    
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not WorkerId other)
        {
            return false;
        }
        
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}