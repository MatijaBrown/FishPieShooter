using System.Runtime.InteropServices;

namespace FishPieClient.Graphics.Materials;

[StructLayout(LayoutKind.Sequential)]
public struct MaterialData(Colour colour)
{

    public Colour Colour = colour;

}