using FishPieShooter.Events;
using FishPieShooter.Utils;
using Silk.NET.GLFW;

namespace FishPieClient.Graphics.Windowing;

public class Window : IDisposable
{

    public record Settings(string Title, uint Width, uint Height);
    
    private int _isDisposed = 0;
    
    private Glfw? _glfw;
    private UIntPtr _window;
    
    private bool _vsyncEnabled;
    
    public uint Width { get; private set; }
    
    public uint Height { get; private set; }
    
    public string Title { get; private set; }

    public unsafe Window(Settings settings, EventListener onEvent)
    {
        Title = settings.Title;
        Width = settings.Width;
        Height = settings.Height;
        
        Log.CoreInfo($"Creating window \"{Title}\" ({Width}, {Height})");

        _glfw = Glfw.GetApi();

        if (_glfw == null)
        {
            Log.CoreError("Failed to get GLFW");
            throw new Exception("Failed to get GLFW");
        }
        
        if (!_glfw.Init())
        {
            Log.CoreError("Failed to initialize GLFW");
            throw new Exception("Failed to initialize GLFW");
        }

        WindowHandle* handle =
            _glfw.CreateWindow((int)settings.Width, (int)settings.Height, settings.Title, null, null);
        if (handle == null)
        {
            Log.CoreError("Failed to create window");
            throw new Exception("Failed to create window");
        }
        _window = (UIntPtr)handle;

        _glfw.MakeContextCurrent(handle);
        SetVSync(true);
        
        // Set GLFW callbacks
        _glfw.SetWindowSizeCallback(handle, (_, width, height) =>
        {
            Width = (uint)width;
            Height = (uint)height;
            
            var resizeEvent = new WindowResizedEvent((uint)width, (uint)height);
            onEvent(resizeEvent);
        });
        _glfw.SetWindowCloseCallback(handle, (_) =>
        {
            var closeEvent = new WindowCloseEvent();
            onEvent(closeEvent);
        });
    }

    public unsafe void Shutdown()
    {
        if (_window == UIntPtr.Zero) return;
        
        _glfw!.DestroyWindow((WindowHandle*)_window);
        _window = UIntPtr.Zero;
    }
    
    public unsafe void OnUpdate()
    {
        _glfw!.PollEvents();
        _glfw.SwapBuffers((WindowHandle*)_window);
    }

    public void SetVSync(bool enabled)
    {
        _glfw!.SwapInterval(enabled ? 1 : 0);
        _vsyncEnabled = enabled;
    }

    public bool IsVsyncEnabled()
    {
        return _vsyncEnabled;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
        {
            return;
        }

        Shutdown();
        
        if (disposing)
        {
            _glfw!.Dispose();
            _glfw = null;
        }
    }
    
}