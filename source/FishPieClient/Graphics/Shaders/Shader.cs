using System.Reflection;
using Serilog;
using Silk.NET.OpenGL;

namespace FishPieClient.Graphics.Shaders;

public class Shader : IDisposable
{
    
    public enum Type
    {
        Vertex,
        Fragment
    }

    private readonly GL _gl;

    public Type ShaderType { get; }
    
    public uint Handle { get; }
    
    public Shader(string source, Type type, string name, GL gl)
    {
        _gl = gl;
        ShaderType = type;

        Handle = _gl.CreateShader(ToNative(type));
        _gl.ObjectLabel(ObjectIdentifier.Shader, Handle, (uint)name.Length, name);
       
        string code = LoadShaderCode(source);
        _gl.ShaderSource(Handle, code);
        _gl.CompileShader(Handle);

        _gl.GetShader(Handle, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
        {
            _gl.GetShaderInfoLog(Handle, out string infoLog);
            Log.Error("Failed to compile {ShaderType}-Shader {Name} \n {InfoLog}", ShaderType, name, infoLog);
            throw new Exception($"Failed to compile {ShaderType}-Shader \"{name}\" \n {infoLog}");
        }
    }

    public void Dispose()
    {
        _gl.DeleteShader(Handle);
    }
    
    private static string LoadShaderCode(string shaderName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream =
            assembly.GetManifestResourceStream("FishPieClient.Assets.Shaders." + shaderName + ".glsl");
        if (stream == null)
        {
            throw new FileLoadException("Failed to load shader: " + shaderName);
        }
                
        var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static ShaderType ToNative(Type type)
    {
        return type switch
        {
            Type.Vertex => Silk.NET.OpenGL.ShaderType.VertexShader,
            Type.Fragment => Silk.NET.OpenGL.ShaderType.FragmentShader,
            _ => throw new ArgumentException($"Unknown shader type {type}", nameof(type))
        };
    }
    
}