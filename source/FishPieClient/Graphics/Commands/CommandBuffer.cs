using System.Runtime.InteropServices;
using FishPieClient.Core;
using FishPieClient.Graphics.Buffers;
using Serilog;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Commands;

public class CommandBuffer : IDisposable
{

    private readonly GL _gl;

    private MultiBuffer<PersistentBuffer<IndirectCommand>, IndirectCommand> _commandBuffer;

    public uint Handle => _commandBuffer.Buffer.Handle;

    public int OffsetBytes => _commandBuffer.FrameOffsetBytes;
    
    public CommandBuffer(GL gl)
    {
        _gl = gl;
        _commandBuffer = PersistentBuffer<IndirectCommand>.CreateMultiBuffer(1, "command_buffer", _gl);
    }

    public uint Build(Scene scene)
    {
        var commands = scene.Entities.ConvertAll(e =>
            new IndirectCommand(
                Count: (uint)e.MeshView.Indices.Length,
                InstanceCount: 1,
                First: e.MeshView.IndexOffset,
                BaseVertex: (int)e.MeshView.VertexOffset,
                BaseInstance: 0
        ));
        var commandView = CollectionsMarshal.AsSpan(commands);
        
        _commandBuffer = Utils.ResizeGpuBuffer(commands, _commandBuffer, _gl);
        _commandBuffer.Write(commandView, 0);
        
        return (uint)commands.Count;
    }

    public void Advance()
    {
        _commandBuffer.Advance();
    }

    public override string ToString()
    {
        return $"command buffer {_commandBuffer.Size} size";
    }

    public void Dispose()
    {
        _commandBuffer.Dispose();
    }
}