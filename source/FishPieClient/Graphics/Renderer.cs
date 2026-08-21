using FishPieClient.Graphics.Buffers;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics;

public class Renderer : IDisposable
{

    private readonly MultiBuffer<PersistentBuffer<VertexData>, VertexData> _triangleBuffer;
    private readonly uint _dummyVao;
    
    private readonly GL _gl;

    public Renderer(MultiBuffer<PersistentBuffer<VertexData>, VertexData> triangleBuffer, GL gl)
    {
        _triangleBuffer = triangleBuffer;
        _gl = gl;
        
        _dummyVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_dummyVao);
        
        _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 0, _triangleBuffer.Buffer.Handle);
    }

    public unsafe void Render(Buffer<IndirectCommand> commandBuffer)
    {
        _gl.BindBuffer(BufferTargetARB.DrawIndirectBuffer, commandBuffer.Handle);
        
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.MultiDrawArraysIndirect(PrimitiveType.Triangles, (void*)0, 2, 0);
        
        _triangleBuffer.Advance();
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_dummyVao);
        _triangleBuffer.Dispose();
    }
}