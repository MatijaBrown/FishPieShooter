using FishPieClient.Utils;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics;

public class Buffer<T> : IDisposable
    where T : unmanaged
{

    private readonly GL _gl;
    private readonly uint _size;
    
    public uint Handle { get; }

    public unsafe Buffer(uint size, GL gl)
    {
        _gl = gl;
        _size = size;

        Handle = _gl.CreateBuffer();
        _gl.NamedBufferStorage(Handle, _size * (uint)sizeof(T), null, BufferStorageMask.DynamicStorageBit);
    }

    public unsafe void Write(Span<T> data, int offset)
    {
        Errors.Expect(_size >= data.Length + offset, "buffer too small");
        _gl.NamedBufferSubData(Handle, offset * sizeof(T), data);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(Handle);
    }
    
}