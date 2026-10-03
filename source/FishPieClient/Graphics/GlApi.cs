using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ARB;

namespace FishPieClient.Graphics;

public sealed record GlApi(GL Gl, ArbBindlessTexture ArbBindless) : IDisposable
{
    
    public void Dispose()
    {
        ArbBindless.Dispose();
        Gl.Dispose();
    }

    public static implicit operator GL(GlApi api) => api.Gl;

}