using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Buffers;

/**
 * A multi buffer wrapper over a Buffer type. Will allocate size * frames amount of data and can advance
 * through the internal frames.
 */
public class MultiBuffer<TBuffer, T> : IBuffer<T>
    where TBuffer : IBuffer<T>
    where T : unmanaged
{

    private readonly uint _originalSize;

    private int _frameOffset;
    
    public int Frames { get; }
    
    public TBuffer Buffer { get; }
    
    public string Name { get; }

    public uint Size => Buffer.Size;

    public MultiBuffer(uint size, string name, GL gl, Func<uint, string, GL, TBuffer> bufferConstructor, int frames = 3)
    {
        Frames = frames;
        _originalSize = size;
        Name = name;

        _frameOffset = 0;

        Buffer = bufferConstructor(size * (uint)frames, name, gl);
    }
    
    public void Write(Span<T> data, int offset)
    {
        Buffer.Write(data, offset + _frameOffset);
    }

    public void Advance()
    {
        _frameOffset = (_frameOffset + (int)_originalSize) % ((int)_originalSize * Frames);
    }

    public void Dispose()
    {
        Buffer.Dispose();
    }
    
}