using FishPieClient.Graphics.Mesh;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics;

public class Scene : IDisposable
{

    public List<Entity> Entities { get; } = [];
    
    public MeshManager MeshManager { get; }

    public Scene(GL gl)
    {
        MeshManager = new MeshManager(gl);
    }

    public void Dispose()
    {
        MeshManager.Dispose();
    }
}