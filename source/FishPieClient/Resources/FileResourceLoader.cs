namespace FishPieClient.Resources;

public sealed class FileResourceLoader(string root) : IResourceLoader
{

    public string LoadString(string resourceName)
    {
        return File.ReadAllText(Path.Combine(root, resourceName));
    }

    public byte[] LoadBytes(string resourceName)
    {
        return File.ReadAllBytes(Path.Combine(root, resourceName));
    }
    
    public void Dispose() { }
    
}