using FishPieClient.Core;
using FishPieClient.Graphics.Materials;
using FishPieClient.Graphics.Mesh;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics;

public class Scene : IDisposable
{

    public List<Entity> Entities { get; } = [];
    
    public MeshManager MeshManager { get; }
    
    public MaterialManager MaterialManager { get; }
    
    public Camera Camera { get; }

    public Scene(MeshManager meshManager, MaterialManager materialManager, Camera camera)
    {
        MeshManager = meshManager;
        MaterialManager = materialManager;
        Camera = camera;
    }

    public void Dispose()
    {
        
    }
}