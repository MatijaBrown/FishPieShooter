using System.Runtime.InteropServices;
using FishPieClient.Graphics.Buffers;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Mesh;

public sealed class MeshManager : IDisposable
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

    public MeshView Load(MeshData meshData)
    {
        var vertexOffset = _vertexDataCpu.Count;
        var indexOffset = _indexDataCpu.Count;
        
        _vertexDataCpu.AddRange(meshData.Vertices);
        _vertexDataGpu = Utils.ResizeGpuBuffer(_vertexDataCpu, _vertexDataGpu, _gl);
        var vertexDataView = CollectionsMarshal.AsSpan(_vertexDataCpu);
        _vertexDataGpu.Write(vertexDataView, 0);
        
        _indexDataCpu.AddRange(meshData.Indices);
        _indexDataGpu = Utils.ResizeGpuBuffer(_indexDataCpu, _indexDataGpu, _gl);
        var indexDataView = CollectionsMarshal.AsSpan(_indexDataCpu);
        _indexDataGpu.Write(indexDataView, 0);
        
        return new MeshView(
            IndexOffset: (uint)indexOffset,
            IndexCount: (uint)meshData.Indices.Count,
            VertexOffset: (uint)vertexOffset,
            VertexCount: (uint)meshData.Vertices.Count
        );
    }

    public Span<uint> IndexData(MeshView view)
    {
        return CollectionsMarshal.AsSpan(_indexDataCpu).Slice((int)view.IndexOffset, (int)view.IndexCount);
    }

    public Span<VertexData> VertexData(MeshView view)
    {
        return CollectionsMarshal.AsSpan(_vertexDataCpu).Slice((int)view.VertexOffset, (int)view.VertexCount);
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