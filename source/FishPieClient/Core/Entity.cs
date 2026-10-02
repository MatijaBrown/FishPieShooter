using FishPieClient.Graphics.Materials;
using FishPieClient.Graphics.Mesh;
using FishPieClient.Maths;

namespace FishPieClient.Core;

public class Entity
{

    public required string Name { get; init; }
    
    public required MeshView MeshView { get; set; }

    public required Transform Transform { get; set; }

    public required MaterialKey MaterialKey { get; set; }

}