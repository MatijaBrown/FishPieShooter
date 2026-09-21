using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FishPieClient.Graphics.Display;
using FishPieClient.Utils;
using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using UnmanagedType = Silk.NET.Core.Native.UnmanagedType;

namespace FishPieClient.Graphics.UI.ImGuiImpl;

public sealed unsafe class ImGuiImplGlfw : IDisposable
{

    private static readonly Dictionary<Keys, ImGuiKey> KeyMappings = new()
    {
        { Keys.Tab, ImGuiKey.Tab },
        { Keys.Left, ImGuiKey.LeftArrow },
        { Keys.Right, ImGuiKey.RightArrow },
        { Keys.Up, ImGuiKey.UpArrow },
        { Keys.Down, ImGuiKey.DownArrow },
        { Keys.PageUp, ImGuiKey.PageUp },
        { Keys.PageDown, ImGuiKey.PageDown },
        { Keys.Home, ImGuiKey.Home },
        { Keys.End, ImGuiKey.End },
        { Keys.Insert, ImGuiKey.Insert },
        { Keys.Delete, ImGuiKey.Delete },
        { Keys.Backspace, ImGuiKey.Backspace },
        { Keys.Space, ImGuiKey.Space },
        { Keys.Enter, ImGuiKey.Enter },
        { Keys.Escape, ImGuiKey.Escape },
        { Keys.Apostrophe, ImGuiKey.Apostrophe },
        { Keys.Comma, ImGuiKey.Comma },
        { Keys.Minus, ImGuiKey.Minus },
        { Keys.Period, ImGuiKey.Period },
        { Keys.Slash, ImGuiKey.Slash },
        { Keys.Semicolon, ImGuiKey.Semicolon },
        { Keys.Equal, ImGuiKey.Equal },
        { Keys.LeftBracket, ImGuiKey.LeftBracket },
        { Keys.BackSlash, ImGuiKey.Backslash },
        { Keys.RightBracket, ImGuiKey.RightBracket },
        { Keys.GraveAccent, ImGuiKey.GraveAccent },
        { Keys.CapsLock, ImGuiKey.CapsLock },
        { Keys.ScrollLock, ImGuiKey.ScrollLock },
        { Keys.NumLock, ImGuiKey.NumLock },
        { Keys.PrintScreen, ImGuiKey.PrintScreen },
        { Keys.Pause, ImGuiKey.Pause },
        { Keys.Keypad0, ImGuiKey.Keypad0 },
        { Keys.Keypad1, ImGuiKey.Keypad1 },
        { Keys.Keypad2, ImGuiKey.Keypad2 },
        { Keys.Keypad3, ImGuiKey.Keypad3 },
        { Keys.Keypad4, ImGuiKey.Keypad4 },
        { Keys.Keypad5, ImGuiKey.Keypad5 },
        { Keys.Keypad6, ImGuiKey.Keypad6 },
        { Keys.Keypad7, ImGuiKey.Keypad7 },
        { Keys.Keypad8, ImGuiKey.Keypad8 },
        { Keys.Keypad9, ImGuiKey.Keypad9 },
        { Keys.KeypadDecimal, ImGuiKey.KeypadDecimal },
        { Keys.KeypadDivide, ImGuiKey.KeypadDivide },
        { Keys.KeypadMultiply, ImGuiKey.KeypadMultiply },
        { Keys.KeypadSubtract,  ImGuiKey.KeypadSubtract },
        { Keys.KeypadAdd,ImGuiKey.KeypadAdd },
        { Keys.KeypadEnter, ImGuiKey.KeypadEnter },
        { Keys.KeypadEqual, ImGuiKey.KeypadEqual },
        { Keys.ShiftLeft, ImGuiKey.LeftShift },
        { Keys.ControlLeft, ImGuiKey.LeftCtrl },
        { Keys.AltLeft, ImGuiKey.LeftAlt },
        { Keys.SuperLeft, ImGuiKey.LeftSuper },
        { Keys.ShiftRight, ImGuiKey.RightShift },
        { Keys.ControlRight, ImGuiKey.RightCtrl },
        { Keys.AltRight, ImGuiKey.RightAlt },
        { Keys.SuperRight, ImGuiKey.RightSuper },
        { Keys.Menu, ImGuiKey.Menu },
        { Keys.Number0, ImGuiKey.Keypad0 },
        { Keys.Number1, ImGuiKey.Keypad1 },
        { Keys.Number2, ImGuiKey.Keypad2 },
        { Keys.Number3, ImGuiKey.Keypad3 },
        { Keys.Number4, ImGuiKey.Keypad4 },
        { Keys.Number5, ImGuiKey.Keypad5 },
        { Keys.Number6, ImGuiKey.Keypad6 },
        { Keys.Number7, ImGuiKey.Keypad7 },
        { Keys.Number8, ImGuiKey.Keypad8 },
        { Keys.Number9, ImGuiKey.Keypad9 },
        { Keys.A, ImGuiKey.A },
        { Keys.B, ImGuiKey.B },
        { Keys.C, ImGuiKey.C },
        { Keys.D, ImGuiKey.D },
        { Keys.E, ImGuiKey.E },
        { Keys.F, ImGuiKey.F },
        { Keys.G, ImGuiKey.G },
        { Keys.H, ImGuiKey.H },
        { Keys.I, ImGuiKey.I },
        { Keys.J, ImGuiKey.J },
        { Keys.K, ImGuiKey.K },
        { Keys.L, ImGuiKey.L },
        { Keys.M, ImGuiKey.M },
        { Keys.N, ImGuiKey.N },
        { Keys.O, ImGuiKey.O },
        { Keys.P, ImGuiKey.P },
        { Keys.Q, ImGuiKey.Q },
        { Keys.R, ImGuiKey.R },
        { Keys.S, ImGuiKey.S },
        { Keys.T, ImGuiKey.T },
        { Keys.U, ImGuiKey.U },
        { Keys.V, ImGuiKey.V },
        { Keys.W, ImGuiKey.W },
        { Keys.X, ImGuiKey.X },
        { Keys.Y, ImGuiKey.Y },
        { Keys.Z, ImGuiKey.Z },
        { Keys.F1, ImGuiKey.F1 },
        { Keys.F2, ImGuiKey.F2 },
        { Keys.F3, ImGuiKey.F3 },
        { Keys.F4, ImGuiKey.F4 },
        { Keys.F5, ImGuiKey.F5 },
        { Keys.F6, ImGuiKey.F6 },
        { Keys.F7, ImGuiKey.F7 },
        { Keys.F8, ImGuiKey.F8 },
        { Keys.F9, ImGuiKey.F9 },
        { Keys.F10, ImGuiKey.F10 },
        { Keys.F11, ImGuiKey.F11 },
        { Keys.F12, ImGuiKey.F12 },
        { Keys.F13, ImGuiKey.F13 },
        { Keys.F14, ImGuiKey.F14 },
        { Keys.F15, ImGuiKey.F15 },
        { Keys.F16, ImGuiKey.F16 },
        { Keys.F17, ImGuiKey.F17 },
        { Keys.F18, ImGuiKey.F18 },
        { Keys.F19, ImGuiKey.F19 },
        { Keys.F20, ImGuiKey.F20 },
        { Keys.F21, ImGuiKey.F21 },
        { Keys.F22, ImGuiKey.F22 },
        { Keys.F23, ImGuiKey.F23 },
        { Keys.F24, ImGuiKey.F24 },
    };
    
    private static bool IsWayland(Glfw glfw)
    {
        string version = glfw.GetVersionString();
        if (!version.Contains("Wayland"))
        {
            return false;
        }

        return true;
    }
    
    private readonly IntPtr[] _mouseCursors = new IntPtr[(int)ImGuiMouseCursor.COUNT];
    
    private readonly IntPtr _context;
    private readonly Window _window;
    
    private readonly Glfw _glfw;

    private double _time;

    private bool _mouseEntered;
    private Vector2 _lastValidMousePos;

    private bool _isWayland;
    private bool _installedCallbacks;

    private GlfwCallbacks.WindowFocusCallback? _prevUserWindowFocusCallback;
    private GlfwCallbacks.CursorPosCallback? _prevUserCursorPosCallback;
    private GlfwCallbacks.CursorEnterCallback? _prevUserCursorEnterCallback;
    private GlfwCallbacks.MouseButtonCallback? _prevMouseButtonCallback;
    private GlfwCallbacks.ScrollCallback? _prevScrollCallback;
    private GlfwCallbacks.KeyCallback? _prevKeyCallback;
    private GlfwCallbacks.CharCallback? _prevCharCallback;

    public ImGuiImplGlfw(Window window, IntPtr context, bool installCallbacks = false)
    {
        _window = window;
        _context = context;

        _glfw = _window.Glfw;

        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();

        _context = ImGui.GetCurrentContext();
        _time = 0.0;
        _isWayland = IsWayland(window.Glfw);

        io.BackendFlags = ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos;
        
        _mouseCursors[(int)ImGuiMouseCursor.Arrow] = (IntPtr)_glfw.CreateStandardCursor(CursorShape.Arrow);
        _mouseCursors[(int)ImGuiMouseCursor.TextInput] = (IntPtr)_glfw.CreateStandardCursor(CursorShape.IBeam);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeNS] = (IntPtr)_glfw.CreateStandardCursor(CursorShape.VResize);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeEW] = (IntPtr)_glfw.CreateStandardCursor(CursorShape.HResize);
        _mouseCursors[(int)ImGuiMouseCursor.Hand] = (IntPtr)_glfw.CreateStandardCursor(CursorShape.Hand);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeAll] = (IntPtr)_glfw.CreateStandardCursor(CursorShape.AllResize);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeNESW] = (IntPtr)_glfw.CreateStandardCursor(CursorShape.NeswResize);
        _mouseCursors[(int)ImGuiMouseCursor.ResizeNWSE] = (IntPtr)_glfw.CreateStandardCursor(CursorShape.NwseResize);
        _mouseCursors[(int)ImGuiMouseCursor.NotAllowed] = (IntPtr)_glfw.CreateStandardCursor(CursorShape.NotAllowed);

        if (installCallbacks)
        {
            InstallCallbacks();
        }
        
        var mainViewport = ImGui.GetMainViewport();
        mainViewport.PlatformHandle = (nint)_window.NativeHandle;
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    private void UpdateMouseData()
    {
        var io = ImGui.GetIO();
        
        var wptr = (WindowHandle*)_window.NativeHandle;
        bool isWindowFocused = _glfw.GetWindowAttrib(wptr, WindowAttributeGetter.Focused);

        if (isWindowFocused)
        {
            if (io.WantSetMousePos)
            {
                _glfw.SetCursorPos(wptr, io.MousePos.X, io.MousePos.Y);
            }

            if (_mouseEntered)
            {
                _glfw.GetCursorPos(wptr, out double mouseX, out double mouseY);
                _lastValidMousePos = new Vector2((float)mouseX, (float)mouseY);
                io.AddMousePosEvent((float)mouseX, (float)mouseY);
            }
        }
    }

    private void UpdateMouseCursor()
    {
        var io = ImGui.GetIO();

        var imguiCursor = ImGui.GetMouseCursor();
        
        var wptr = (WindowHandle*)_window.NativeHandle;
        if (imguiCursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
        {
            _glfw.SetInputMode(wptr, CursorStateAttribute.Cursor, CursorModeValue.CursorHidden);
        }
        else
        {
            _glfw.SetCursor(wptr,
                (Cursor*)(_mouseCursors[(int)imguiCursor] != 0
                    ? _mouseCursors[(int)imguiCursor]
                    : _mouseCursors[(int)ImGuiMouseCursor.Arrow]));
            _glfw.SetInputMode(wptr, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
        }
    }
    
    public void NewFrame()
    {
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }
        
        var io = ImGui.GetIO();
        var wptr = (WindowHandle*)_window.NativeHandle;

        _glfw.GetWindowSize(wptr, out int w, out int h);
        _glfw.GetFramebufferSize(wptr, out int displayW, out int displayH);
        io.DisplaySize = new Vector2(w, h);
        if (w > 0 && h > 0)
        {
            io.DisplayFramebufferScale = new Vector2((float)displayW / w, (float)displayH / h);
        }
        
        // setup time step
        double currentTime = _glfw.GetTime();
        if (currentTime <= _time)
        {
            currentTime = _time + 0.00001f;
        }

        io.DeltaTime = _time > 0.0 ? (float)(currentTime - _time) : (1.0f / 60.0f);
        _time = currentTime;

        UpdateMouseData();
        UpdateMouseCursor();
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    public void InstallCallbacks()
    {
        var wptr = (WindowHandle*)_window.NativeHandle;
            
        _prevUserWindowFocusCallback = _glfw.SetWindowFocusCallback(wptr, WindowFocusCallback);
        _prevUserCursorPosCallback = _glfw.SetCursorPosCallback(wptr, CursorPosCallback);
        _prevUserCursorEnterCallback = _glfw.SetCursorEnterCallback(wptr, CursorEnterCallback);
        _prevMouseButtonCallback = _glfw.SetMouseButtonCallback(wptr, MouseButtonCallback);
        _prevScrollCallback = _glfw.SetScrollCallback(wptr, ScrollCallback);
        _prevKeyCallback = _glfw.SetKeyCallback(wptr, KeyCallback);
        _prevCharCallback = _glfw.SetCharCallback(wptr, CharCallback);
        _installedCallbacks = true;
    }

    public void UninstallCallbacks()
    {
        var wptr = (WindowHandle*)_window.NativeHandle;

        _glfw.SetWindowFocusCallback(wptr, _prevUserWindowFocusCallback);
        _glfw.SetCursorEnterCallback(wptr, _prevUserCursorEnterCallback);
        _glfw.SetCursorPosCallback(wptr, _prevUserCursorPosCallback);
        _glfw.SetMouseButtonCallback(wptr, _prevMouseButtonCallback);
        _glfw.SetScrollCallback(wptr, _prevScrollCallback);
        _glfw.SetKeyCallback(wptr, _prevKeyCallback);
        _glfw.SetCharCallback(wptr, _prevCharCallback);
        _installedCallbacks = false;
        _prevUserWindowFocusCallback = null;
        _prevUserCursorEnterCallback = null;
        _prevUserCursorPosCallback = null;
        _prevMouseButtonCallback = null;
        _prevScrollCallback = null;
        _prevKeyCallback = null;
        _prevCharCallback = null;
    }

    public void Dispose()
    {
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }
        
        var io = ImGui.GetIO();

        if (_installedCallbacks)
        {
            UninstallCallbacks();
        }

        for (int i = 0; i < (int)ImGuiMouseCursor.COUNT; i++)
        {
            _glfw.DestroyCursor((Cursor*)_mouseCursors[i].ToPointer());
        }

        io.BackendFlags &= ~(ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos |
                             ImGuiBackendFlags.HasGamepad);

        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    private void WindowFocusCallback(WindowHandle* window, bool focused)
    {
        _prevUserWindowFocusCallback?.Invoke(window, focused);
        
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();
        io.AddFocusEvent(focused);
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    private void CursorPosCallback(WindowHandle* window, double x, double y)
    {
        _prevUserCursorPosCallback?.Invoke(window, x, y);
        
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();
        io.AddMousePosEvent((float)x, (float)y);
        _lastValidMousePos = new Vector2((float)x, (float)y);
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }
    
    private void CursorEnterCallback(WindowHandle* window, bool entered)
    {
        _prevUserCursorEnterCallback?.Invoke(window, entered);
        
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();
        if (_mouseEntered)
        {
            io.AddMousePosEvent(_lastValidMousePos.X, _lastValidMousePos.Y);
        }
        else if (!entered && _mouseEntered)
        {
            _lastValidMousePos = io.MousePos;
            io.AddMousePosEvent(float.MaxValue, float.MaxValue);
        }
        
        _mouseEntered = entered;
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }

    private void UpdateKeyModifiers(ImGuiIOPtr io, KeyModifiers mods)
    {
        io.AddKeyEvent(ImGuiKey.ModCtrl, mods.HasFlag(KeyModifiers.Control));
        io.AddKeyEvent(ImGuiKey.ModShift, mods.HasFlag(KeyModifiers.Shift));
        io.AddKeyEvent(ImGuiKey.ModAlt, mods.HasFlag(KeyModifiers.Alt));
        io.AddKeyEvent(ImGuiKey.ModSuper, mods.HasFlag(KeyModifiers.Super));
    }
    
    private void MouseButtonCallback(WindowHandle* window, MouseButton button, InputAction action, KeyModifiers mods)
    {
        _prevMouseButtonCallback?.Invoke(window, button, action, mods);
        
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();
        UpdateKeyModifiers(io, mods);
        if ((int)button >= 0 && (int)button < (int)ImGuiMouseButton.COUNT)
        {
            io.AddMouseButtonEvent((int)button, action == InputAction.Press);
        }
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }
    
    private void ScrollCallback(WindowHandle* window, double offsetX, double offsetY)
    {
        _prevScrollCallback?.Invoke(window, offsetX, offsetY);
        
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();
        io.AddMouseWheelEvent((float)offsetX, (float)offsetY);
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }
    
    private int TranslateUntranslatedKey(Keys key, int scancode)
    {
        int ikey = (int)key;
        
        if (ikey >= (int)Keys.Keypad0 && ikey <= (int)Keys.KeypadEqual)
        {
            return ikey;
        }

        string keyName = _glfw.GetKeyName(ikey, scancode);
        if (keyName != null && keyName[0] != 0)
        {
            Span<char> names = ['`', '-', '=', '[', ']', '\\', ',', ';', '\'', '.', '/'];
            Span<Keys> keys =
            [
                Keys.GraveAccent, Keys.Minus, Keys.Equal, Keys.LeftBracket, Keys.RightBracket, Keys.BackSlash,
                Keys.Comma, Keys.Semicolon, Keys.Apostrophe, Keys.Period, Keys.Slash
            ];
            Errors.Expect(names.Length == keys.Length, "key list good");
            if (keyName[0] >= '0' && keyName[0] <= '9')
            {
                key = Keys.Number0 + (keyName[0] - '0');
            }
            else if (keyName[0] >= 'A' && keyName[0] <= 'Z')
            {
                key = Keys.A + (keyName[0] - 'A');
            }
            else if (keyName[0] >= 'a' && keyName[0] <= 'z')
            {
                key = Keys.A + (keyName[0] - 'a');
            }
            else if (names.Contains(keyName[0]))
            {
                key = keys[names.IndexOf(keyName[0])];
            }
        }

        return (int)key;
    }
    
    private void KeyCallback(WindowHandle* window, Keys key, int scancode, InputAction action, KeyModifiers mods)
    {
        _prevKeyCallback?.Invoke(window, key, scancode, action, mods);
        
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();
        UpdateKeyModifiers(io, mods);

        int keycode = TranslateUntranslatedKey(key, scancode);
        
        if (!KeyMappings.TryGetValue(key, out var imguiKey))
        {
            imguiKey = ImGuiKey.None;
        }

        io.AddKeyEvent(imguiKey, action == InputAction.Press);
        io.SetKeyEventNativeData(imguiKey, keycode, scancode);
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }
    
    private void CharCallback(WindowHandle* window, uint codepoint)
    {
        _prevCharCallback?.Invoke(window, codepoint);
        
        var lastContext = ImGui.GetCurrentContext();
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(_context);
        }

        var io = ImGui.GetIO();
        io.AddInputCharacter(codepoint);
        
        if (lastContext != _context)
        {
            ImGui.SetCurrentContext(lastContext);
        }
    }
    
}