using FishPieClient.Graphics.Mesh;

namespace FishPieClient.Graphics;

public class Entity(MeshView meshView)
{

    public MeshView MeshView { get; } = meshView;

}