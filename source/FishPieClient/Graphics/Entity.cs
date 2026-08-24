using FishPieClient.Graphics.Mesh;

namespace FishPieClient.Graphics;

public readonly struct Entity(MeshView meshView)
{

    public readonly MeshView MeshView = meshView;

}