// See https://aka.ms/new-console-template for more information

using FishPieClient.Graphics.Display;
using FishPieClient.Utils;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Silk.NET.OpenGL;

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
        _window.OnKeyboard += (key, state) => Log.Debug("KeyEvent {Key} {State}", key, state);
        _window.OnMouseButton += (x, y, state) => Log.Debug("MouseButton {X} {Y} {State}", x, y, state);
        _window.OnMouseMove += (dx, dy) => Log.Debug("MouseMove {DeltaX} {DeltaY}", dx, dy);
        
        _gl = _window.Gl;
        
        while (_running)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _window.PumpEvent();

            _window.Swap();
        }

        _window.Dispose();
    }
    
}