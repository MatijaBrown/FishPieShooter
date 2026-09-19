using System.Numerics;
using System.Runtime.InteropServices;

namespace FishPieClient.Graphics.Data;

[StructLayout(LayoutKind.Explicit, Size = 80)]
public readonly struct ObjectData(Matrix4x4 model, uint materialIdIndex)
{

    [FieldOffset(0)]
    public readonly Matrix4x4 Model = model;
    
    [FieldOffset(64)]
    public readonly uint MaterialId = materialIdIndex;

}