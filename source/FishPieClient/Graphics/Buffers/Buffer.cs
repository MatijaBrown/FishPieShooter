using FishPieClient.Utils;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Buffers;

public class Buffer<T> : IBuffer<T>
    where T : unmanaged
{
    
    public static Buffer<T> Create(uint size, string name, GL gl)
        => new(size, name, gl);

    public static MultiBuffer<Buffer<T>, T> CreateMultiBuffer(uint size, string name, GL gl, int frames = 3)
        => new(size, name, gl, Create, frames);
    
    private readonly GL _gl;

    public uint Handle { get; }

    public string Name { get; }

    public uint Size { get; }

    public unsafe Buffer(uint size, string name, GL gl)
    {
        _gl = gl;
        Size = size;
        Name = name;

        Handle = _gl.CreateBuffer();
        _gl.ObjectLabel(ObjectIdentifier.Buffer, Handle, (uint)name.Length, name);
        
        _gl.NamedBufferStorage(Handle, Size * (uint)sizeof(T), null, BufferStorageMask.DynamicStorageBit);
    }

    public unsafe void Write(Span<T> data, int offset)
    {
        Errors.Expect(Size >= data.Length + offset, "buffer too small");
        _gl.NamedBufferSubData(Handle, offset * sizeof(T), data);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(Handle);
    }
    
}