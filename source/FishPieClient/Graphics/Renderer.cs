using FishPieClient.Graphics.Commands;
using FishPieClient.Graphics.Shaders;
using Silk.NET.OpenGL;
using Shader = FishPieClient.Graphics.Shaders.Shader;

namespace FishPieClient.Graphics;

public class Renderer : IDisposable
{

    private static ShaderProgram CreateProgram(GL gl)
    {
        var sampleVert = new Shader("sample.vert", Shader.Type.Vertex, "sample_vertex_shader", gl);
        var sampleFrag = new Shader("sample.frag", Shader.Type.Fragment, "sample_fragment_shader", gl);

        var prog = new ShaderProgram(sampleVert, sampleFrag, "sample_prog", gl);

        sampleVert.Dispose();
        sampleFrag.Dispose();

        return prog;
    }

    private readonly uint _dummyVao;
    private readonly CommandBuffer _commandBuffer;
    private readonly ShaderProgram _program;
    
    private readonly GL _gl;

    public Renderer(GL gl)
    {
        _gl = gl;

        _commandBuffer = new CommandBuffer(_gl);
        _program = CreateProgram(_gl);
        
        _dummyVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_dummyVao);

        _program.Use();
    }

    public unsafe void Render(Scene scene)
    {
        _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, scene.MeshManager.Handle);

        var commandCount = _commandBuffer.Build(scene);
        _gl.BindBuffer(BufferTargetARB.DrawIndirectBuffer, _commandBuffer.Handle);

        _gl.MultiDrawArraysIndirect(PrimitiveType.Triangles, null, commandCount, 0);

        _commandBuffer.Advance();
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_dummyVao);
        _program.Dispose();
        _commandBuffer.Dispose();
    }
}