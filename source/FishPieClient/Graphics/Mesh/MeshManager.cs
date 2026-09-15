using System.Runtime.InteropServices;
using FishPieClient.Graphics.Buffers;
using Serilog;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Mesh;

public class MeshManager : IDisposable
{

    private readonly List<VertexData> _vertexDataCpu;
    private readonly List<uint> _indexDataCpu;
    private readonly GL _gl;
    
    private Buffer<VertexData> _vertexDataGpu;
    private Buffer<uint> _indexDataGpu;

    public (uint, uint) Handle => (_vertexDataGpu.Handle, _indexDataGpu.Handle);
    
    public MeshManager(GL gl)
    {
        _gl = gl;
        
        _vertexDataCpu = [];
        _indexDataCpu = [];
        _vertexDataGpu = new Buffer<VertexData>(1, "vertex_mesh_data", _gl);
        _indexDataGpu = new Buffer<uint>(1, "index_mesh_data", _gl);
    }

    private void ResizeGpuBuffer<T>(ICollection<T> cpuBuffer, ref Buffer<T> gpuBuffer)
        where T : unmanaged
    {
        var name = gpuBuffer.Name;
        var bufferSize = (uint)cpuBuffer.Count;

        if (gpuBuffer.Size <= bufferSize)
        {
            var newSize = gpuBuffer.Size * 2;
            while (newSize < bufferSize)
            {
                newSize *= 2;
            }

            Log.Information("growing {BufferName} buffer {OldSize} -> {NewSize}", name, gpuBuffer.Size,
                newSize);
            gpuBuffer.Dispose();
            
            // OpenGL barrier in case GPU using previous frame
            _gl.Finish();
            
            gpuBuffer = new Buffer<T>(newSize, name, _gl);
        }
    }

    public MeshView Load(MeshData meshData)
    {
        var vertexOffset = _vertexDataCpu.Count;
        var indexOffset = _indexDataCpu.Count;
        
        _vertexDataCpu.AddRange(meshData.Vertices);
        ResizeGpuBuffer(_vertexDataCpu, ref _vertexDataGpu);
        var vertexDataView = CollectionsMarshal.AsSpan(_vertexDataCpu);
        _vertexDataGpu.Write(vertexDataView, 0);
        
        _indexDataCpu.AddRange(meshData.Indices);
        ResizeGpuBuffer(_indexDataCpu, ref _indexDataGpu);
        var indexDataView = CollectionsMarshal.AsSpan(_indexDataCpu);
        _indexDataGpu.Write(indexDataView, 0);

        return new MeshView(
            IndexOffset: (uint)indexOffset,
            IndexCount: (uint)meshData.Indices.Count,
            VertexOffset: (uint)vertexOffset,
            VertexCount: (uint)meshData.Vertices.Count
        );
    }

    public override string ToString()
    {
        return $"mesh manager: vertex count {_vertexDataCpu.Count} index count: {_indexDataCpu.Count}";
    }

    public void Dispose()
    {
        _vertexDataGpu.Dispose();
        _indexDataGpu.Dispose();
    }
}