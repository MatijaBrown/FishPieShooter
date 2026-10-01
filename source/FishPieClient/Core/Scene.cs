using System.Numerics;
using FishPieClient.Graphics.Materials;
using FishPieClient.Graphics.Mesh;
using FishPieClient.Maths;

namespace FishPieClient.Core;

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

    public IntersectionResult? IntersectRay(Ray ray)
    {
        IntersectionResult? result = null;
        float minDistance = float.MaxValue;
        
        foreach (var entity in Entities)
        {
            Matrix4x4.Invert((Matrix4x4)entity.Transform, out var invTransform);
            var transformedRay = new Ray(
                Vector3.Transform(ray.Origin, invTransform),
                Vector3.Transform(ray.Direction, invTransform)
            );

            var indices = entity.MeshView.Indices.Span;
            var vertices = entity.MeshView.Vertices.Span;
            
            for (int i = 0; i < entity.MeshView.Indices.Length; i += 3)
            {
                var v0 = vertices[(int)indices[i + 0]].Position;
                var v1 = vertices[(int)indices[i + 1]].Position;
                var v2 = vertices[(int)indices[i + 2]].Position;

                var distance = Ray.Intersect(transformedRay, v0, v1, v2);
                if (distance.HasValue)
                {
                    var intersectionPoint = transformedRay.Origin + transformedRay.Direction * distance.Value;

                    if (result == null || distance.Value < minDistance)
                    {
                        result = new IntersectionResult(entity, intersectionPoint);
                        minDistance = distance.Value;
                    }
                }
            }
        }

        return result;
    }

    public void Dispose()
    {
        
    }
}