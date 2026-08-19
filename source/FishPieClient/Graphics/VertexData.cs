using System.Numerics;
using System.Runtime.InteropServices;

namespace FishPieClient.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct VertexData
{
    
    public Vector3 Position;
    public Colour Colour; 
    
    public VertexData(Vector3 position)
    {
        Position = position;
        Colour = default;
    }

    public VertexData(Vector3 position, Colour colour)
    {
        Position = position;
        Colour = colour;
    }

    public VertexData(float x, float y, float z)
    {
        Position = new Vector3(x, y, z);
    }
    
}