using System.Numerics;

namespace FishPieClient.Maths;

public struct Ray(Vector3 origin, Vector3 direction)
{

    public readonly Vector3 Origin = origin;
    public readonly Vector3 Direction = Vector3.Normalize(direction);
    
}