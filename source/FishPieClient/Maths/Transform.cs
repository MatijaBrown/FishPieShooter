using System.Numerics;
using System.Runtime.CompilerServices;

namespace FishPieClient.Maths;

public class Transform(Vector3 position, Vector3 scale, Quaternion rotation)
{

    public Vector3 Position = position;
    public Vector3 Scale = scale;
    public Quaternion Rotation = rotation;

    public Transform(Matrix4x4 transform)
        : this(Vector3.Zero, Vector3.One, Quaternion.Identity)
    {
        FromMatrix(transform);
    }

    public void FromMatrix(Matrix4x4 transform)
    {
        if (!Matrix4x4.Decompose(transform, out Scale, out Rotation, out Position))
        {
            Position = transform.Translation;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Matrix4x4(Transform transform)
    {
        return Matrix4x4.Identity
            * Matrix4x4.CreateScale(transform.Scale)
            * Matrix4x4.CreateFromQuaternion(transform.Rotation)
            * Matrix4x4.CreateTranslation(transform.Position);
    }

    public override string ToString()
    {
        return $"pos:{Position}, scale:{Scale}, rot:{Rotation}";
    }
    
}