// See https://aka.ms/new-console-template for more information

using FishPieClient.Graphics;
using FishPieClient.Graphics.Display;
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
    
    private static void Main(string[] args)
    {
        Logging.InitLogger();
        
        Log.Information("FPS Version: {Major}.{Minor}.{Build}", ProjectUtils.Major, ProjectUtils.Minor, ProjectUtils.Build);
        Log.Information("{OSVersion}", Environment.OSVersion.ToString());

        _window = new Window(WindowMode.Windowed, 1920, 1080, 1920, 0);
        _window.OnClose += () => { _running = false; };
        
        _gl = _window.Gl;

        var sampleVert = new Shader("sample.vert", Shader.Type.Vertex, "sample_vertex_shader", _gl);
        var sampleFrag = new Shader("sample.frag", Shader.Type.Fragment, "sample_fragment_shader", _gl);
        var sampleProg = new ShaderProgram(sampleVert, sampleFrag, "sample_prog", _gl);
        sampleVert.Dispose();
        sampleFrag.Dispose();

        Span<VertexData> triangle =
        [
            new VertexData(0.0f, 0.5f, 0.0f),
            new VertexData(-0.5f, -0.5f, 0.0f),
            new VertexData(0.5f, -0.5f, 0.0f)
        ];

        var triangleBuffer = new Buffer<VertexData>((uint)triangle.Length, _gl);
        triangleBuffer.Write(triangle, 0);

        uint dummyVao = _gl.GenVertexArray();
        
        _gl.BindVertexArray(dummyVao);
        _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, triangleBuffer.Handle);
        sampleProg.Use();
        
        while (_running)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _window.PumpEvent();

            _gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
            
            _window.Swap();
        }

        _gl.BindVertexArray(0);
        _gl.DeleteVertexArray(dummyVao);
        
        triangleBuffer.Dispose();
        sampleProg.Dispose();

        _window.Dispose();
    }
    
}