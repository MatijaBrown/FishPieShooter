using FishPieClient.Graphics.Buffers;
using Silk.NET.OpenGL;

namespace BufferTests;

internal class FakeBuffer<T>(uint size, string name) : IBuffer<T>
    where T : unmanaged
{
    
    internal readonly List<(T[], int)> WriteCalls = [];

    public string Name => name;

    public uint Handle => uint.MaxValue;

    public uint Size => size;

    public Func<uint, string, GL, IBuffer<T>> InstanceCreator =>
        (s, n, _) => new FakeBuffer<T>(s, n);

    public void Write(Span<T> data, int offset)
    {
        WriteCalls.Add((data.ToArray(), offset));
    }

    public void Dispose() { }
    
}