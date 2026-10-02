using System.Numerics;
using FishPieClient.Core;
using FishPieClient.Graphics.Display;
using FishPieClient.Graphics.Shaders;
using FishPieClient.Graphics.UI.ImGuiImpl;
using FishPieClient.Input;
using FishPieClient.Maths;
using FishPieClient.Utils;
using Hexa.NET.ImGui;
using ImGuizmoSharp;
using Serilog;
using Silk.NET.OpenGL;
using Shader = Silk.NET.OpenGL.Shader;

namespace FishPieClient.Graphics.UI;

public sealed class DebugUi : IDisposable
{

    private static Ray ScreenRay(float mouseX, float mouseY, Window window, Camera camera)
    {
        var x = 2.0f * mouseX / window.RenderWidth - 1.0f;
        var y = 1.0f - 2.0f * mouseY / window.RenderHeight;

        var rayClip = new Vector4(x, y, -1.0f, 1.0f);

        Matrix4x4.Invert(camera.Data.Projection, out var invProj);
        var rayEye = Vector4.Transform(rayClip, invProj);
        rayEye = rayEye with { Z = -1.0f, W = 0.0f };

        Matrix4x4.Invert(camera.Data.View, out var invView);
        var wsDir = Vector3.Normalize(Vector4.Transform(rayEye, invView).AsVector3());
        var wsOrigin = invView.Translation;

        return new Ray(wsOrigin, wsDir);
    }
    
    private readonly ImGuiContextPtr _context;
    
    private readonly Window _window;
    private readonly GL _gl;

    private Vector2? _click = null;
    private Entity? _selectedEntity;
    
    public DebugUi(Window window, GL gl)
    {
        _window = window;
        _gl = gl;
        _selectedEntity = null;

        _context = ImGui.CreateContext();
        ImGui.SetCurrentContext(_context);

        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
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

    public void Render(Scene scene)
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
            }

            if (entity == _selectedEntity)
            {
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
        
        ImGui.Begin("Log");
        
        ImGui.BeginChild("Log Output");
        Logging.ImGuiSink.DrawToImGui();
        ImGui.EndChild();
        
        ImGui.End();

        ImGui.Render();
        ImGuiImplOpenGl.RenderDrawData(ImGui.GetDrawData());
        
        if (_click.HasValue)
        {
            var pickRay = ScreenRay(_click.Value.X, _click.Value.Y, _window, scene.Camera);
            var intersection = scene.IntersectRay(pickRay);
            _selectedEntity = intersection?.Entity;

            _click = null;
        }
    }

    public void AddMouseEvent(float x, float y, MouseButtonState state)
    {
        var io = ImGui.GetIO();
        io.AddMouseButtonEvent(0, state == MouseButtonState.Down);

        if (io.WantCaptureMouse)
            return;
        
        if (state == MouseButtonState.Down)
            _click = new Vector2(x, y);
    }
    
    public void Dispose()
    {
        ImGuiImplOpenGl.Shutdown();
        ImGuiImplGlfw.Shutdown();
        
        ImGui.DestroyPlatformWindows();
        ImGui.DestroyContext(_context);
    }
    
}