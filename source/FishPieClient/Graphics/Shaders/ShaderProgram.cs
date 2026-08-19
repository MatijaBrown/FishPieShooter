using FishPieClient.Utils;
using Serilog;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Shaders;

public class ShaderProgram : IDisposable
{

    private static void CheckState(uint handle, ProgramPropertyARB state, string name, string message, GL gl)
    {
        gl.GetProgram(handle, state, out int res);
        if (res == 0)
        {
            gl.GetProgramInfoLog(handle, out string infoLog);
            Log.Error("[{Name}] {Message}: \n {InfoLog}", name, message, infoLog);
            throw new Exception($"[{name}] {message}: \n {infoLog}");
        }
    }

    private readonly GL _gl;
    private readonly uint _handle;

    public ShaderProgram(Shader vertexShader, Shader fragmentShader, string name, GL gl)
    {
        _gl = gl;
        
        Errors.Expect(vertexShader.ShaderType == Shader.Type.Vertex, "vertexShader is not a vertex shader");
        Errors.Expect(fragmentShader.ShaderType == Shader.Type.Fragment, "fragmentShader is not a fragment shader");
        
        _handle = _gl.CreateProgram();
        Errors.Ensure(_handle != 0, "failed to create OpenGL Program");
        _gl.ObjectLabel(ObjectIdentifier.Program, _handle, (uint)name.Length, name);

        _gl.AttachShader(_handle, vertexShader.Handle);
        _gl.AttachShader(_handle, fragmentShader.Handle);
        
        _gl.LinkProgram(_handle);
        CheckState(_handle, ProgramPropertyARB.LinkStatus, name, "Failed to link program", _gl);
        
        _gl.ValidateProgram(_handle);
        CheckState(_handle, ProgramPropertyARB.ValidateStatus, name, "Failed to validate program", _gl);
    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }
    
    public void Dispose()
    {
        _gl.UseProgram(0);
        _gl.DeleteProgram(_handle);
    }
    
}