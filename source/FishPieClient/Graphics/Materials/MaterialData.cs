using System.Runtime.InteropServices;

namespace FishPieClient.Graphics.Materials;

[StructLayout(LayoutKind.Sequential)]
public readonly struct MaterialData(Colour colour)
{

    public readonly Colour Colour = colour;

}