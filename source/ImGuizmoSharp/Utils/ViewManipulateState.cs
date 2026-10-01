using System.Numerics;

namespace ImGuizmoSharp.Utils;

internal record ViewManipulateState
{

    internal uint Id = uint.MaxValue;
    
    internal bool IsDragging = false;
    internal bool IsClicking = false;
    internal int InterpolationFrames = 0;
    internal Vector4 InterpolationUp = default;
    internal Vector4 InterpolationDir = default;

}