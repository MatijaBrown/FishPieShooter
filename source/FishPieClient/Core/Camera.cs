using System.Numerics;

namespace FishPieClient.Core;

public class Camera
{

    private static Vector3 CreateDirection(float pitch, float yaw)
    {
        return Vector3.Normalize(new Vector3(
            MathF.Cos(yaw) * MathF.Cos(pitch), MathF.Sin(pitch), MathF.Sin(yaw) * MathF.Cos(pitch)
        ));
    }

    private CameraData _data;

    private Vector3 _position;
    
    private float _pitch;
    private float _yaw;

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            _data.View = Matrix4x4.CreateLookAt(_position, _position + Direction, Up);
        }
    }

    public Vector3 Direction { get; private set; }

    public Vector3 Up { get; private set; }
    
    public Vector3 Right { get; private set; }

    public float Pitch
    {
        get => _pitch;
        set => AdjustPitch(value - _pitch);
    }

    public float Yaw
    {
        get => _yaw;
        set => AdjustYaw(value - _yaw);
    }
    
    public float Fov { get; private set; }
    
    public float Width { get; private set; }
    
    public float Height { get; private set; }
    
    public float NearPlane { get; private set; }
    
    public float FarPlane { get; private set; }

    public CameraData Data => _data;

    public ref CameraData DataView => ref _data;

    public Camera(Vector3 position, Vector3 lookAt, Vector3 up, float fov, float width, float height, float nearPlane,
        float farPlane)
    {
        _data = new CameraData
        {
            View = Matrix4x4.CreateLookAt(position, lookAt, up),
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(fov, width / height, nearPlane, farPlane)
        };
        _position = position;
        Direction = lookAt;
        Up = up;
        Right = Vector3.Normalize(Vector3.Cross(Direction, Up));
        _pitch = 0.0f;
        _yaw = -MathF.PI / 2.0f;
        Fov = fov;
        Width = width;
        Height = height;
        NearPlane = nearPlane;
        FarPlane = farPlane;

        Direction = CreateDirection(_pitch, _yaw);
        _data.View = Matrix4x4.CreateLookAt(_position, _position + Direction, Up);
        AdjustPitch(0.0f);
    }

    public Camera(float width, float height, float depth)
    {
        _data = new CameraData()
        {
            View = Matrix4x4.CreateLookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY),
            Projection = Matrix4x4.CreateOrthographic(width, height, 0.0f, depth)
        };
        _position = Vector3.UnitZ;
        Direction = -Vector3.UnitZ;
        Up = Vector3.UnitY;
        Right = Vector3.Normalize(Vector3.Cross(Direction, Up));
        _pitch = 0.0f;
        _yaw = -MathF.PI / 2.0f;
        Fov = 0.0f;
        Width = width;
        Height = height;
        NearPlane = 0.0f;
        FarPlane = depth;
    }

    public void AdjustYaw(float adjust)
    {
        _yaw += adjust;
        Direction = CreateDirection(_pitch, _yaw);

        var worldUp = Vector3.UnitY;
        Right = Vector3.Normalize(Vector3.Cross(Direction, worldUp));
        Up = Vector3.Normalize(Vector3.Cross(Right, Direction));
        
        _data.View = Matrix4x4.CreateLookAt(_position, _position + Direction, Up);
    }

    public void AdjustPitch(float adjust)
    {
        const float epsilon = 0.0001f;
        
        _pitch += adjust;
        _pitch = MathF.Max(_pitch, -(MathF.PI / 2.0f) + epsilon);
        _pitch = MathF.Min(_pitch, (MathF.PI / 2.0f) - epsilon);
        Direction = CreateDirection(_pitch, _yaw);

        var worldUp = Vector3.UnitY;
        Right = Vector3.Normalize(Vector3.Cross(Direction, worldUp));
        Up = Vector3.Normalize(Vector3.Cross(Right, Direction));
        
        _data.View = Matrix4x4.CreateLookAt(_position, _position + Direction, Up);
    }

    public void Translate(Vector3 translation)
    {
        _position += translation;
        Direction = CreateDirection(_pitch, _yaw);
        _data.View = Matrix4x4.CreateLookAt(_position, _position + Direction, Up);
    }
    
}