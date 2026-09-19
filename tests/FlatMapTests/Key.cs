using System.Diagnostics.CodeAnalysis;

namespace FlatMapTests;

public readonly struct Key(uint key) : IComparable<Key>
{

    public uint Get()
    {
        return key;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Key other)
        {
            return false;
        }

        return Get() == other.Get();
    }

    public int CompareTo(Key other)
    {
        return Get().CompareTo(other.Get());
    }

    public override int GetHashCode()
    {
        return (int)Get();
    }

    public static bool operator ==(Key a, Key b)
    {
        return a.Get() == b.Get();
    }
    
    public static bool operator !=(Key a, Key b)
    {
        return a.Get() != b.Get();
    }
    
}