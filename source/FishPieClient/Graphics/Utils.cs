using FishPieClient.Graphics.Buffers;
using Serilog;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics;

public static class Utils
{

    public static TBuffer ResizeGpuBuffer<TBuffer, T>(ICollection<T> cpuBuffer, TBuffer gpuBuffer, GL gl)
        where TBuffer : class, IBuffer<T>
        where T : unmanaged
    {
        var name = gpuBuffer.Name;
        var bufferSize = (uint)cpuBuffer.Count;

        var newBuffer = gpuBuffer;
        
        if (gpuBuffer.Size <= bufferSize)
        {
            var newSize = gpuBuffer.Size * 2;
            while (newSize < bufferSize)
            {
                newSize *= 2;
            }

            Log.Information("growing {BufferName} buffer {OldSize} -> {NewSize}", name, gpuBuffer.Size,
                newSize);
            
            // OpenGL barrier in case GPU using previous frame
            gl.Finish();

            newBuffer = gpuBuffer.InstanceCreator(newSize, name, gl) as TBuffer
                ?? throw new InvalidOperationException($"Failed to create gpu buffer {name}");
            gpuBuffer.Dispose();
        }

        return newBuffer;
    }

}