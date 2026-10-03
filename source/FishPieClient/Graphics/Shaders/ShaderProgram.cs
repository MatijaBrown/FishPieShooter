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

    public uint Handle { get; }

    public ShaderProgram(Shader vertexShader, Shader fragmentShader, string name, GL gl)
    {
        _gl = gl;
        
        Errors.Expect(vertexShader.ShaderType == Shader.Type.Vertex, "vertexShader is not a vertex shader");
        Errors.Expect(fragmentShader.ShaderType == Shader.Type.Fragment, "fragmentShader is not a fragment shader");
        
        Handle = _gl.CreateProgram();
        Errors.Ensure(Handle != 0, "failed to create OpenGL Program");
        _gl.ObjectLabel(ObjectIdentifier.Program, Handle, (uint)name.Length, name);

        _gl.AttachShader(Handle, vertexShader.Handle);
        _gl.AttachShader(Handle, fragmentShader.Handle);
        
        _gl.LinkProgram(Handle);
        CheckState(Handle, ProgramPropertyARB.LinkStatus, name, "Failed to link program", _gl);
        
        _gl.ValidateProgram(Handle);
        CheckState(Handle, ProgramPropertyARB.ValidateStatus, name, "Failed to validate program", _gl);
    }

    public void Use()
    {
        _gl.UseProgram(Handle);
    }

    public int GetAttribLocation(string name)
    {
        return _gl.GetAttribLocation(Handle, name);
    }

    public int GetUniformLocation(string name)
    {
        return _gl.GetUniformLocation(Handle, name);
    }
    
    public void Dispose()
    {
        _gl.UseProgram(0);
        _gl.DeleteProgram(Handle);
    }
    
}