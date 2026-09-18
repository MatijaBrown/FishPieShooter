using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Buffers;

public interface IBuffer<T> : IDisposable
    where T : unmanaged
{
    
    public string Name { get; }
    
    public uint Size { get; }
    
    public Func<uint, string, GL, IBuffer<T>> InstanceCreator { get; }
    
    public uint Handle { get; }
    
    public void Write(Span<T> data, int offset);

}