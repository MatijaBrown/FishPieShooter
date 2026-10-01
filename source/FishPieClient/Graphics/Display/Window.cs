using System.Configuration;
using FishPieClient.Events;
using FishPieClient.Input;
using FishPieClient.Utils;
using Serilog;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using Monitor = Silk.NET.GLFW.Monitor;

namespace FishPieClient.Graphics.Display;

public sealed class Window : IGLContextSource, IDisposable
{

    private static void OpenGlDebugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
    {
        if (type == GLEnum.DebugTypeError)
        {
            string msg = SilkMarshal.PtrToString(message) ?? "(unavailable)";
            Log.Fatal("OpenGL Error: {Source} {Type} {ID} {Severity} {Message}", source, type, id, severity, msg);
        }
    }

    private readonly GlfwContext _glfwContext;
    private readonly IntPtr _invisibleCursor;
    
    private uint _width;
    private uint _height;
    private WindowMode _mode;

    private bool _mouseLocked;
    private double _lastMouseX, _lastMouseY;
    
    public UIntPtr NativeHandle { get; private set; }

    public uint RenderWidth => _width;

    public uint RenderHeight => _height;

    public GL Gl { get; }
    
    public event WindowCloseEvent? OnClose;
    public event KeyEvent? OnKeyboard;
    public event MouseButtonEvent? OnMouseButton;
    public event MouseEvent? OnMouseMove;
    public event WindowResizeEvent? OnResize;

    public IGLContext? GLContext => _glfwContext;

    public Glfw Glfw { get; }

    public uint WindowWidth
    {
        get
        {
            if (_mode == WindowMode.Windowed)
                return RenderWidth;

            GetMonitorInfo(out int left, out _, out int right, out _);
            return (uint)(right - left);
        }
    }

    public uint WindowHeight
    {
        get
        {
            if (_mode == WindowMode.Windowed)
                return RenderHeight;

            GetMonitorInfo(out int _, out int top, out _, out int bottom);
            return (uint)(bottom - top);
        }
    }

    public WindowMode Mode
    {
        get => _mode;
        set => SetMode(value);
    }
    
    public unsafe Window(WindowMode mode, uint width, uint height, uint x, uint y, bool mouseLocked = false)
    {
        _width = width;
        _height = height;
        _mode = mode;
        _mouseLocked = mouseLocked;
        
        Glfw = Glfw.GetApi();

        if (!Glfw.Init())
        {
            Log.Fatal("Failed to init glfw");
            throw new Exception("Failed to init glfw");
        }

        Glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
        Glfw.WindowHint(WindowHintInt.RedBits, 32);
        Glfw.WindowHint(WindowHintInt.GreenBits, 32);
        Glfw.WindowHint(WindowHintInt.BlueBits, 32);
        Glfw.WindowHint(WindowHintInt.AlphaBits, 32);
        Glfw.WindowHint(WindowHintInt.DepthBits, 24);
        Glfw.WindowHint(WindowHintInt.StencilBits, 8);
        Glfw.WindowHint(WindowHintInt.Samples, 0);

        Glfw.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        Glfw.WindowHint(WindowHintInt.ContextVersionMinor, 6);
        Glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

        WindowHandle* handle = Glfw.CreateWindow((int)_width, (int)_height, "FPS Window", null, null);
        if (handle == null)
        {
            Glfw.Terminate();
            Log.Fatal("Failed to create window");
            throw new Exception("Failed to create window");
        }
        NativeHandle = (UIntPtr)handle;

        Glfw.ShowWindow(handle);

        // Create invisible cursor
        byte* pixels = stackalloc byte[16 * 16 * 4];
        var img = new Image()
        {
            Width = 16,
            Height = 16,
            Pixels =pixels
        };
        var cursorPtr = Glfw.CreateCursor(&img, 0, 0);
        _invisibleCursor = new IntPtr(cursorPtr);
        SetMouseLocked(mouseLocked);
        
        SetupEventCallbacks();
        
        Glfw.MakeContextCurrent(handle);
        
        Gl = CreateOpenGl(out _glfwContext);

        if (ProjectUtils.OpenGlDebugEnabled)
        {
            SetupDebug();
        }
        
        Gl.Enable(EnableCap.DepthTest);
        Gl.Enable(EnableCap.Blend);
        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        SetMode(mode);

        string? vendor = SilkMarshal.PtrToString((nint)Gl.GetString(StringName.Vendor));
        string? renderer = SilkMarshal.PtrToString((nint)Gl.GetString(StringName.Renderer));
        string? version = SilkMarshal.PtrToString((nint)Gl.GetString(StringName.Version));
        
        Log.Information("Created new window {Width} {Height} {WindowMode} {Vendor} {Renderer} {Version}",
            _width,
            _height,
            _mode,
            vendor ?? "(unavailable)",
            renderer ?? "(unavailable)",
            version ?? "(unavailable)");
    }

    private unsafe void SetupEventCallbacks()
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        
        Glfw.SetWindowCloseCallback(handle, _ => OnClose?.Invoke());
        Glfw.SetWindowSizeCallback(handle, (_, width, height) =>
        {
            _width = (uint)width;
            _height = (uint)height;

            OnResize?.Invoke(_width, _height);
        });
        Glfw.SetKeyCallback(handle, (_, key, keyCode, action, mods) =>
        {
            if (action == InputAction.Repeat) return;

            var state = action == InputAction.Press ? KeyState.Down : KeyState.Up;
            if (Enum.IsDefined(typeof(Key), (int)key))
            {
                OnKeyboard?.Invoke((Key)key, state);
            }
        });
        Glfw.SetMouseButtonCallback(handle, (_, button, action, mods) =>
        {
            if (button == MouseButton.Left)
            {
                Glfw.GetCursorPos(handle, out double x, out double y);
                OnMouseButton?.Invoke((float)x, (float)y, action == InputAction.Press ? MouseButtonState.Down : MouseButtonState.Up);
            }
        });
        Glfw.SetCursorPosCallback(handle, (_, x, y) =>
        {
            double deltaX = x - _lastMouseX;
            double deltaY = y - _lastMouseY;
            _lastMouseX = x;
            _lastMouseY = y;
            OnMouseMove?.Invoke((float)deltaX, (float)deltaY);
        });
        Glfw.GetCursorPos(handle, out _lastMouseX, out _lastMouseY);
    }
    
    private unsafe void SetupDebug()
    {
        Gl.Enable(EnableCap.DebugOutput);
        Gl.Enable(EnableCap.DebugOutputSynchronous);
        Gl.DebugMessageCallback(OpenGlDebugCallback, null);
    }

    private unsafe GL CreateOpenGl(out GlfwContext glfwContext)
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;

        glfwContext = new GlfwContext(Glfw, handle, this);
        return GL.GetApi(glfwContext);
    }

    private unsafe void GetMonitorInfo(out int left, out int top, out int right, out int bottom)
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;

        Monitor* monitor = Glfw.GetWindowMonitor(handle);
        if (monitor == null)
        {
            Log.Fatal("Failed to get monitor");
            throw new Exception("Failed to get monitor");
        }

        Glfw.GetMonitorWorkarea(monitor, out left, out top, out right, out bottom);
    }

    private unsafe void SetMode(WindowMode mode)
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        
        Monitor* monitor;
        VideoMode* videoMode;
        if (_mode == WindowMode.Fullscreen && mode == WindowMode.Windowed)
        {
            monitor = Glfw.GetWindowMonitor(handle);
            videoMode = Glfw.GetVideoMode(monitor);
            Glfw.SetWindowMonitor(handle, null, (int)(videoMode->Width + _width) / 2,
                (int)(videoMode->Height + _height) / 2,  (int)_width, (int)_height,videoMode->RefreshRate);
        }
        else if (_mode == WindowMode.Windowed && mode == WindowMode.Fullscreen)
        {
            monitor = Glfw.GetPrimaryMonitor();
            videoMode = Glfw.GetVideoMode(monitor);
            Glfw.SetWindowMonitor(handle, monitor, 0, 0, videoMode->Width, videoMode->Height,
                videoMode->RefreshRate);
        }
        _mode = mode;
    }

    public unsafe void Dispose()
    {
        Gl.Dispose();
        _glfwContext.Dispose();
        
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        
        Glfw.DestroyWindow(handle);
        Glfw.Dispose();
    }

    public void PumpEvent()
    {
        Glfw.PollEvents();
    }

    public unsafe void Swap()
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        
        Glfw.SwapBuffers(handle);
    }

    public unsafe void SetWindowTitle(string title)
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        Glfw.SetWindowTitle(handle, title);
    }

    public unsafe void SetMouseLocked(bool mouseLocked)
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        _mouseLocked = mouseLocked;
        if (_mouseLocked)
        {
            Glfw.SetCursor(handle, (Cursor*)_invisibleCursor.ToPointer());
            Glfw.SetInputMode(handle, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);
        }
        else
        {
            Glfw.SetInputMode(handle, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
        }
    }

}