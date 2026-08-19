// See https://aka.ms/new-console-template for more information

using FishPieClient.Graphics.Display;
using FishPieClient.Utils;
using Serilog;
using Silk.NET.OpenGL;
using Shader = FishPieClient.Graphics.Shaders.Shader;

namespace FishPieClient;

public static class Program
{

    private static Window? _window;
    private static bool _running = true;

    private static GL _gl;
    
    private static void Main(string[] args)
    {
        Logging.InitLogger();
        
        Log.Information("FPS Version: {Major}.{Minor}.{Build}", ProjectUtils.Major, ProjectUtils.Minor, ProjectUtils.Build);
        Log.Information("{OSVersion}", Environment.OSVersion.ToString());

        _window = new Window(WindowMode.Windowed, 1920, 1080, 1920, 0);
        _window.OnClose += () => { _running = false; };
        
        _gl = _window.Gl;

        var shader = new Shader("simple.vert", Shader.Type.Vertex, "Simple Vertex Shader", _gl);
        shader.Dispose();
        
        while (_running)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _window.PumpEvent();

            _window.Swap();
        }

        _window.Dispose();
    }
    
}