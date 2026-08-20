// See https://aka.ms/new-console-template for more information

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

public struct IndirectCommand(uint count, uint instanceCount, uint first, uint baseInstance)
{
    public uint Count = count;
    public uint InstanceCount = instanceCount;
    public uint First = first;
    public uint BaseInstance = baseInstance;
}

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
            new(new Vector3(0.0f, 0.5f, 0.0f), Colour.Azure),
            new(new Vector3(-0.5f, -0.5f, 0.0f), new Colour(0.6f, 0.1f, 0.0f)),
            new(new Vector3(0.5f, -0.5f, 0.0f), new Colour(0.42f, 0.42f, 0.42f))
        ];

        var triangleBuffer = PersistentBuffer<VertexData>.CreateMultiBuffer((uint)triangle.Length, "triangle_buffer", _gl);
        triangleBuffer.Write(triangle, 0);

        var commandBuffer = new Buffer<IndirectCommand>(1, "command_buffer", _gl);
        var command = new IndirectCommand(
            count: 3,
            instanceCount: 1,
            first: 0,
            baseInstance: 0
        );
        Span<IndirectCommand> commandView = [command];
        commandBuffer.Write(commandView, 0);

        uint dummyVao = _gl.GenVertexArray();
        
        _gl.BindVertexArray(dummyVao);
        _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, triangleBuffer.Buffer.Handle);
        _gl.BindBuffer(BufferTargetARB.DrawIndirectBuffer, commandBuffer.Handle);
        sampleProg.Use();
        
        while (_running)
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _window.PumpEvent();

            triangle[0].Colour.R += 0.001f;
            if (triangle[0].Colour.R >= 1.0f)
            {
                triangle[0].Colour.R = 0.0f;
            }

            triangleBuffer.Write(triangle, 0);
            
            _gl.MultiDrawArraysIndirect(PrimitiveType.Triangles, (void*)0, 1, 0);
            triangleBuffer.Advance();
            
            _window.Swap();
        }

        _gl.BindVertexArray(0);
        _gl.DeleteVertexArray(dummyVao);

        commandBuffer.Dispose();
        triangleBuffer.Dispose();
        sampleProg.Dispose();

        _window.Dispose();
    }
    
}