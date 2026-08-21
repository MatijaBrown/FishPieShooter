using System.Numerics;
using FishPieClient.Graphics;
using FishPieClient.Graphics.Buffers;
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
    
    private static unsafe void Main(string[] args)
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
            new(new Vector3(0.0f, 0.0f, 0.0f), Colour.Azure),
            new(new Vector3(-0.5f, 0.0f, 0.0f), new Colour(0.6f, 0.1f, 0.0f)),
            new(new Vector3(-0.5f, 0.5f, 0.0f), new Colour(0.42f, 0.42f, 0.42f)),
            
            new(new Vector3(0.0f, 0.0f, 0.0f), Colour.Azure),
            new(new Vector3(-0.5f, 0.5f, 0.0f), new Colour(0.42f, 0.42f, 0.42f)),
            new(new Vector3(0.0f, 0.5f, 0.0f), new Colour(0.6f, 0.1f, 0.0f))
        ];

        var triangleBuffer = PersistentBuffer<VertexData>.CreateMultiBuffer((uint)triangle.Length, "triangle_buffer", _gl);
        triangleBuffer.Write(triangle, 0);

        Span<IndirectCommand> commands =
        [
            new(
                count: 3,
                instanceCount: 1,
                first: 0,
                baseInstance: 0
            ),
            new(
                count: 3,
                instanceCount: 1,
                first: 3,
                baseInstance: 0
            )
        ];
        var commandBuffer = new Buffer<IndirectCommand>((uint)commands.Length, "command_buffer", _gl);
        commandBuffer.Write(commands, 0);

        sampleProg.Use();

        var renderer = new Renderer(triangleBuffer, _gl);
        
        while (_running)
        {
            
            _window.PumpEvent();

            renderer.Render(commandBuffer);
        
            triangle[0].Colour.R += 0.01f;
            if (triangle[0].Colour.R >= 1.0f)
            {
                triangle[0].Colour.R = 0.0f;
            }
            triangle[3].Colour = triangle[0].Colour;

            triangleBuffer.Write(triangle, 0);
            
            _window.Swap();
        }

        renderer.Dispose();
        commandBuffer.Dispose();
        sampleProg.Dispose();

        _window.Dispose();
    }
    
}