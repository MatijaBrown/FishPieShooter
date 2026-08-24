using System.Numerics;
using FishPieClient.Graphics;
using FishPieClient.Graphics.Buffers;
using FishPieClient.Graphics.Display;
using FishPieClient.Graphics.Mesh;
using FishPieClient.Graphics.Shaders;
using FishPieClient.Utils;
using Serilog;
using Silk.NET.OpenGL;
using Shader = FishPieClient.Graphics.Shaders.Shader;

namespace FishPieClient;

public static class Program
{

    private static Window? _window;
    private static bool _running = true;

    private static GL? _gl;
    
    private static unsafe void Main(string[] args)
    {
        Logging.InitLogger();
        
        Log.Information("FPS Version: {Major}.{Minor}.{Build}", ProjectUtils.Major, ProjectUtils.Minor, ProjectUtils.Build);
        Log.Information("{OSVersion}", Environment.OSVersion.ToString());

        _window = new Window(WindowMode.Windowed, 1920, 1080, 1920, 0);
        _window.OnClose += () =>
        {
            Log.Information("stopping");
            _running = false;
        };
        
        _gl = _window.Gl;

        var sampleVert = new Shader("sample.vert", Shader.Type.Vertex, "sample_vertex_shader", _gl);
        var sampleFrag = new Shader("sample.frag", Shader.Type.Fragment, "sample_fragment_shader", _gl);
        var sampleProg = new ShaderProgram(sampleVert, sampleFrag, "sample_prog", _gl);
        sampleVert.Dispose();
        sampleFrag.Dispose();

        var meshManager = new MeshManager(_gl);
        var tri1 = meshManager.Load([
            new VertexData(0.0f, 0.0f, 0.0f, Colour.Azure),
            new VertexData(-0.5f, 0.0f, 0.0f, new Colour(0.6f, 0.1f, 0.0f)),
            new VertexData(-0.5f, 0.5f, 0.0f, new Colour(0.42f, 0.42f, 0.42f))
        ]);
        var tri2 = meshManager.Load([
            new VertexData(0.0f, 0.0f, 0.0f, Colour.Azure),
            new VertexData(-0.5f, 0.5f, 0.0f, new Colour(0.42f, 0.42f, 0.42f)),
            new VertexData(0.0f, 0.5f, 0.0f, new Colour(0.6f, 0.1f, 0.0f))
        ]);

        Span<IndirectCommand> commands =
        [
            new(
                count: tri1.Count,
                instanceCount: 1,
                first: tri1.Offset,
                baseInstance: 0
            ),
            new(
                count: tri2.Count,
                instanceCount: 1,
                first: tri2.Offset,
                baseInstance: 0
            )
        ];
        var commandBuffer = new Buffer<IndirectCommand>((uint)commands.Length, "command_buffer", _gl);
        commandBuffer.Write(commands, 0);

        uint dummyVao = _gl.GenVertexArray();

        _gl.BindVertexArray(dummyVao);
        _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, meshManager.Handle);
        _gl.BindBuffer(BufferTargetARB.DrawIndirectBuffer, commandBuffer.Handle);
        sampleProg.Use();
        
        while (_running)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _window.PumpEvent();

            _gl.MultiDrawArraysIndirect(PrimitiveType.Triangles, null, 2, 0);
            
            _window.Swap();
        }

        _gl.DeleteVertexArray(dummyVao);

        commandBuffer.Dispose();
        meshManager.Dispose();
        sampleProg.Dispose();

        _window.Dispose();
    }
    
}