namespace ImGuizmoSharp;

[Flags]
public enum ImGuizmoOperation : uint
{
    
    TranslateX = 1 << 0,
    TranslateY = 1 << 1,
    TranslateZ = 1 << 2,
    RotateX = 1 << 3,
    RotateY = 1 << 4,
    RotateZ = 1 << 5,
    RotateScreen =  1 << 6,
    ScaleX = 1 << 7,
    ScaleY = 1 << 8,
    ScaleZ = 1 << 9,
    Bounds = 1 << 10,
    ScaleXu = 1 << 11,
    ScaleYu = 1 << 12,
    ScaleZu = 1 << 13,
    
    Translate = TranslateX | TranslateY | TranslateZ,
    Rotate = RotateX | RotateY | RotateZ | RotateScreen,
    Scale = ScaleX | ScaleY | ScaleZ,
    ScaleU = ScaleXu | ScaleYu | ScaleZu,
    Universal = Translate | Rotate | ScaleU
    
}