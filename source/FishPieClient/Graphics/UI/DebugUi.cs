using System.Numerics;
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
        var io = ImGui.GetIO();
        
        _openglImpl.NewFrame();
        _glfwImpl.NewFrame();
        ImGui.NewFrame();

        ImGui.LabelText($"FPS {io.Framerate:F1}", "");

        foreach (var entity in scene.Entities)
        {
            var material = scene.MaterialManager[entity.MaterialKey];
            
            if (ImGui.CollapsingHeader(entity.Name))
            {
                var colour = (Vector3)material.Colour;
                
                var label = $"{entity.Name} colour";
                
                if (ImGui.ColorPicker3(label, ref colour))
                {
                    scene.MaterialManager[entity.MaterialKey] = material with
                    {
                        Colour = (Colour)colour
                    };
                }
            }
        }
        
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