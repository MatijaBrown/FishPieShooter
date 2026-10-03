using FishPieClient.Graphics.Buffers;
using FishPieClient.Graphics.Textures;
using Serilog;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace FishPieClient.Graphics;

public static class Utils
{

    public static TBuffer ResizeGpuBuffer<TBuffer, T>(uint bufferSize, TBuffer gpuBuffer, GL gl)
        where TBuffer : class, IBuffer<T>
        where T : unmanaged
    {
        var name = gpuBuffer.Name;

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
    
    public static TBuffer ResizeGpuBuffer<TBuffer, T>(ICollection<T> cpuBuffer, TBuffer gpuBuffer, GL gl)
        where TBuffer : class, IBuffer<T>
        where T : unmanaged
        => ResizeGpuBuffer<TBuffer, T>((uint)cpuBuffer.Count, gpuBuffer, gl);
    
    public static TBuffer ResizeGpuBuffer<TBuffer, T>(Span<T> cpuBuffer, TBuffer gpuBuffer, GL gl)
        where TBuffer : class, IBuffer<T>
        where T : unmanaged
        => ResizeGpuBuffer<TBuffer, T>((uint)cpuBuffer.Length, gpuBuffer, gl);

    public static TextureData LoadTexture(byte[] textureData)
    {
        var image = ImageResult.FromMemory(textureData);
        return new TextureData(
            Width: (uint)image.Width,
            Height: (uint)image.Height,
            Format: ComponentsToFormat(image.Comp),
            Data: image.Data
        );

        static TextureFormat ComponentsToFormat(ColorComponents components)
        {
            return components switch
            {
                ColorComponents.RedGreenBlueAlpha => TextureFormat.Rgba,
                ColorComponents.RedGreenBlue => TextureFormat.Rgb,
                _ => throw new NotSupportedException($"Colour Format {components} is not supported")
            };
        }
    }
    
}