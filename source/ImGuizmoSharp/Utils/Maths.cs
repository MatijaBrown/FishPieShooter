using System.Numerics;

namespace ImGuizmoSharp.Utils;

internal static class Maths
{

    extension(Vector4 v)
    {
        internal float CLength()
        {
            return MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        internal float CLengthSq()
        {
            return v.X * v.X + v.Y * v.Y + v.Z * v.Z;
        }
    }

    internal static Vector4 Normalize(Vector4 v)
    {
        float length = v.CLength();
        return v * 1.0f / (length > float.Epsilon ? length : float.Epsilon);
    }

    internal static Vector4 Cross(Vector4 u, Vector4 v)
    {
        return Vector4.Cross(u, v) with { W = 0.0f };
    }

    internal static float Dot3(Vector4 u, Vector4 v)
    {
        return u.X * v.X + u.Y * v.Y + u.Z * v.Z;
    }

    internal static Vector4 BuildPlan(Vector4 point1, Vector4 normal)
    {
        normal = Normalize(normal);
        return normal with { W = Vector4.Dot(normal, point1) };
    }

    internal static Vector4 TransformPoint(Vector4 p, Matrix4x4 m)
    {
        return new Vector4(
            x: p.X * m.M11 + p.Y * m.M21 + p.Z * m.M31 + m.M41,
            y: p.X * m.M12 + p.Y * m.M22 + p.Z * m.M32 + m.M42,
            z: p.X * m.M13 + p.Y * m.M23 + p.Z * m.M33 + m.M43,
            w: p.X * m.M14 + p.Y * m.M24 + p.Z * m.M34 + m.M44
        );
    }

    internal static Vector4 TransformVector(Vector4 v, Matrix4x4 m)
    {
        return new Vector4(
            x: v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31,
            y: v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32,
            z: v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33,
            w: v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34
        );
    }

    extension(Matrix4x4 m)
    {
        internal Vector4 Right => m.X;

        internal Vector4 Up => m.Y;

        internal Vector4 Dir => m.Z;

        internal Vector4 Position => m.W;
    }

    internal static Matrix4x4 OrthoNormalize(Matrix4x4 m)
    {
        return new Matrix4x4()
        {
            X = Normalize(m.Right),
            Y = Normalize(m.Up),
            Z = Normalize(m.Dir),
            W = m.W
        };
    }

    internal static Vector4 PointOnSegment(Vector4 point, Vector4 vertPos1, Vector4 vertPos2)
    {
        var c = point - vertPos1;

        var v = Normalize(vertPos2 - vertPos1);
        float d = (vertPos2 - vertPos1).CLength();
        float t = Dot3(v, c);

        if (t < 0.0f)
        {
            return vertPos1;
        }

        if (t > d)
        {
            return vertPos2;
        }

        return vertPos1 + v * t;
    }

    internal static float IntersectRayPlane(Vector4 rOrigin, Vector4 rVector, Vector4 plan)
    {
        var numer = Dot3(plan, rOrigin) - plan.W;
        var denom = Dot3(plan, rVector);

        if (MathF.Abs(denom) < float.Epsilon)
        {
            return -1.0f;
        }

        return -(numer / denom);
    }

    internal static float DistanceToPlane(Vector4 point, Vector4 plan)
    {
        return Dot3(plan, point) + plan.W;
    }

    internal static float Clamp(float value, float min, float max)
    {
        value = MathF.Max(value, min);
        value = MathF.Min(value, max);
        return value;
    }
    
}