using System.Numerics;
using System.Runtime.InteropServices;

namespace FishPieClient.Graphics.Data;

[StructLayout(LayoutKind.Sequential)]
public readonly struct ObjectData(Matrix4x4 model)
{

    public readonly Matrix4x4 Model = model;

}