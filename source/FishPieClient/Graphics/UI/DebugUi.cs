using FishPieClient.Graphics.Display;
using FishPieClient.Graphics.UI.ImGuiImpl;
using FishPieClient.Input;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.UI;

public sealed class DebugUi : IDisposable
{

    private readonly IntPtr _context;

    private readonly ImGuiImplGlfw _glfwImpl;
    private readonly ImGuiImplOpenGl _openglImpl;
    
    private readonly Window _window;
    
    public DebugUi(Window window, GL gl)
    {
        _window = window;
        
        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);
        
        var io = ImGui.GetIO();
        
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.MouseDrawCursor = io.WantCaptureMouse;
        
        ImGui.StyleColorsDark();

        _glfwImpl = new ImGuiImplGlfw(_window, _context);
        _openglImpl = new ImGuiImplOpenGl(_context, gl);
    }

    public void Activate()
    {
        _glfwImpl.InstallCallbacks();
    }

    public void Deactivate()
    {
        _glfwImpl.UninstallCallbacks();
    }

    public void Render(Scene scene)
    {
        _openglImpl.NewFrame();
        _glfwImpl.NewFrame();
        ImGui.NewFrame();

        ImGui.ShowDemoWindow();
        
        ImGui.Render();
        _openglImpl.RenderDrawData(ImGui.GetDrawData());
    }
    
    public void Dispose()
    {
        _openglImpl.Dispose();
        _glfwImpl.Dispose();
        ImGui.DestroyContext();
    }
    
}