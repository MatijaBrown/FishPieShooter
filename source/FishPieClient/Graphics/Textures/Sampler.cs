using FishPieClient.Utils;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Textures;

public sealed class Sampler : IDisposable
{

    private static GLEnum ToOpenGl(FilterType filterType)
    {
        return filterType switch
        {
            FilterType.LinearMipmap => GLEnum.LinearMipmapLinear,
            FilterType.Linear => GLEnum.Linear,
            FilterType.Nearest => GLEnum.Nearest,
            _ => throw new NotSupportedException($"filterType {filterType} not supported")
        };
    }
    
    private readonly GL _gl;
    
    public uint Handle { get; }
    
    public string Name { get; }

    public Sampler(FilterType minFilter, FilterType magFilter, string name, GL gl, float? anisotropySamples = null)
    {
        _gl = gl;
        Name = name;
        
        Handle = _gl.CreateSampler();
        _gl.ObjectLabel(ObjectIdentifier.Sampler, Handle, (uint)Name.Length, Name);
        
        _gl.SamplerParameter(Handle, SamplerParameterI.MinFilter, (int)ToOpenGl(minFilter));
        _gl.SamplerParameter(Handle, SamplerParameterI.MagFilter, (int)ToOpenGl(magFilter));
        
        if (anisotropySamples.HasValue)
        {
            Errors.Expect(anisotropySamples.Value >= 1.0f, $"invalid samples: {anisotropySamples.Value}");
            _gl.SamplerParameter(Handle, SamplerParameterF.MaxAnisotropy, anisotropySamples.Value);
        }
    }
    
    public void Dispose()
    {
        _gl.DeleteSampler(Handle);
    }

}