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

    private static unsafe void OpenGLDebugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
    {
        if (type == GLEnum.DebugTypeError)
        {
            string msg = SilkMarshal.PtrToString(message) ?? "(unavailable)";
            Log.Fatal("OpenGL Error: {Source} {Type} {ID} {Severity} {Message}", source, type, id, severity, msg);
        }
    }

    private readonly Glfw _glfw;
    private readonly GlfwContext _glfwContext;
    
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

    public IGLContext? GLContext => _glfwContext;
    
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
        
        _glfw = Glfw.GetApi();

        if (!_glfw.Init())
        {
            Log.Fatal("Failed to init glfw");
            throw new Exception("Failed to init glfw");
        }

        _glfw.WindowHint(WindowHintBool.Resizable, false);
        _glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
        _glfw.WindowHint(WindowHintInt.RedBits, 32);
        _glfw.WindowHint(WindowHintInt.GreenBits, 32);
        _glfw.WindowHint(WindowHintInt.BlueBits, 32);
        _glfw.WindowHint(WindowHintInt.AlphaBits, 32);
        _glfw.WindowHint(WindowHintInt.DepthBits, 24);
        _glfw.WindowHint(WindowHintInt.StencilBits, 8);
        _glfw.WindowHint(WindowHintInt.Samples, 0);

        _glfw.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        _glfw.WindowHint(WindowHintInt.ContextVersionMinor, 6);
        _glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

        WindowHandle* handle = _glfw.CreateWindow((int)_width, (int)_height, "FPS Window", null, null);
        if (handle == null)
        {
            _glfw.Terminate();
            Log.Fatal("Failed to create window");
            throw new Exception("Failed to create window");
        }
        NativeHandle = (UIntPtr)handle;

        _glfw.ShowWindow(handle);

        if (_mouseLocked)
        {
            _glfw.SetInputMode(handle, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);
        }
        
        SetupEventCallbacks();
        
        _glfw.MakeContextCurrent(handle);
        
        Gl = CreateOpenGl(out _glfwContext);

        if (ProjectUtils.OpenGlDebugEnabled)
        {
            SetupDebug();
        }
        
        Gl.Enable(EnableCap.DepthTest);
        Gl.Enable(EnableCap.Blend);
        Gl.Enable(EnableCap.Multisample);
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
        
        _glfw.SetWindowCloseCallback(handle, _ => OnClose?.Invoke());
        _glfw.SetKeyCallback(handle, (_, key, keyCode, action, mods) =>
        {
            if (action == InputAction.Repeat) return;

            var state = action == InputAction.Press ? KeyState.Down : KeyState.Up;
            if (Enum.IsDefined(typeof(Key), (int)key))
            {
                OnKeyboard?.Invoke((Key)key, state);
            }
        });
        _glfw.SetMouseButtonCallback(handle, (_, button, action, mods) =>
        {
            if (button == MouseButton.Left)
            {
                _glfw.GetCursorPos(handle, out double x, out double y);
                OnMouseButton?.Invoke((float)x, (float)y, action == InputAction.Press ? MouseButtonState.Down : MouseButtonState.Up);
            }
        });
        _glfw.SetCursorPosCallback(handle, (_, x, y) =>
        {
            double deltaX = x - _lastMouseX;
            double deltaY = y - _lastMouseY;
            _lastMouseX = x;
            _lastMouseY = y;
            OnMouseMove?.Invoke((float)deltaX, (float)deltaY);
        });
        _glfw.GetCursorPos(handle, out _lastMouseX, out _lastMouseY);
    }
    
    private unsafe void SetupDebug()
    {
        Gl.Enable(EnableCap.DebugOutput);
        Gl.Enable(EnableCap.DebugOutputSynchronous);
        Gl.DebugMessageCallback(OpenGLDebugCallback, null);
    }

    private unsafe GL CreateOpenGl(out GlfwContext glfwContext)
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;

        glfwContext = new GlfwContext(_glfw, handle, this);
        return GL.GetApi(glfwContext);
    }

    private unsafe void GetMonitorInfo(out int left, out int top, out int right, out int bottom)
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;

        Monitor* monitor = _glfw.GetWindowMonitor(handle);
        if (monitor == null)
        {
            Log.Fatal("Failed to get monitor");
            throw new Exception("Failed to get monitor");
        }

        _glfw.GetMonitorWorkarea(monitor, out left, out top, out right, out bottom);
    }

    private unsafe void SetMode(WindowMode mode)
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        
        Monitor* monitor;
        VideoMode* videoMode;
        if (_mode == WindowMode.Fullscreen && mode == WindowMode.Windowed)
        {
            monitor = _glfw.GetWindowMonitor(handle);
            videoMode = _glfw.GetVideoMode(monitor);
            _glfw.SetWindowMonitor(handle, null, (int)(videoMode->Width + _width) / 2,
                (int)(videoMode->Height + _height) / 2,  (int)_width, (int)_height,videoMode->RefreshRate);
        }
        else if (_mode == WindowMode.Windowed && mode == WindowMode.Fullscreen)
        {
            monitor = _glfw.GetPrimaryMonitor();
            videoMode = _glfw.GetVideoMode(monitor);
            _glfw.SetWindowMonitor(handle, monitor, 0, 0, videoMode->Width, videoMode->Height,
                videoMode->RefreshRate);
        }
        _mode = mode;
    }

    public unsafe void Dispose()
    {
        Gl.Dispose();
        _glfwContext.Dispose();
        
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        
        _glfw.DestroyWindow(handle);
        _glfw.Dispose();
    }

    public void PumpEvent()
    {
        _glfw.PollEvents();
    }

    public unsafe void Swap()
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        
        _glfw.SwapBuffers(handle);
    }

    public unsafe void SetWindowTitle(string title)
    {
        WindowHandle* handle = (WindowHandle*)NativeHandle;
        _glfw.SetWindowTitle(handle, title);
    }

}