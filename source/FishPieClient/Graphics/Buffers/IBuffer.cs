namespace FishPieClient.Graphics.Buffers;

public interface IBuffer<T> : IDisposable
    where T : unmanaged
{
    
    public string Name { get; }
    
    public void Write(Span<T> data, int offset);

}