using System.Numerics;

namespace FishPieClient.Maths;

public readonly struct Ray(Vector3 origin, Vector3 direction)
{

    public readonly Vector3 Origin = origin;
    public readonly Vector3 Direction = Vector3.Normalize(direction);

    public float? Intersects(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        return Intersects(this, v0, v1, v2);
    }
    
    public static float? Intersects(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        var edge1 = v1 - v0;
        var edge2 = v2 - v0;

        var h = Vector3.Cross(ray.Direction, edge2);
        var a = Vector3.Dot(edge1, h);

        if (MathF.Abs(a) < float.Epsilon)
        {
            return null;
        }

        var f = 1.0f / a;
        var s = ray.Origin - v0;
        var u = f * Vector3.Dot(s, h);
        
        if (u is < 0.0f or > 1.0f)
        {
            return null;
        }

        var q = Vector3.Cross(s, edge1);
        var v = f * Vector3.Dot(ray.Direction, q);

        if (v < 0.0f || u + v > 1.0f)
        {
            return null;
        }

        var t = f * Vector3.Dot(edge2, q);
        return t > float.Epsilon ? t : null;
    }
    
}