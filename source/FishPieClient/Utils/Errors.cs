using System.Diagnostics;
using Serilog;
using Silk.NET.OpenGL;

namespace FishPieClient.Utils;

public static class Errors
{

    public static void Expect(bool predicate, string msg, params object[] args)
    {
        if (!predicate)
        {
            var trace = new StackTrace();
            Log.Error(msg, args);
            Log.Error("{Trace}", trace.GetFrame(1));
            Environment.Exit(1);
        }
    }

    public static void Ensure(bool predicate, string msg)
    {
        if (!predicate)
        {
            throw new Exception(msg);
        }
    }
    
    public static void CheckGlError(string location, GL gl)
    {
        GLEnum err;
        while ((err = gl.GetError()) != GLEnum.NoError)
        {
            Log.Error($"GL Error {err} at \"{location}\"");
            throw new Exception($"GL Error {err} at \"{location}\"");
        }
    }
    
}