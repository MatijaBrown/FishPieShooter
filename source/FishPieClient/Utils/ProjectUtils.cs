using System.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace FishPieClient.Utils;

public static class ProjectUtils
{

    public static string FullVersion
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion ?? "(unavailable)";
        }
    }

    public static int Major
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductMajorPart;
        }
    }
    
    public static int Minor
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductMinorPart;
        }
    }
    
    public static int Build
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductBuildPart;
        }
    }
    
    public static string FullName
    {
        get
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductName ?? "(unavailable)";
        }
    }

    public static bool LoggingEnabled
        => ConfigurationManager.AppSettings["logging_enabled"] == "true";
    
    public static bool LogToFile
        => ConfigurationManager.AppSettings["log_to_file"] == "true";
    
    public static bool OpenGlDebugEnabled
        => ConfigurationManager.AppSettings["opengl_debug_enabled"] == "true";

}