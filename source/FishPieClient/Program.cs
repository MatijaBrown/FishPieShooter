using System.Collections.Specialized;
using System.Numerics;
using FishPieClient.Core;
using FishPieClient.Graphics;
using FishPieClient.Graphics.Display;
using FishPieClient.Graphics.Materials;
using FishPieClient.Graphics.Mesh;
using FishPieClient.Graphics.UI;
using FishPieClient.Input;
using FishPieClient.Maths;
using FishPieClient.Utils;
using Serilog;
using Silk.NET.OpenGL;
using Key = FishPieClient.Input.Key;

namespace FishPieClient;

public static class Program
{

    private static Window? _window;
    private static bool _running = true;

    private static GL? _gl;

    private static MeshData Cube()
    {
        List<Vector3> positions =
        [
            new(-1.0f, -1.0f, 1.0f),     new(1.0f, -1.0f, 1.0f),      new(1.0f, 1.0f, 1.0f),
            new(-1.0f, 1.0f, 1.0f),      new(-1.0f, -1.0f, -1.0f),    new(1.0f, -1.0f, -1.0f),
            new(1.0f, 1.0f, -1.0f),      new(-1.0f, 1.0f, -1.0f),     new(-1.0f, -1.0f, -1.0f),
            new(-1.0f, -1.0f, 1.0f),     new(-1.0f, 1.0f, 1.0f),      new(-1.0f, 1.0f, -1.0f),
            new(1.0f, -1.0f, -1.0f),     new(1.0f, -1.0f, 1.0f),      new(1.0f, 1.0f, 1.0f),
            new(1.0f, 1.0f, -1.0f),      new(-1.0f, 1.0f, 1.0f),      new(1.0f, 1.0f, 1.0f),
            new(1.0f, 1.0f, -1.0f),      new(-1.0f, 1.0f, -1.0f),     new(-1.0f, -1.0f, 1.0f),
            new(-1.0f, -1.0f, -1.0f),    new(1.0f, -1.0f, -1.0f),     new(1.0f, -1.0f, 1.0f)
        ];

        List<uint> indices =
        [
            0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4, 8, 9, 10, 10, 11, 8,
            12, 13, 14, 14, 15, 12, 16, 17, 18, 18, 19, 16, 20, 21, 22,
            22, 23, 20
        ];
        
        return new MeshData(
            Vertices: positions.ConvertAll(pos => new VertexData(pos)),
            Indices: indices
        );
    }

    private static Vector3 WalkDirection(Dictionary<Key, bool> keyState, Camera camera)
    {
        var direction = Vector3.Zero;

        if (keyState[Key.W])
        {
            direction += camera.Direction;
        }

        if (keyState[Key.S])
        {
            direction -= camera.Direction;
        }

        if (keyState[Key.D])
        {
            direction += camera.Right;
        }

        if (keyState[Key.A])
        {
            direction -= camera.Right;
        }

        if (keyState[Key.Space])
        {
            direction += Vector3.UnitY;
        }

        if (keyState[Key.LShift])
        {
            direction -= Vector3.UnitY;
        }

        const float speed = 0.5f;
        return direction == Vector3.Zero ? Vector3.Zero : Vector3.Normalize(direction) * speed;
    }
    
    private static void Main(string[] args)
    {
        Logging.InitLogger();
        
        Log.Information("FPS Version: {Major}.{Minor}.{Build}", ProjectUtils.Major, ProjectUtils.Minor, ProjectUtils.Build);
        Log.Information("{OSVersion}", Environment.OSVersion.ToString());

        _window = new Window(WindowMode.Windowed, 1920, 1080, 1920, 0, mouseLocked: true);
        _window.OnClose += () =>
        {
            Log.Information("stopping");
            _running = false;
        };
        
        _gl = _window.Gl;
        
        var meshManager = new MeshManager(_gl);
        var materialManager = new MaterialManager(_gl);
        var renderer = new Renderer(_gl);
        
        var debugUi = new DebugUi(_window, _gl);
        bool debugMode = false;
        
        var scene = new Scene(
            meshManager: meshManager,
            materialManager: materialManager,
            camera: new Camera(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY,
                MathF.PI / 4.0f,
                _window.RenderWidth, _window.RenderHeight, 0.1f, 1000.0f)
        );
        
        var materialKeyRed = materialManager.Add(new MaterialData(
            new Colour(1.0f, 0.0f, 0.0f)
        ));
        var materialKeyBlue = materialManager.Add(new MaterialData(
            new Colour(0.0f, 0.0f, 1.0f)
        ));
        var materialKeyGreen = materialManager.Add(new MaterialData(
            new Colour(0.0f, 1.0f, 0.0f)
        ));
        
        materialManager.Remove(materialKeyBlue);
        
        scene.Entities.Add(new Entity()
        {
            Name = "cube1",
            MeshView = meshManager.Load(Cube()),
            Transform = new Transform(new Vector3(10.0f, 0.0f, -10.0f), 5.0f * Vector3.One, Quaternion.Identity),
            MaterialKey = materialKeyRed
        });
        scene.Entities.Add(new Entity()
        {
            Name = "cube2",
            MeshView = meshManager.Load(Cube()),
            Transform = new Transform(new Vector3(-10.0f, 0.0f, -10.0f), 5.0f * Vector3.One, Quaternion.Identity),
            MaterialKey = materialKeyGreen
        });

        var keyState = new Dictionary<Key, bool>()
        {
            { Key.W, false }, { Key.A, false }, { Key.S, false }, { Key.D, false }, { Key.Space, false }, { Key.LShift, false }
        };
        
        _window.OnKeyboard += (key, state) =>
        {
            if (key == Key.Esc)
            {
                Log.Information("stopping");
                _running = false;
            }
            if (key == Key.F1 && state == KeyState.Down)
            {
                debugMode = !debugMode;
                if (debugMode) debugUi.Activate();
                else debugUi.Deactivate();
                _window.SetMouseLocked(!debugMode);
            }
            else
            {
                keyState[key] = state == KeyState.Down;
            }
        };

        _window.OnMouseMove += (deltaX, deltaY) =>
        {
            const float sensitivity = 0.002f;

            if (!debugMode)
            {
                var dx = deltaX * sensitivity;
                var dy = deltaY * sensitivity;
                scene.Camera.AdjustYaw(dx);
                scene.Camera.AdjustPitch(-dy); 
            }
        };
        
        while (_running)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _window.PumpEvent();

            scene.Camera.Translate(WalkDirection(keyState, scene.Camera));
            
            renderer.Render(scene);

            if (debugMode)
            {
                debugUi.Render(scene);
            }
            
            _window.Swap();
        }
        
        scene.Dispose();
        debugUi.Dispose();
        renderer.Dispose();
        materialManager.Dispose();
        meshManager.Dispose();

        _window.Dispose();
    }
    
}