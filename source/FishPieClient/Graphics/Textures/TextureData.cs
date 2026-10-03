namespace FishPieClient.Graphics.Textures;

public record TextureData(
    uint Width,
    uint Height,
    TextureFormat Format,
    byte[] Data
);