using System.Numerics;
using FishPieClient.Graphics;
using FishPieClient.Graphics.Buffers;
using FishPieClient.Graphics.Commands;
using FishPieClient.Graphics.Display;
using FishPieClient.Graphics.Mesh;
using FishPieClient.Graphics.Shaders;
using FishPieClient.Utils;
using Serilog;
using Silk.NET.OpenGL;
using Shader = FishPieClient.Graphics.Shaders.Shader;

namespace FishPieClient;

public static class Program
{

    private static Window? _window;
    private static bool _running = true;

    private static GL? _gl;
    
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

        var scene = new Scene(meshManager);
        
        scene.Entities.Add(new Entity(meshManager.Load([
            new VertexData(0.0f, 0.0f, 0.0f, Colour.Azure),
            new VertexData(-0.5f, 0.0f, 0.0f, new Colour(0.6f, 0.1f, 0.0f)),
            new VertexData(-0.5f, 0.5f, 0.0f, new Colour(0.42f, 0.42f, 0.42f))
        ])));
        scene.Entities.Add(new Entity(meshManager.Load([
            new VertexData(0.0f, 0.0f, 0.0f, Colour.Azure),
            new VertexData(-0.5f, 0.5f, 0.0f, new Colour(0.42f, 0.42f, 0.42f)),
            new VertexData(0.0f, 0.5f, 0.0f, new Colour(0.6f, 0.1f, 0.0f))
        ])));
        
        while (_running)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _window.PumpEvent();

            renderer.Render(scene);
            
            _window.Swap();
        }
        
        scene.Dispose();
        renderer.Dispose();
        meshManager.Dispose();

        _window.Dispose();
    }
    
}