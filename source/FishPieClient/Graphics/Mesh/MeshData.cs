namespace FishPieClient.Graphics.Mesh;

public record MeshData(
    List<VertexData> Vertices,
    List<uint> Indices
)
{
    
    public override string ToString()
    {
        return $"mesh data: |v|={Vertices.Count} |i|={Indices.Count}";
    }
    
}