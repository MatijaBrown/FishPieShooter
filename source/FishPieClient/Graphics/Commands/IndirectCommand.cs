using System.Runtime.InteropServices;

namespace FishPieClient.Graphics.Commands;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct IndirectCommand(uint Count, uint InstanceCount, uint First, uint BaseInstance);