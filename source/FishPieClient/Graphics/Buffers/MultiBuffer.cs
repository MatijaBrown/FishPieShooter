using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Buffers;

/**
 * A multi buffer wrapper over a Buffer type. Will allocate size * frames amount of data and can advance
 * through the internal frames.
 */
public class MultiBuffer<TBuffer, T> : IBuffer<T>
    where TBuffer : class, IBuffer<T>
    where T : unmanaged
{
    
    private readonly int _byteSize;
    private readonly Func<uint, string, GL, TBuffer> _bufferConstructor;
    
    private int _frameOffset;
    
    public int Frames { get; }
    
    public TBuffer Buffer { get; }
    
    public string Name { get; }

    public uint Size { get; }
    
    public uint Handle => Buffer.Handle;

    public int FrameOffsetBytes => _frameOffset * _byteSize;

    public Func<uint, string, GL, IBuffer<T>> InstanceCreator => (size, name, gl)
        => new MultiBuffer<TBuffer, T>(size, name, gl, _bufferConstructor, Frames);

    public MultiBuffer(uint size, string name, GL gl, Func<uint, string, GL, TBuffer> bufferConstructor, int frames = 3)
    {
        Frames = frames;
        Size = size;
        Name = name;
        _bufferConstructor = bufferConstructor;

        _frameOffset = 0;
        _byteSize = Marshal.SizeOf<T>();

        Buffer = _bufferConstructor(size * (uint)Frames, name, gl);
    }
    
    public void Write(Span<T> data, int offset)
    {
        Buffer.Write(data, offset + _frameOffset);
    }

    public void Advance()
    {
        _frameOffset = (_frameOffset + (int)Size) % ((int)Size * Frames);
    }

    public void Dispose()
    {
        Buffer.Dispose();
    }
    
}