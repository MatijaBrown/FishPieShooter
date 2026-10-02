using System.Runtime.InteropServices;

namespace FishPieClient.Graphics.Mesh;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct MeshView(
    uint IndexOffset,
    uint IndexCount,
    uint VertexOffset,
    uint VertexCount
);