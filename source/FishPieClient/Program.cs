using System.Collections.Specialized;
using System.Numerics;
using FishPieClient.Core;
using FishPieClient.Graphics;
using FishPieClient.Graphics.Display;
using FishPieClient.Graphics.Mesh;
using FishPieClient.Input;
using FishPieClient.Utils;
using Serilog;
using Silk.NET.OpenGL;

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
            Vertices: positions.ConvertAll(pos => new VertexData(pos, Colour.Azure)),
            Indices: indices
        );
    }
    
    private static unsafe void Main(string[] args)
    {
        Logging.InitLogger();
        
        Log.Information("FPS Version: {Major}.{Minor}.{Build}", ProjectUtils.Major, ProjectUtils.Minor, ProjectUtils.Build);
        Log.Information("{OSVersion}", Environment.OSVersion.ToString());

        _window = new Window(WindowMode.Windowed, 1920, 1080, 1920, 0);
        _window.OnClose += () =>
        {
            Log.Information("stopping");
            _running = false;
        };
        
        _gl = _window.Gl;
        
        var meshManager = new MeshManager(_gl);
        var renderer = new Renderer(_gl);

        var scene = new Scene(
            meshManager: meshManager,
            camera: new Camera(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY,
                MathF.PI / 4.0f,
                _window.RenderWidth, _window.RenderHeight, 0.1f, 1000.0f)
        );
        
        scene.Entities.Add(new Entity(meshManager.Load(Cube())));

        _window.OnKeyboard += (key, keyState) =>
        {
            if (key == Key.Esc)
            {
                Log.Information("stopping");
                _running = false;
            }
        };
        
        while (_running)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _window.PumpEvent();

            scene.Camera.Translate(0.01f * Vector3.UnitZ);
            
            renderer.Render(scene);
            
            _window.Swap();
        }
        
        scene.Dispose();
        renderer.Dispose();
        meshManager.Dispose();

        _window.Dispose();
    }
    
}