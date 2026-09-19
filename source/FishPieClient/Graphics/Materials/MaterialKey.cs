using System.Diagnostics.CodeAnalysis;

namespace FishPieClient.Graphics.Materials;

public readonly struct MaterialKey(uint key) : IComparable<MaterialKey>
{

    public uint Get()
    {
        return key;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not MaterialKey other)
        {
            return false;
        }

        return Get() == other.Get();
    }

    public int CompareTo(MaterialKey other)
    {
        return Get().CompareTo(other.Get());
    }
    
    public override int GetHashCode()
    {
        return (int)Get();
    }

    public override string ToString()
    {
        return key.ToString();
    }

    public static bool operator ==(MaterialKey a, MaterialKey b)
    {
        return a.Get() == b.Get();
    }
    
    public static bool operator !=(MaterialKey a, MaterialKey b)
    {
        return a.Get() != b.Get();
    }
    
}