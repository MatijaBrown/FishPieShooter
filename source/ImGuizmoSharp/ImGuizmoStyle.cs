using System.Numerics;

namespace ImGuizmoSharp;

public record ImGuizmoStyle
{

    public readonly Vector4[] Colours = new Vector4[(int)ImGuizmoColour.Count];
    
    public float TranslationLineThickness = 3.0f;
    public float TranslationLineArrowSize = 6.0f;
    public float RotationLineThickness = 2.0f;
    public float RotationOuterLineThickness = 3.0f;
    public float ScaleLineThickness = 3.0f;
    public float ScaleLineCircleSize = 6.0f;
    public float HatchedAxisLineThickness = 6.0f;
    public float CenterCircleSize = 6.0f;

    public ImGuizmoStyle()
    {
        Colours[(int)ImGuizmoColour.DirectionX] = new Vector4(0.666f, 0.000f, 0.000f, 1.000f);
        Colours[(int)ImGuizmoColour.DirectionY] = new Vector4(0.000f, 0.666f, 0.000f, 1.000f);
        Colours[(int)ImGuizmoColour.DirectionZ] = new Vector4(0.000f, 0.000f, 0.666f, 1.000f);
        Colours[(int)ImGuizmoColour.PlaneX] = new Vector4(0.666f, 0.000f, 0.000f, 0.380f);
        Colours[(int)ImGuizmoColour.PlaneY] = new Vector4(0.666f, 0.000f, 0.000f, 0.380f);
        Colours[(int)ImGuizmoColour.PlaneZ] = new Vector4(0.666f, 0.000f, 0.000f, 0.380f);
        Colours[(int)ImGuizmoColour.Selection] = new Vector4(1.000f, 0.500f, 0.062f, 0.541f);
        Colours[(int)ImGuizmoColour.Inactive] = new Vector4(0.600f, 0.600f, 0.600f, 0.600f);
        Colours[(int)ImGuizmoColour.TranslationLine] = new Vector4(0.666f, 0.666f, 0.666f, 0.666f);
        Colours[(int)ImGuizmoColour.ScaleLine] = new Vector4(0.250f, 0.250f, 0.250f, 1.000f);
        Colours[(int)ImGuizmoColour.RotationUsingBorder] = new Vector4(1.000f, 0.500f, 0.062f, 1.000f);
        Colours[(int)ImGuizmoColour.RotationUsingFill] = new Vector4(1.000f, 0.500f, 0.062f, 0.500f);
        Colours[(int)ImGuizmoColour.HatchedAxisLines] = new Vector4(0.000f, 0.000f, 0.000f, 0.500f);
        Colours[(int)ImGuizmoColour.Text] = new Vector4(1.000f, 1.000f, 1.000f, 1.000f);
        Colours[(int)ImGuizmoColour.TextShadow] = new Vector4(0.000f, 0.000f, 0.000f, 1.000f);
    }

}