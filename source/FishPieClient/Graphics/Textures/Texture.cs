using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Textures;

public sealed class Texture : IDisposable
{

    private static GLEnum ToOpenGl(TextureFormat format, bool includeSize)
    {
        return format switch
        {
            TextureFormat.Rgb => includeSize ? GLEnum.Rgb8 : GLEnum.Rgb,
            TextureFormat.Rgba => includeSize ? GLEnum.Rgba8 : GLEnum.Rgba,
            _ => throw new NotSupportedException($"unknown texture format {format}")
        };
    }

    private readonly GlApi _api;

    private readonly uint _textureId;
    
    public ulong Handle { get; }
    
    public string Name { get; }
    
    public Texture(TextureData texture, string name, Sampler sampler, GlApi api)
    {
        _api = api;
        
        Name = name;

        _textureId = _api.Gl.CreateTexture(TextureTarget.Texture2D);
        _api.Gl.ObjectLabel(ObjectIdentifier.Texture, _textureId, (uint)name.Length, name);
        
        _api.Gl.TextureStorage2D(_textureId, 1, ToOpenGl(texture.Format, true), texture.Width, texture.Height);
        _api.Gl.TextureSubImage2D(_textureId, 0, 0, 0, texture.Width, texture.Height, ToOpenGl(texture.Format, false),
            PixelType.UnsignedByte, texture.Data);

        Handle = _api.ArbBindless.GetTextureSamplerHandle(_textureId, sampler.Handle);
        _api.ArbBindless.MakeTextureHandleResident(Handle);
    }
    
    public void Dispose()
    {
        _api.ArbBindless.MakeTextureHandleNonResident(Handle);
        _api.Gl.DeleteTexture(_textureId);
    }

}