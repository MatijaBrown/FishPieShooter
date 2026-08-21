using FishPieClient.Graphics;
using FishPieClient.Graphics.Buffers;

namespace BufferTests;

internal class FakeBuffer<T>(uint size, string name) : IBuffer<T>
    where T : unmanaged
{
    
    internal readonly List<(T[], int)> WriteCalls = [];
    internal uint Size = size;

    public string Name => name;
        
    public void Write(Span<T> data, int offset)
    {
        WriteCalls.Add((data.ToArray(), offset));
    }

    public void Dispose() { }
    
}