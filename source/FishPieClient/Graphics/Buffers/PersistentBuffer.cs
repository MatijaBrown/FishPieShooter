using System.Runtime.CompilerServices;
using FishPieClient.Utils;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Buffers;

public unsafe class PersistentBuffer<T> : IBuffer<T>
    where T : unmanaged
{
    
    public static PersistentBuffer<T> Create(uint size, string name, GL gl)
        => new(size, name, gl);

    public static MultiBuffer<PersistentBuffer<T>, T> CreateMultiBuffer(uint size, string name, GL gl,
        int frames = 3) => new(size, name, gl, Create, frames);
    
    private readonly GL _gl;
    private readonly uint _size;
    
    private readonly void* _map;
    
    public uint Handle { get; }
    
    public string Name { get; }

    public PersistentBuffer(uint size, string name, GL gl)
    {
        _gl = gl;
        _size = size;
        Name = name;

        Handle = _gl.CreateBuffer();
        _gl.ObjectLabel(ObjectIdentifier.Buffer, Handle, (uint)name.Length, name);
        
        _gl.NamedBufferStorage(Handle, _size * (uint)sizeof(T), null, BufferStorageMask.DynamicStorageBit
            | BufferStorageMask.MapWriteBit | BufferStorageMask.MapPersistentBit | BufferStorageMask.MapCoherentBit);
        _map = _gl.MapNamedBufferRange(Handle, 0, size * (uint)sizeof(T),
            MapBufferAccessMask.WriteBit | MapBufferAccessMask.PersistentBit | MapBufferAccessMask.CoherentBit);
        
    }

    public void Write(Span<T> data, int offset)
    {
        Errors.Expect(_size >= data.Length + offset, "buffer too small");
        fixed (void* ptr = data)
        {
            void* dest = Unsafe.Add<T>(_map, offset);
            Unsafe.CopyBlock(dest, ptr, (uint)(data.Length * sizeof(T)));
        }
    }

    public void Dispose()
    {
        _gl.UnmapNamedBuffer(Handle);
        _gl.DeleteBuffer(Handle);
    }
    
}