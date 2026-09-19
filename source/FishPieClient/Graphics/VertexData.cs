using System.Numerics;
using System.Runtime.InteropServices;

namespace FishPieClient.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct VertexData
{
    
    public Vector3 Position;
    
    public VertexData(Vector3 position)
    {
        Position = position;
    }

    public VertexData(float x, float y, float z)
    {
        Position = new Vector3(x, y, z);
    }
    
}