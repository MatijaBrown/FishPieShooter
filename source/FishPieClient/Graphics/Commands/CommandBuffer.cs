using FishPieClient.Graphics.Buffers;
using Serilog;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Commands;

public class CommandBuffer : IDisposable
{

    private readonly GL _gl;

    private MultiBuffer<PersistentBuffer<IndirectCommand>, IndirectCommand> _commandBuffer;

    public uint Handle => _commandBuffer.Buffer.Handle;
    
    public CommandBuffer(GL gl)
    {
        _gl = gl;
        _commandBuffer = PersistentBuffer<IndirectCommand>.CreateMultiBuffer(1, "command_buffer", _gl);
    }

    public uint Build(Scene scene)
    {
        var commands = scene.Entities.ConvertAll(e => new IndirectCommand(
            Count: e.MeshView.Count, InstanceCount: 1, First: e.MeshView.Offset, BaseInstance: 0
        )).ToArray();
        var commandView = new Span<IndirectCommand>(commands);

        if (commandView.Length > _commandBuffer.OriginalSize)
        {
            var newSize = _commandBuffer.OriginalSize * 2;
            while (newSize < commandView.Length)
            {
                newSize *= 2;
            }

            Log.Information("growing command buffer {OriginalSize} -> {NewSize}", _commandBuffer.OriginalSize, newSize);
            
            // OpenGL barrier in case GPU using previous frame
            _gl.Finish();

            _commandBuffer.Dispose();
            _commandBuffer = PersistentBuffer<IndirectCommand>.CreateMultiBuffer(newSize, "command_buffer", _gl);
        }

        _commandBuffer.Write(commandView, 0);
        return (uint)commands.Length;
    }

    public void Advance()
    {
        _commandBuffer.Advance();
    }

    public override string ToString()
    {
        return $"command buffer {_commandBuffer.OriginalSize} size";
    }

    public void Dispose()
    {
        _commandBuffer.Dispose();
    }
}