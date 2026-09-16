using FishPieClient.Core;
using FishPieClient.Graphics.Mesh;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics;

public class Scene : IDisposable
{

    public List<Entity> Entities { get; } = [];
    
    public MeshManager MeshManager { get; }
    
    public Camera Camera { get; }

    public Scene(MeshManager meshManager, Camera camera)
    {
        MeshManager = meshManager;
        Camera = camera;
    }

    public void Dispose()
    {
        
    }
}