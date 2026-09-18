using FishPieClient.Graphics.Mesh;
using FishPieClient.Maths;

namespace FishPieClient.Graphics;

public class Entity
{

    public required MeshView MeshView { get; set; }

    public required Transform Transform { get; set; }
    
}