using System.Diagnostics;
using Serilog;

namespace FishPieClient.Utils;

public static class Asserts
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
    
}