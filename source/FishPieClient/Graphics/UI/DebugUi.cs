using System.Numerics;
using FishPieClient.Graphics.Display;
using FishPieClient.Graphics.UI.ImGuiImpl;
using FishPieClient.Input;
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
    private readonly GL _gl;

    private Vector2? _click = null;
    
    public unsafe DebugUi(Window window, GL gl)
    {
        _window = window;
        _gl = gl;

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

        if (_click.HasValue)
        {
            var buffer = stackalloc byte[4];

            _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            _gl.ReadBuffer(ReadBufferMode.Back);
            _gl.ReadPixels(
                (int)_click.Value.X,
                (int)_click.Value.Y,
                1,
                1,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                buffer
            );
            Log.Debug("r: {R:X} g: {G:X} b: {B:X}", buffer[0], buffer[1], buffer[2]);
            _click = null;
        }
    }

    public void AddMouseEvent(float x, float y, MouseButtonState state)
    {
        var io = ImGui.GetIO();
        io.AddMouseButtonEvent(0, state == MouseButtonState.Down);
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