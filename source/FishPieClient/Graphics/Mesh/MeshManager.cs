using System.Runtime.InteropServices;
using FishPieClient.Graphics.Buffers;
using Serilog;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Mesh;

public class MeshManager : IDisposable
{

    private readonly List<VertexData> _meshDataCpu;
    private readonly GL _gl;
    
    private Buffer<VertexData> _meshDataGpu;

    public uint Handle => _meshDataGpu.Handle;
    
    public MeshManager(GL gl)
    {
        _gl = gl;
        
        _meshDataCpu = new List<VertexData>(1);
        _meshDataGpu = new Buffer<VertexData>(1, "mesh_data", _gl);
    }

    public MeshView Load(ICollection<VertexData> mesh)
    {
        var offset = (uint)_meshDataCpu.Count;

        _meshDataCpu.AddRange(mesh);

        var bufferSize = (uint)_meshDataCpu.Count;

        if (_meshDataGpu.Size <= bufferSize)
        {
            var newSize = _meshDataGpu.Size * 2;
            while (newSize < bufferSize)
            {
                newSize *= 2;
            }

            Log.Information("growing {BufferName} buffer {OldSize} -> {NewSize}", _meshDataGpu.Name, _meshDataGpu.Size,
                newSize);
            _meshDataGpu.Dispose();
            _meshDataGpu = new Buffer<VertexData>(newSize, "mesh_data", _gl);
        }

        var meshView = CollectionsMarshal.AsSpan(_meshDataCpu);
        _meshDataGpu.Write(meshView, 0);

        return new MeshView(Offset: offset, Count: (uint)mesh.Count);
    }

    public override string ToString()
    {
        return $"mesh manager: vertex count {_meshDataCpu.Count}";
    }

    public void Dispose()
    {
        _meshDataGpu.Dispose();
    }
}