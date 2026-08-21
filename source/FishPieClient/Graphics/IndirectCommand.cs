using System.Runtime.InteropServices;

namespace FishPieClient.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct IndirectCommand(uint count, uint instanceCount, uint first, uint baseInstance)
{
    
    public uint Count = count;
    
    public uint InstanceCount = instanceCount;
    
    public uint First = first;
    
    public uint BaseInstance = baseInstance;
    
}