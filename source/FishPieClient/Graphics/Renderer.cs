using FishPieClient.Core;
using FishPieClient.Graphics.Buffers;
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

    private readonly CommandBuffer _commandBuffer;
    private readonly MultiBuffer<PersistentBuffer<CameraData>, CameraData> _cameraBuffer;
    
    private readonly uint _dummyVao;
    private readonly ShaderProgram _program;
    
    private readonly GL _gl;

    public Renderer(GL gl)
    {
        _gl = gl;

        _commandBuffer = new CommandBuffer(_gl);
        _cameraBuffer = PersistentBuffer<CameraData>.CreateMultiBuffer(1, "camera_buffer", _gl);
        
        _program = CreateProgram(_gl);
        
        _dummyVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_dummyVao);

        _program.Use();
    }

    public unsafe void Render(Scene scene)
    {
        Span<CameraData> cameraDataView = [scene.Camera.Data];
        _cameraBuffer.Write(cameraDataView, 0);
        
        var (vertexBufferHandle, indexBufferHandle) = scene.MeshManager.Handle;
        _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, vertexBufferHandle);
        _gl.BindBufferRange(BufferTargetARB.ShaderStorageBuffer, 1, _cameraBuffer.Handle,
            _cameraBuffer.FrameOffsetBytes, (uint)sizeof(CameraData));
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, indexBufferHandle);
        
        var commandCount = _commandBuffer.Build(scene);
        _gl.BindBuffer(BufferTargetARB.DrawIndirectBuffer, _commandBuffer.Handle);

        _gl.MultiDrawElementsIndirect(
            PrimitiveType.Triangles,
            DrawElementsType.UnsignedInt,
            (void*)_commandBuffer.OffsetBytes,
            commandCount,
            0
        );
        
        _commandBuffer.Advance();
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_dummyVao);
        _program.Dispose();
        _commandBuffer.Dispose();
        _cameraBuffer.Dispose();
    }
}