using System.Runtime.InteropServices;
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
    
    private readonly int _byteSize;
    
    private int _frameOffset;
    
    public int Frames { get; }
    
    public TBuffer Buffer { get; }
    
    public string Name { get; }

    public uint Handle => Buffer.Handle;
    
    public uint OriginalSize { get; }

    public int FrameOffsetBytes => _frameOffset * _byteSize;

    public MultiBuffer(uint size, string name, GL gl, Func<uint, string, GL, TBuffer> bufferConstructor, int frames = 3)
    {
        Frames = frames;
        OriginalSize = size;
        Name = name;

        _frameOffset = 0;
        _byteSize = Marshal.SizeOf<T>();

        Buffer = bufferConstructor(size * (uint)frames, name, gl);
    }
    
    public void Write(Span<T> data, int offset)
    {
        Buffer.Write(data, offset + _frameOffset);
    }

    public void Advance()
    {
        _frameOffset = (_frameOffset + (int)OriginalSize) % ((int)OriginalSize * Frames);
    }

    public void Dispose()
    {
        Buffer.Dispose();
    }
    
}