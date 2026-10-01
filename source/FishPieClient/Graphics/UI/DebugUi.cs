using System.Numerics;
using FishPieClient.Graphics.Display;
using FishPieClient.Graphics.UI.ImGuiImpl;
using FishPieClient.Maths;
using Hexa.NET.ImGui;
using ImGuizmoSharp;
using Serilog;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.UI;

public sealed class DebugUi : IDisposable
{

    private readonly ImGuiContextPtr _context;
    
    private readonly Window _window;

    public unsafe DebugUi(Window window, GL gl)
    {
        _window = window;

        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);

        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
        //io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;

        ImGui.StyleColorsDark();

        if (!ImGuiImplGlfw.Init(_window, true))
        {
            Log.Error("Failed to init ImGui Impl Glfw");
            return;
        }
        
        if (!ImGuiImplOpenGl.Init(gl))
        {
            Log.Error("Failed to init ImGui OpenGL3");
            return;
        }
    }

    public void Activate()
    {
        
    }

    public void Deactivate()
    {
        
    }

    public unsafe void Render(Scene scene)
    {
        var io = ImGui.GetIO();
        
        ImGuiImplOpenGl.NewFrame();
        ImGuiImplGlfw.NewFrame();
        ImGui.NewFrame();

        ImGuizmo.SetOrthographic(false);
        ImGuizmo.BeginFrame();
        ImGuizmo.Enable(true);
        ImGuizmo.SetRect(0, 0, io.DisplaySize.X, io.DisplaySize.Y);
        
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

                var transform = (Matrix4x4)entity.Transform;
                var cameraData = scene.Camera.Data;
                
                ImGuizmo.Manipulate(cameraData.View,
                    cameraData.Projection,
                    ImGuizmoOperation.Translate | ImGuizmoOperation.Scale | ImGuizmoOperation.Rotate,
                    ImGuizmoMode.World,
                    ref transform,
                    out _);

                entity.Transform.FromMatrix(transform);
            }
        }
        
        ImGui.Render();
        ImGuiImplOpenGl.RenderDrawData(ImGui.GetDrawData());
    }
    
    public void Dispose()
    {
        ImGuiImplOpenGl.Shutdown();
        ImGuiImplGlfw.Shutdown();
        
        ImGui.DestroyPlatformWindows();
        ImGui.DestroyContext(_context);
    }
    
}