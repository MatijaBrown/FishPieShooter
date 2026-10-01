using System.Runtime.InteropServices;

namespace FishPieClient.Graphics.Mesh;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct MeshView(
    uint IndexOffset,
    Memory<uint> Indices,
    uint VertexOffset,
    Memory<VertexData> Vertices
);