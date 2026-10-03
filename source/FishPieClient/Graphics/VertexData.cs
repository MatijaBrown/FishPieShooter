using System.Numerics;
using System.Runtime.InteropServices;

namespace FishPieClient.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct VertexData
{
    
    public Vector3 Position;
    public Vector2 Uv;
    
    public VertexData(Vector3 position, Vector2 uv)
    {
        Position = position;
        Uv = uv;
    }

    public VertexData(float x, float y, float z, float u, float v)
    {
        Position = new Vector3(x, y, z);
        Uv = new Vector2(u, v);
    }
    
}