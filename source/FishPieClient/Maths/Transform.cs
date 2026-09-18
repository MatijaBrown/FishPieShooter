using System.Numerics;

namespace FishPieClient.Maths;

public class Transform(Vector3 position, Vector3 scale, Quaternion rotation)
{

    public Vector3 Position = position;
    public Vector3 Scale = scale;
    public Quaternion Rotation = rotation;

    public static implicit operator Matrix4x4(Transform transform)
    {
        return Matrix4x4.Identity
            * Matrix4x4.CreateFromQuaternion(transform.Rotation)
            * Matrix4x4.CreateScale(transform.Scale)
            * Matrix4x4.CreateTranslation(transform.Position);
    }

    public override string ToString()
    {
        return $"pos:{Position}, scale:{Scale}, rot:{Rotation}";
    }
    
}