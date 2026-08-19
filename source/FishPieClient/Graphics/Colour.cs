using System.Runtime.InteropServices;

namespace FishPieClient.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct Colour(float r, float g, float b)
{

    public static readonly Colour White = new(1.0f, 1.0f, 1.0f);
    public static readonly Colour Azure = new(0.0f, 0.5f, 1.0f);
    
    public float R = r;
    public float G = g;
    public float B = b;

    public override string ToString()
    {
        return $"r={R} g={G} b={B}";
    }
}