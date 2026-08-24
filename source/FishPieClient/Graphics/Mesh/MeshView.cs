using System.Runtime.InteropServices;

namespace FishPieClient.Graphics.Mesh;

[StructLayout(LayoutKind.Sequential)]
public record struct MeshView(uint Offset, uint Count);