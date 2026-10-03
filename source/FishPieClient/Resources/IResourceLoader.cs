namespace FishPieClient.Resources;

public interface IResourceLoader : IDisposable
{
    
    public string LoadString(string resourceName);
    
    public byte[] LoadBytes(string resourceName);
    
}