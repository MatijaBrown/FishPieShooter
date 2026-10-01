using System.Numerics;
using System.Runtime.InteropServices;
using FishPieClient.Graphics.Display;
using FishPieClient.Utils;
using Hexa.NET.ImGui;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;

namespace FishPieClient.Graphics.UI.ImGuiImpl;

public static class ImGuiImplGlfw
{
    
    private unsafe delegate void SetClipboardTextFn(ImGuiContext* ctx, byte* text);
    private unsafe delegate byte* GetClipboardTextFn(ImGuiContext* _);
    
    private static readonly Dictionary<UIntPtr, ImGuiContextPtr> _contextMap = new();
    
    private sealed class ImGuiImplGlfwData
    {
        public ImGuiContextPtr Context;
        
        public UIntPtr Window;
        public Glfw Glfw;

        public double Time;
        
        public UIntPtr MouseWindow;

        public IntPtr[] MouseCursors = new IntPtr[(int)ImGuiMouseCursor.Count];
        public IntPtr LastMouseCursor;
        
        public Vector2 LastValidMousePos;
        public bool IsWayland;
        public bool InstalledCallbacks;
        public bool CallbacksChainForAllWindows;
        public string BackendPlatformName;

        public GlfwCallbacks.WindowFocusCallback? PrevUserCallbackWindowFocus = null;
        public GlfwCallbacks.CursorPosCallback? PrevUserCallbackCursorPos = null;
        public GlfwCallbacks.CursorEnterCallback? PrevUserCallbackCursorEnter = null;
        public GlfwCallbacks.MouseButtonCallback? PrevUserCallbackMouseButton = null;
        public GlfwCallbacks.ScrollCallback? PrevUserCallbackScroll = null;
        public GlfwCallbacks.KeyCallback? PrevUserCallbackKey = null;
        public GlfwCallbacks.CharCallback? PrevUserCallbackChar = null;
    }

    private static unsafe ImGuiImplGlfwData? GetBackendData()
    {
        if (ImGui.GetCurrentContext().Handle == null) return null;
        var handle = GCHandle.FromIntPtr((nint)ImGui.GetIO().BackendPlatformUserData);
        return (ImGuiImplGlfwData?)handle.Target;
    }

    private static unsafe ImGuiImplGlfwData? GetBackendData(UIntPtr window)
    {
        var ctx = _contextMap[window];
        var currentCtx = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(ctx);

        var handle = GCHandle.FromIntPtr((nint)ImGui.GetIO().BackendPlatformUserData);
        var res = (ImGuiImplGlfwData?)handle.Target;
        
        ImGui.SetCurrentContext(currentCtx);
        return res;
    }

    private static bool IsWayland(Glfw glfw)
    {
        var version = glfw.GetVersionString();
        return version.Contains("Wayland");
    }

    private static ImGuiKey KeyToImGuiKey(Keys key)
    {
        return key switch
        {
            Keys.Tab => ImGuiKey.Tab,
            Keys.Left => ImGuiKey.LeftArrow,
            Keys.Right => ImGuiKey.RightArrow,
            Keys.Up => ImGuiKey.UpArrow,
            Keys.Down => ImGuiKey.DownArrow,
            Keys.PageUp => ImGuiKey.PageUp,
            Keys.PageDown => ImGuiKey.PageDown,
            Keys.Home => ImGuiKey.Home,
            Keys.End => ImGuiKey.End,
            Keys.Insert => ImGuiKey.Insert,
            Keys.Delete => ImGuiKey.Delete,
            Keys.Backspace => ImGuiKey.Backspace,
            Keys.Space => ImGuiKey.Space,
            Keys.Enter => ImGuiKey.Enter,
            Keys.Escape => ImGuiKey.Escape,
            Keys.Apostrophe => ImGuiKey.Apostrophe,
            Keys.Comma => ImGuiKey.Comma,
            Keys.Minus => ImGuiKey.Minus,
            Keys.Period => ImGuiKey.Period,
            Keys.Slash => ImGuiKey.Slash,
            Keys.Semicolon => ImGuiKey.Semicolon,
            Keys.Equal => ImGuiKey.Equal,
            Keys.LeftBracket => ImGuiKey.LeftBracket,
            Keys.BackSlash => ImGuiKey.Backslash,
            Keys.RightBracket => ImGuiKey.RightBracket,
            Keys.GraveAccent => ImGuiKey.GraveAccent,
            Keys.CapsLock => ImGuiKey.CapsLock,
            Keys.ScrollLock => ImGuiKey.ScrollLock,
            Keys.NumLock => ImGuiKey.NumLock,
            Keys.PrintScreen => ImGuiKey.PrintScreen,
            Keys.Pause => ImGuiKey.Pause,
            Keys.Keypad0 => ImGuiKey.Keypad0,
            Keys.Keypad1 => ImGuiKey.Keypad1,
            Keys.Keypad2 => ImGuiKey.Keypad2,
            Keys.Keypad3 => ImGuiKey.Keypad3,
            Keys.Keypad4 => ImGuiKey.Keypad4,
            Keys.Keypad5 => ImGuiKey.Keypad5,
            Keys.Keypad6 => ImGuiKey.Keypad6,
            Keys.Keypad7 => ImGuiKey.Keypad7,
            Keys.Keypad8 => ImGuiKey.Keypad8,
            Keys.Keypad9 => ImGuiKey.Keypad9,
            Keys.KeypadDecimal => ImGuiKey.KeypadDecimal,
            Keys.KeypadDivide => ImGuiKey.KeypadDivide,
            Keys.KeypadMultiply => ImGuiKey.KeypadMultiply,
            Keys.KeypadSubtract => ImGuiKey.KeypadSubtract,
            Keys.KeypadAdd => ImGuiKey.KeypadAdd,
            Keys.KeypadEnter => ImGuiKey.KeypadEnter,
            Keys.KeypadEqual => ImGuiKey.KeypadEqual,
            Keys.ShiftLeft => ImGuiKey.LeftShift,
            Keys.ControlLeft => ImGuiKey.LeftCtrl,
            Keys.AltLeft => ImGuiKey.LeftAlt,
            Keys.SuperLeft => ImGuiKey.LeftSuper,
            Keys.ShiftRight => ImGuiKey.RightShift,
            Keys.ControlRight => ImGuiKey.RightCtrl,
            Keys.AltRight => ImGuiKey.RightAlt,
            Keys.SuperRight => ImGuiKey.RightSuper,
            Keys.Menu => ImGuiKey.Menu,
            Keys.Number0 => ImGuiKey.Key0,
            Keys.Number1 => ImGuiKey.Key1,
            Keys.Number2 => ImGuiKey.Key2,
            Keys.Number3 => ImGuiKey.Key3,
            Keys.Number4 => ImGuiKey.Key4,
            Keys.Number5 => ImGuiKey.Key5,
            Keys.Number6 => ImGuiKey.Key6,
            Keys.Number7 => ImGuiKey.Key7,
            Keys.Number8 => ImGuiKey.Key8,
            Keys.Number9 => ImGuiKey.Key9,
            Keys.A => ImGuiKey.A,
            Keys.B => ImGuiKey.B,
            Keys.C => ImGuiKey.C,
            Keys.D => ImGuiKey.D,
            Keys.E => ImGuiKey.E,
            Keys.F => ImGuiKey.F,
            Keys.G => ImGuiKey.G,
            Keys.H => ImGuiKey.H,
            Keys.I => ImGuiKey.I,
            Keys.J => ImGuiKey.J,
            Keys.K => ImGuiKey.K,
            Keys.L => ImGuiKey.L,
            Keys.M => ImGuiKey.M,
            Keys.N => ImGuiKey.N,
            Keys.O => ImGuiKey.O,
            Keys.P => ImGuiKey.P,
            Keys.Q => ImGuiKey.Q,
            Keys.R => ImGuiKey.R,
            Keys.S => ImGuiKey.S,
            Keys.T => ImGuiKey.T,
            Keys.U => ImGuiKey.U,
            Keys.V => ImGuiKey.V,
            Keys.W => ImGuiKey.W,
            Keys.X => ImGuiKey.X,
            Keys.Y => ImGuiKey.Y,
            Keys.Z => ImGuiKey.Z,
            Keys.F1 => ImGuiKey.F1,
            Keys.F2 => ImGuiKey.F2,
            Keys.F3 => ImGuiKey.F3,
            Keys.F4 => ImGuiKey.F4,
            Keys.F5 => ImGuiKey.F5,
            Keys.F6 => ImGuiKey.F6,
            Keys.F7 => ImGuiKey.F7,
            Keys.F8 => ImGuiKey.F8,
            Keys.F9 => ImGuiKey.F9,
            Keys.F10 => ImGuiKey.F10,
            Keys.F11 => ImGuiKey.F11,
            Keys.F12 => ImGuiKey.F12,
            Keys.F13 => ImGuiKey.F13,
            Keys.F14 => ImGuiKey.F14,
            Keys.F15 => ImGuiKey.F15,
            Keys.F16 => ImGuiKey.F16,
            Keys.F17 => ImGuiKey.F17,
            Keys.F18 => ImGuiKey.F18,
            Keys.F19 => ImGuiKey.F19,
            Keys.F20 => ImGuiKey.F20,
            Keys.F21 => ImGuiKey.F21,
            Keys.F22 => ImGuiKey.F22,
            Keys.F23 => ImGuiKey.F23,
            Keys.F24 => ImGuiKey.F24,
            _ => ImGuiKey.None
        };
    }
    
    private static unsafe void UpdateKeyModifiers(ImGuiIOPtr io, UIntPtr window, Glfw glfw)
    {
        var handle = (WindowHandle*)window;
        io.AddKeyEvent(ImGuiKey.ModCtrl,
            (glfw.GetKey(handle, Keys.ControlLeft) == (int)InputAction.Press) ||
            (glfw.GetKey(handle, Keys.ControlRight) == (int)InputAction.Press));
        io.AddKeyEvent(ImGuiKey.ModShift,
            (glfw.GetKey(handle, Keys.ShiftLeft) == (int)InputAction.Press) ||
            (glfw.GetKey(handle, Keys.ShiftRight) == (int)InputAction.Press));
        io.AddKeyEvent(ImGuiKey.ModAlt,
            (glfw.GetKey(handle, Keys.AltLeft) == (int)InputAction.Press) ||
            (glfw.GetKey(handle, Keys.AltRight) == (int)InputAction.Press));
        io.AddKeyEvent(ImGuiKey.ModSuper,
            (glfw.GetKey(handle, Keys.SuperLeft) == (int)InputAction.Press) ||
            (glfw.GetKey(handle, Keys.SuperRight) == (int)InputAction.Press));
    }

    private static bool ShouldChainCallback(ImGuiImplGlfwData bd, UIntPtr window)
    {
        return window == bd.Window;
    }
    
    private static unsafe void MouseButtonCallback(WindowHandle* ptr, MouseButton button, InputAction action, KeyModifiers mods)
    {
        var window = (nuint)ptr;
        
        var bd = GetBackendData(window);
        if (bd == null) return;
        if (bd.PrevUserCallbackMouseButton != null && ShouldChainCallback(bd, window))
            bd.PrevUserCallbackMouseButton(ptr, button, action, mods);

        var prevCtx = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(bd.Context);
        
        var io = ImGui.GetIO();
        UpdateKeyModifiers(io, window, bd.Glfw);
        if (button >= 0 && (int)button < (int)ImGuiMouseButton.Count)
            io.AddMouseButtonEvent((int)button, action == InputAction.Press);

        ImGui.SetCurrentContext(prevCtx);
    }
    
    private static unsafe void ScrollCallback(WindowHandle* ptr, double offsetX, double offsetY)
    {
        var window = (nuint)ptr;
        
        var bd = GetBackendData(window);
        if (bd == null) return;
        if (bd.PrevUserCallbackScroll != null && ShouldChainCallback(bd, window))
            bd.PrevUserCallbackScroll(ptr, offsetX, offsetY);
        
        var prevCtx = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(bd.Context);
        
        var io = ImGui.GetIO();
        io.AddMouseWheelEvent((float)offsetX, (float)offsetY);
        
        ImGui.SetCurrentContext(prevCtx);
    }

    private static Keys TranslateUntranslatedKey(Keys key, int scancode, Glfw glfw)
    {
        if (key is >= Keys.Keypad0 and <= Keys.KeypadEqual)
            return key;

        string keyName = glfw.GetKeyName((int)key, scancode);
        if (keyName is { Length: > 0 } && keyName[0] != 0)
        {
            const string charNames = "`-=[]\\,;'./";
            Span<Keys> charKeys =
            [
                Keys.GraveAccent, Keys.Minus, Keys.Equal, Keys.LeftBracket, Keys.RightBracket, Keys.BackSlash,
                Keys.Comma, Keys.Semicolon, Keys.Apostrophe, Keys.Period, Keys.Slash
            ];
            Errors.Expect(charNames.Length == charKeys.Length, "Translation arrays faulty!");
            if (keyName[0] >= '0' && keyName[0] <= '9') key = Keys.Number0 + (keyName[0] - '0');
            else if (keyName[0] >= 'A' && keyName[0] <= 'Z') key = Keys.A + (keyName[0] - 'A');
            else if (keyName[0] >= 'a' && keyName[0] <= 'z') key = Keys.A + (keyName[0] - 'a');
            else
            {
                int idx = charNames.IndexOf(keyName, StringComparison.Ordinal);
                if (idx != -1) key = charKeys[idx];
            }
        }

        return key;
    }
    
    private static unsafe void KeyCallback(WindowHandle* ptr, Keys key, int scancode, InputAction action, KeyModifiers mods)
    {
        var window = (nuint)ptr;
        
        var bd = GetBackendData(window);
        if (bd == null) return;
        if (bd.PrevUserCallbackKey != null && ShouldChainCallback(bd, window))
            bd.PrevUserCallbackKey(ptr, key, scancode, action, mods);
        
        var prevCtx = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(bd.Context);
        
        var io = ImGui.GetIO();
        UpdateKeyModifiers(io, window, bd.Glfw);

        key = TranslateUntranslatedKey(key, scancode, bd.Glfw);

        var imguiKey = KeyToImGuiKey(key);
        io.AddKeyEvent(imguiKey, action == InputAction.Press);
        io.SetKeyEventNativeData(imguiKey, (int)key, scancode);
        
        ImGui.SetCurrentContext(prevCtx);
    }
    
    private static unsafe void WindowFocusCallback(WindowHandle* ptr, bool focused)
    {
        var window = (nuint)ptr;
        
        var bd = GetBackendData(window);
        if (bd == null) return;
        if (bd.PrevUserCallbackWindowFocus != null && ShouldChainCallback(bd, window))
            bd.PrevUserCallbackWindowFocus(ptr, focused);
        
        var prevCtx = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(bd.Context);
        
        var io = ImGui.GetIO();
        io.AddFocusEvent(focused);
        
        ImGui.SetCurrentContext(prevCtx);
    }
    
    private static unsafe void CursorPosCallback(WindowHandle* ptr, double x, double y)
    {
        var window = (nuint)ptr;
        
        var bd = GetBackendData(window);
        if (bd == null) return;
        if (bd.PrevUserCallbackCursorPos != null && ShouldChainCallback(bd, window))
            bd.PrevUserCallbackCursorPos(ptr, x, y);
        
        var prevCtx = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(bd.Context);
        
        var io = ImGui.GetIO();
        io.AddMousePosEvent((float)x, (float)y);
        bd.LastValidMousePos = new Vector2((float)x, (float)y);
        
        ImGui.SetCurrentContext(prevCtx);
    }
    
    private static unsafe void CursorEnterCallback(WindowHandle* ptr, bool entered)
    {
        var window = (nuint)ptr;
        
        var bd = GetBackendData(window);
        if (bd == null) return;
        if (bd.PrevUserCallbackCursorEnter != null && ShouldChainCallback(bd, window))
            bd.PrevUserCallbackCursorEnter(ptr, entered);
        
        var prevCtx = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(bd.Context);
        
        var io = ImGui.GetIO();
        if (entered)
        {
            bd.MouseWindow = window;
            io.AddMousePosEvent(bd.LastValidMousePos.X, bd.LastValidMousePos.Y);
        }
        else if (!entered && bd.MouseWindow == window)
        {
            bd.LastValidMousePos = io.MousePos;
            bd.MouseWindow = UIntPtr.Zero;
            io.AddMousePosEvent(-float.MaxValue, -float.MaxValue);
        }
        
        ImGui.SetCurrentContext(prevCtx);
    }
    
    private static unsafe void CharCallback(WindowHandle* ptr, uint c)
    {
        var window = (nuint)ptr;
        
        var bd = GetBackendData(window);
        if (bd == null) return;
        if (bd.PrevUserCallbackChar != null && ShouldChainCallback(bd, window))
            bd.PrevUserCallbackChar(ptr, c);
        
        var prevCtx = ImGui.GetCurrentContext();
        ImGui.SetCurrentContext(bd.Context);
        
        var io = ImGui.GetIO();
        io.AddInputCharacter(c);
        
        ImGui.SetCurrentContext(prevCtx);
    }

    public static unsafe void InstallCallbacks(UIntPtr window)
    {
        var bd = GetBackendData(window);
        if (bd == null) return;
        Errors.Ensure(!bd.InstalledCallbacks, "Callbacks already installed!");
        Errors.Ensure(bd.Window == window, "Wrong window!");

        var glfw = bd.Glfw;
        var handle =(WindowHandle*)window;

        bd.PrevUserCallbackWindowFocus = glfw.SetWindowFocusCallback(handle, WindowFocusCallback);
        bd.PrevUserCallbackCursorEnter = glfw.SetCursorEnterCallback(handle, CursorEnterCallback);
        bd.PrevUserCallbackCursorPos = glfw.SetCursorPosCallback(handle, CursorPosCallback);
        bd.PrevUserCallbackMouseButton = glfw.SetMouseButtonCallback(handle, MouseButtonCallback);
        bd.PrevUserCallbackScroll = glfw.SetScrollCallback(handle, ScrollCallback);
        bd.PrevUserCallbackKey = glfw.SetKeyCallback(handle, KeyCallback);
        bd.PrevUserCallbackChar = glfw.SetCharCallback(handle, CharCallback);
        bd.InstalledCallbacks = true;
    }

    public static unsafe void RestoreCallbacks(UIntPtr window)
    {
        var bd = GetBackendData(window);
        if (bd == null) return;
        Errors.Ensure(bd.InstalledCallbacks, "Callbacks not installed!");
        Errors.Ensure(bd.Window == window, "Wrong window!");
        
        var glfw = bd.Glfw;
        var handle = (WindowHandle*)window;

        glfw.SetWindowFocusCallback(handle, bd.PrevUserCallbackWindowFocus);
        glfw.SetCursorEnterCallback(handle, bd.PrevUserCallbackCursorEnter);
        glfw.SetCursorPosCallback(handle, bd.PrevUserCallbackCursorPos);
        glfw.SetMouseButtonCallback(handle, bd.PrevUserCallbackMouseButton);
        glfw.SetScrollCallback(handle, bd.PrevUserCallbackScroll);
        glfw.SetKeyCallback(handle, bd.PrevUserCallbackKey);
        glfw.SetCharCallback(handle, bd.PrevUserCallbackChar);
        bd.InstalledCallbacks = false;
        bd.PrevUserCallbackWindowFocus = null;
        bd.PrevUserCallbackCursorEnter = null;
        bd.PrevUserCallbackCursorPos = null;
        bd.PrevUserCallbackMouseButton = null;
        bd.PrevUserCallbackScroll = null;
        bd.PrevUserCallbackKey = null;
        bd.PrevUserCallbackChar = null;
    }
    
    public static unsafe bool Init(Window window, bool installCallbacks)
    {
        var io = ImGui.GetIO();
        Errors.Ensure(io.BackendPlatformUserData == null, "Already initialized a platform backend!");

        var glfw = window.Glfw;
        var bd = new ImGuiImplGlfwData()
        {
            Context = ImGui.GetCurrentContext(),
            Glfw = glfw,
            Window = window.NativeHandle,
            Time = 0.0,
            IsWayland = IsWayland(window.Glfw)
        };
        _contextMap.Add(bd.Window, bd.Context);

        bd.BackendPlatformName = $"imgui_impl_glfw ({glfw.GetVersionString()})";
        bd.BackendPlatformName += bd.IsWayland ? " (Wayland)" : "";
        io.BackendPlatformUserData = (void*)GCHandle.ToIntPtr(GCHandle.Alloc(bd));
        io.BackendPlatformName = (byte*)SilkMarshal.StringToPtr(bd.BackendPlatformName);
        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;

        var platformIo = ImGui.GetPlatformIO();

        platformIo.PlatformSetClipboardTextFn = (void*)GCHandle.ToIntPtr(GCHandle.Alloc((SetClipboardTextFn)SetClipboardText));
        platformIo.PlatformGetClipboardTextFn = (void*)GCHandle.ToIntPtr(GCHandle.Alloc((GetClipboardTextFn)GetClipboardText));

        bd.MouseCursors[(int)ImGuiMouseCursor.Arrow] = (nint)glfw.CreateStandardCursor(CursorShape.Arrow);
        bd.MouseCursors[(int)ImGuiMouseCursor.TextInput] = (nint)glfw.CreateStandardCursor(CursorShape.IBeam);
        bd.MouseCursors[(int)ImGuiMouseCursor.ResizeNs] = (nint)glfw.CreateStandardCursor(CursorShape.VResize);
        bd.MouseCursors[(int)ImGuiMouseCursor.ResizeEw] = (nint)glfw.CreateStandardCursor(CursorShape.HResize);
        bd.MouseCursors[(int)ImGuiMouseCursor.Hand] = (nint)glfw.CreateStandardCursor(CursorShape.Hand);
        
        bd.MouseCursors[(int)ImGuiMouseCursor.ResizeAll] = (nint)glfw.CreateStandardCursor(CursorShape.AllResize);
        bd.MouseCursors[(int)ImGuiMouseCursor.ResizeNesw] = (nint)glfw.CreateStandardCursor(CursorShape.NeswResize);
        bd.MouseCursors[(int)ImGuiMouseCursor.ResizeNwse] = (nint)glfw.CreateStandardCursor(CursorShape.NwseResize);
        bd.MouseCursors[(int)ImGuiMouseCursor.NotAllowed] = (nint)glfw.CreateStandardCursor(CursorShape.NotAllowed);

        if (installCallbacks)
        {
            InstallCallbacks(window.NativeHandle);
        }

        var mainViewport = ImGui.GetMainViewport();
        mainViewport.PlatformHandle = (void*)bd.Window;

        return true;
        
        void SetClipboardText(ImGuiContext* _, byte* text)
        {
            var d = GetBackendData()!; // isn't null
            d.Glfw.SetClipboardString((WindowHandle*)d.Window, SilkMarshal.PtrToString((nint)text));
        }
        byte* GetClipboardText(ImGuiContext* _)
        {
            var d = GetBackendData()!;
            return (byte*)SilkMarshal.StringToPtr(d.Glfw.GetClipboardString((WindowHandle*)d.Window));
        }
    }

    public static unsafe void Shutdown()
    {
        var bd = GetBackendData();
        Errors.Ensure(bd != null, "No platform backend to shutdown, or already shutdown?");

        var glfw = bd!.Glfw;
        
        var io = ImGui.GetIO();

        if (bd.InstalledCallbacks)
        {
            RestoreCallbacks(bd.Window);
        }

        for (ImGuiMouseCursor cursorN = 0; cursorN < ImGuiMouseCursor.Count; cursorN++)
        {
            glfw.DestroyCursor((Cursor*)bd.MouseCursors[(int)cursorN]);
        }

        _contextMap.Remove(bd.Window);
        
        var handle = GCHandle.FromIntPtr((nint)io.BackendPlatformUserData);
        handle.Free();
        io.BackendPlatformUserData = null;
        io.BackendPlatformName = null;

        io.BackendFlags &= ~(ImGuiBackendFlags.HasMouseCursors | ImGuiBackendFlags.HasSetMousePos |
                             ImGuiBackendFlags.HasGamepad);
    }

    private static unsafe void UpdateMouseData()
    {
        var bd = GetBackendData()!;
        var io = ImGui.GetIO();
        
        var window = (WindowHandle*)bd.Window;
        var glfw = bd.Glfw;
        bool isWindowFocused = glfw.GetWindowAttrib(window, WindowAttributeGetter.Focused);

        if (isWindowFocused)
        {
            if (io.WantSetMousePos)
                glfw.SetCursorPos(window, io.MousePos.X, io.MousePos.Y);

            if (bd.MouseWindow == UIntPtr.Zero)
            {
                glfw.GetCursorPos(window, out double mouseX, out double mouseY);
                bd.LastValidMousePos = new Vector2((float)mouseX, (float)mouseY);
                io.AddMousePosEvent((float)mouseX, (float)mouseY);
            }
        }
    }

    private static unsafe void UpdateMouseCursor()
    {
        var bd = GetBackendData();
        var io = ImGui.GetIO();

        var glfw = bd!.Glfw;
        var window = (WindowHandle*)bd.Window;

        if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.NoMouseCursorChange) ||
            glfw.GetInputMode(window, CursorStateAttribute.Cursor) == (int)CursorModeValue.CursorDisabled)
        {
            return;
        }

        var imguiCursor = ImGui.GetMouseCursor();
        if (imguiCursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
        {
            glfw.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorHidden);
        }
        else
        {
            glfw.SetCursor(window,
                bd.MouseCursors[(int)imguiCursor] != IntPtr.Zero
                    ? (Cursor*)bd.MouseCursors[(int)imguiCursor]
                    : (Cursor*)bd.MouseCursors[(int)ImGuiMouseCursor.Arrow]);
            glfw.SetInputMode(window, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
        }
    }

    public static unsafe void NewFrame()
    {
        var io = ImGui.GetIO();
        var bd = GetBackendData();
        Errors.Ensure(bd != null, "Context or backend not initialized! DDid you call ImGuiImplGlfw.Init()?");

        var glfw = bd!.Glfw;
        var window = (WindowHandle*)bd.Window;

        glfw.GetWindowSize(window, out var w, out var h);
        glfw.GetFramebufferSize(window, out var displayW, out var displayH);
        io.DisplaySize = new Vector2(w, h);
        if (w > 0 && h > 0)
            io.DisplayFramebufferScale = new Vector2((float)displayW / w, (float)displayH / h);

        double currentTime = glfw.GetTime();
        if (currentTime <= bd.Time)
            currentTime = bd.Time + 0.00001f;
        io.DeltaTime = bd.Time > 0.0 ? (float)(currentTime - bd.Time) : (1.0f / 6.0f);
        bd.Time = currentTime;
        
        UpdateMouseData();
        UpdateMouseCursor();
        
        // TODO: Gamepad
    }
    
}