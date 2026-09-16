using System.Numerics;
using System.Runtime.InteropServices;

namespace FishPieClient.Core;

[StructLayout(LayoutKind.Sequential)]
public struct CameraData
{

    public Matrix4x4 View;
    public Matrix4x4 Projection;

}