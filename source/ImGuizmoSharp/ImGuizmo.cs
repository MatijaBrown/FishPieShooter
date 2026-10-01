using System.Numerics;
using Hexa.NET.ImGui;
using ImGuizmoSharp.Utils;

namespace ImGuizmoSharp;

public static class ImGuizmo
{

    private const float ScreenRotateSize = 0.06f;
    private const float RotationDisplayFactor = 1.2f;

    private static bool IsTranslateType(ImGuizmoMoveType type)
    {
        return type is >= ImGuizmoMoveType.MoveX and <= ImGuizmoMoveType.MoveScreen;
    }

    private static bool IsRotateType(ImGuizmoMoveType type)
    {
        return type is >= ImGuizmoMoveType.RotateX and <= ImGuizmoMoveType.RotateScreen;
    }

    private static bool IsScaleType(ImGuizmoMoveType type)
    {
        return type is >= ImGuizmoMoveType.ScaleX and <= ImGuizmoMoveType.ScaleXYZ;
    }

    private static readonly ImGuizmoOperation[] TranslatePlans =
    [
        ImGuizmoOperation.TranslateY | ImGuizmoOperation.TranslateZ,
        ImGuizmoOperation.TranslateX | ImGuizmoOperation.TranslateZ,
        ImGuizmoOperation.TranslateX | ImGuizmoOperation.TranslateY
    ];

    private static ImDrawListPtr _drawList;
    private static uint _windowId = uint.MaxValue;
    
    private static ImGuizmoStyle _style = new();

    private static ImGuizmoMode _mode;
    private static Matrix4x4 _viewMat;
    private static Matrix4x4 _projectionMat;
    private static Matrix4x4 _model;
    private static Matrix4x4 _modelLocal;
    private static Matrix4x4 _modelInverse;
    private static Matrix4x4 _modelSource;
    private static Matrix4x4 _modelSourceInverse;
    private static Matrix4x4 _mvp;
    private static Matrix4x4 _mvpLocal;
    private static Matrix4x4 _viewProjection;

    private static Vector4 _modelScaleOrigin;
    private static Vector4 _cameraEye;
    private static Vector4 _cameraRight;
    private static Vector4 _cameraDir;
    private static Vector4 _cameraUp;
    private static Vector4 _rayOrigin;
    private static Vector4 _rayVector;

    private static float _radiusSquareCenter;
    private static Vector2 _screenSquareCenter;
    private static Vector2 _screenSquareMin;
    private static Vector2 _screenSquareMax;

    private static float _screenFactor;
    private static Vector4 _relativeOrigin;

    private static bool _using = false;
    private static bool _usingViewManipulate = false;
    private static bool _enable = true;
    private static bool _mouseOver;
    private static bool _isViewManipulatorHovered = false;

    private static Vector4 _translationPlan;
    private static Vector4 _translationPlanOrigin;
    private static Vector4 _matrixOrigin;
    private static Vector4 _translationLastDelta;

    private static Vector4 _rotationVectorSource;
    private static float _rotationAngle;
    private static float _rotationAngleOrigin;

    private static Vector4 _scale;
    private static Vector4 _scaleValueOrigin;
    private static Vector4 _scaleLast;
    private static float _saveMousePosX;

    private static int _axisMask = 0;

    private static TripodState _tripodState = new();
    private static TripodState _activeTripodState = new();

    private static float _axisLimit = 0.0025f;
    private static float _planeLimit = 0.02f;

    private static Vector4 _boundsPivot;
    private static Vector4 _boundsAnchor;
    private static Vector4 _boundsPlan;
    private static Vector4 _boundsLocalPivot;
    private static int _boundsBestAxi;
    
    private static readonly int[] _boundsAxis = new int[2];

    private static bool _usingBounds = false;
    private static Matrix4x4 _boundsMatrix;

    private static ImGuizmoMoveType _currentHandleType = ImGuizmoMoveType.None;
    private static ImGuizmoMoveType _hoveredHandleType = ImGuizmoMoveType.None;
    
    private static float _x = 0.0f;
    private static float _y = 0.0f;
    private static float _width = 0.0f;
    private static float _height = 0.0f;
    private static float _xMax = 0.0f;
    private static float _yMax = 0.0f;
    private static float _displayRatio = 1.0f;
    
    private static bool _isOrthographic = false;
    private static bool _overGizmoHotspot = false;
    private static bool _overGizmoHotspotLastFrame = false;

    private static ImGuiWindowPtr _alternativeWindow = default;
    
    private static readonly Stack<uint> _idStack = [];
    
    private static uint _editingId = uint.MaxValue;
    private static ImGuizmoOperation _operation = 0;

    private static readonly List<ViewManipulateState> _viewManipulateStates = [];

    private static bool _allowAxisFlip = true;
    private static float _gizmoSizeClipSpace = 0.1f;

    private static uint CurrentId
    {
        get
        {
            if (_idStack.Count == 0)
            {
                _idStack.Push(uint.MaxValue);
            }
            
            return _idStack.Peek();
        }
    }

    private static TripodState TripodState =>
        _using && CurrentId == _editingId ? _activeTripodState : _tripodState;

    private static ViewManipulateState ViewManipulateState
    {
        get
        {
            uint id = CurrentId;
            foreach (var manipulateState in _viewManipulateStates)
            {
                if (manipulateState.Id == id)
                {
                    return manipulateState;
                }
            }

            var state = new ViewManipulateState { Id = id };
            _viewManipulateStates.Add(state);
            return _viewManipulateStates[^1];
        }
    }

    private static readonly Vector4[] DirectionUnary =
    [
        Vector4.UnitX, Vector4.UnitY, Vector4.UnitZ
    ];

    private static readonly string[] TranslationInfoMask =
    [
        "X : {0:F3}", "Y : {0:F3}", "Z : {0:F3}",
        "Y : {0:F3} Z : {1:F3}", "X : {0:F3} Z : {1:F3}", "X : {0:F3} Y : {1:F3}",
        "X : {0:F3} Y : {1:F3} Z : {2:F3}"
    ];

    private static readonly string[] ScaleInfoMask =
    [
        "X : {0:F2}", "Y : {0:F2}", "Z : {0:F2}", "XYZ : {0:F2}"
    ];

    private static readonly string[] RotationInfoMask =
    [
        "X : {0:F2} deg {1:F2} rad", "Y {0:F2} deg {1:F2} rad", "Z : {0:F2} deg {1:F2} rad",
        "Screen : {0:F2} deg {1:F2} rad"
    ];

    private static readonly int[] TranslationInfoIndex =
    [
        0, 0, 0,
        1, 0, 0,
        2, 0, 0,
        1, 2, 0,
        0, 2, 0,
        0, 1, 0,
        0, 1, 2
    ];

    private const float QuadMin = 0.5f;
    private const float QuadMax = 0.8f;

    private static readonly float[] QuadUv =
    [
        QuadMin, QuadMin, QuadMin, QuadMax, QuadMax, QuadMax, QuadMax, QuadMin
    ];

    private const int HalfCircleSegmentCount = 64;
    private const float SnapTension = 0.5f;

    private static uint GetColourU32(int idx)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(idx, (int)ImGuizmoColour.Count);
        return ImGui.ColorConvertFloat4ToU32(_style.Colours[idx]);
    }

    private static Vector2 WorldToPos(Vector4 worldPos, Matrix4x4 mat, Vector2 position, Vector2 size)
    {
        var trans = Maths.TransformPoint(worldPos, mat);
        if (MathF.Abs(trans.W) < float.Epsilon)
            return new Vector2(-float.MaxValue, -float.MaxValue);
        trans *= 0.5f / trans.W;
        trans += new Vector4(0.5f, 0.5f, 0.0f, 0.0f);
        trans.Y = 1.0f - trans.Y;
        trans.X *= size.X;
        trans.Y *= size.Y;
        trans.X += position.X;
        trans.Y += position.Y;
        return new Vector2(trans.X, trans.Y);
    }

    private static Vector2 WorldToPos(Vector4 worldPos, Matrix4x4 mat)
        => WorldToPos(worldPos, mat, new Vector2(_x, _y), new Vector2(_width, _height));

    private static void ComputeCameraRay(out Vector4 rayOrigin, out Vector4 rayDir, Matrix4x4 viewMatrix,
        Matrix4x4 projectionMatrix, Vector2 mousePosition, Vector2 position, Vector2 size)
    {
        Matrix4x4.Invert(viewMatrix * projectionMatrix, out var viewProjInverse);

        float mox = ((mousePosition.X - position.X) / size.X) * 2.0f - 1.0f;
        float moy = (1.0f - ((mousePosition.Y - position.Y) / size.Y)) * 2.0f - 1.0f;

        var hA = Vector4.Transform(new Vector4(mox, moy, 0, 1), viewProjInverse);
        var hB = Vector4.Transform(new Vector4(mox, moy, 1.0f - float.Epsilon, 1.0f), viewProjInverse);
        bool aAtInfinity = MathF.Abs(hA.W) < float.Epsilon;
        bool bAtInfinity = MathF.Abs(hB.W) < float.Epsilon;
        var pointA = aAtInfinity ? hA : hA * (1.0f / hA.W);
        var pointB = bAtInfinity ? hB : hB * (1.0f / hB.W);

        Matrix4x4.Invert(viewMatrix, out var viewInverse);
        var eye = viewInverse.Position;

        Vector4 nearPoint, farPoint;
        bool farAtInfinity;
        if ((pointA - eye).CLengthSq() <= (pointB - eye).CLengthSq())
        {
            nearPoint = pointA;
            farPoint = pointB;
            farAtInfinity = bAtInfinity;
        }
        else
        {
            nearPoint = pointB;
            farPoint = pointA;
            farAtInfinity = aAtInfinity;
        }

        rayOrigin = nearPoint;
        rayDir = farAtInfinity ? Maths.Normalize(nearPoint - eye) : Maths.Normalize(farPoint - nearPoint);
    }

    private static void ComputeCameraRay(out Vector4 rayOrigin, out Vector4 rayDir, Vector2 position, Vector2 size)
    {
        var io = ImGui.GetIO();
        ComputeCameraRay(out rayOrigin, out rayDir, _viewMat, _projectionMat, io.MousePos, position, size);
    }

    private static void ComputeCameraRay(out Vector4 rayOrigin, out Vector4 rayDir)
    {
        ComputeCameraRay(out rayOrigin, out rayDir, new Vector2(_x, _y), new Vector2(_width, _height));
    }

    private static void ComputeMouseRay(Matrix4x4 view, Matrix4x4 projection, Vector2 mousePosition,
        Vector2 rectPosition, Vector2 rectSize, out Vector3 rayOrigin, out Vector3 rayDirection)
    {
        ComputeCameraRay(out var origin, out var dir, view, projection, mousePosition, rectPosition, rectSize);
        rayOrigin = new Vector3(origin.X, origin.Y, origin.Z);
        rayDirection = new Vector3(dir.X, dir.Y, dir.Z);
    }
    
    private static float GetSegmentLengthClipSpace(Vector4 start, Vector4 end, bool localCoordinates = false)
    {
        var startOfSegment = start;
        var mvp = localCoordinates ? _mvpLocal : _mvp;
        startOfSegment = Maths.TransformPoint(startOfSegment, mvp);

        if (MathF.Abs(startOfSegment.W) > float.Epsilon)
        {
            startOfSegment *= 1.0f / startOfSegment.W;
        }
        
        var endOfSegment = end;
        endOfSegment = Maths.TransformPoint(endOfSegment, mvp);
        if (MathF.Abs(endOfSegment.W) > float.Epsilon)
        {
            endOfSegment *= 1.0f / endOfSegment.W;
        }
        
        var clipAxisSpace = endOfSegment - startOfSegment;
        if (_displayRatio < 1.0f)
        {
            clipAxisSpace.X *= _displayRatio;
        }
        else
        {
            clipAxisSpace.Y /= _displayRatio;
        }
        
        float segmentLengthInClipSpace = MathF.Sqrt(clipAxisSpace.X * clipAxisSpace.X + clipAxisSpace.Y * clipAxisSpace.Y);
        return segmentLengthInClipSpace;
    }

    private static float GetParallelogram(Vector4 pt0, Vector4 ptA, Vector4 ptB)
    {
        Span<Vector4> pts = [pt0, ptA, ptB];
        for (int i = 0; i < 3; i++)
        {
            pts[i] = Maths.TransformPoint(pts[i], _mvp);
            if (MathF.Abs(pts[i].W) > float.Epsilon)
            {
                pts[i] *= 1.0f / pts[i].W;
            }
        }

        var segA = pts[1] - pts[0];
        var segB = pts[2] - pts[0];
        segA.Y /= _displayRatio;
        segB.Y /= _displayRatio;
        var segAOrtho = new Vector4(-segA.Y, -segA.X, 0.0f, 0.0f);
        segAOrtho = Maths.Normalize(segAOrtho);
        float dt = Maths.Dot3(segAOrtho, segB);
        float surface = MathF.Sqrt(segA.X * segA.X + segA.Y * segA.Y) * MathF.Abs(dt);
        return surface;
    }
    
    public static unsafe void BeginFrame()
    {
        const ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize |
                                       ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoInputs |
                                       ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing |
                                       ImGuiWindowFlags.NoBringToFrontOnFocus;

        ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().Pos);

        ImGui.PushStyleColor(ImGuiCol.WindowBg, 0);
        ImGui.PushStyleColor(ImGuiCol.Border, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);

        ImGui.Begin("gizmo", null, flags);
        _windowId = ImGui.GetCurrentContext().CurrentWindow.ID;
        _drawList = ImGui.GetWindowDrawList();
        _overGizmoHotspotLastFrame = _overGizmoHotspot;
        _overGizmoHotspot = false;
        
        _usingViewManipulate = false;
        _isViewManipulatorHovered = false;
        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
    }

    public static void Enable(bool enable)
    {
        _enable = enable;
        if (!_enable)
        {
            if (!_using || CurrentId == _editingId)
            {
                _using = false;
                _usingBounds = false;
                _currentHandleType = ImGuizmoMoveType.None;
                _hoveredHandleType = ImGuizmoMoveType.None;
            }
        }
    }

    private static void ComputeContext(Matrix4x4 view, Matrix4x4 projection, ref Matrix4x4 matrix, ImGuizmoMode mode)
    {
        _mode = mode;
        _viewMat = view;
        _projectionMat = projection;
        _mouseOver = IsHoveringWindow;

        _modelLocal = Maths.OrthoNormalize(matrix);

        if (mode == ImGuizmoMode.Local)
        {
            _model = _modelLocal;
        }
        else
        {
            _model = Matrix4x4.CreateTranslation(matrix.Translation);
        }

        _modelSource = matrix;
        _modelScaleOrigin = new Vector4(_modelSource.Right.CLength(), _modelSource.Up.CLength(),
            _modelSource.Dir.CLength(), 0.0f);

        Matrix4x4.Invert(_model, out _modelInverse);
        Matrix4x4.Invert(_modelSource, out _modelSourceInverse);
        _viewProjection = _viewMat * _projectionMat;
        _mvp = _model * _viewProjection;
        _mvpLocal = _modelLocal * _viewProjection;

        Matrix4x4.Invert(_viewMat, out var viewInverse);
        _cameraDir = viewInverse.Dir;
        _cameraEye = viewInverse.Position;
        _cameraRight = viewInverse.Right;
        _cameraUp = viewInverse.Up;

        var pointRight = viewInverse.Right;
        pointRight = Maths.TransformPoint(pointRight, _viewProjection);

        var rightViewInverse = viewInverse.Right;
        rightViewInverse = Maths.TransformVector(rightViewInverse, _modelInverse);
        float rightLength = GetSegmentLengthClipSpace(Vector4.Zero, rightViewInverse);
        _screenFactor = _gizmoSizeClipSpace / rightLength;

        var centerSSpace = WorldToPos(Vector4.Zero, _mvp);
        _screenSquareCenter = centerSSpace;
        _screenSquareMin = new Vector2(centerSSpace.X - 10.0f, centerSSpace.Y - 10.0f);
        _screenSquareMax = new Vector2(centerSSpace.X + 10.0f, centerSSpace.Y + 10.0f);

        ComputeCameraRay(out _rayOrigin, out _rayVector);
    }

    private static void ComputeColours(Span<uint> colours, ImGuizmoMoveType type, ImGuizmoOperation operation)
    {
        if (_enable)
        {
            uint selectionColour = GetColourU32((int)ImGuizmoColour.Selection);

            switch (operation)
            {
            case ImGuizmoOperation.Translate:
                colours[0] = (type == ImGuizmoMoveType.MoveScreen) ? selectionColour : 0xFFFFFFFF;
                for (int i = 0; i < 3; i++)
                {
                    colours[i + 1] = (type == ImGuizmoMoveType.MoveX + i)
                        ? selectionColour
                        : GetColourU32((int)ImGuizmoColour.DirectionX + i);
                    colours[i + 4] = (type == ImGuizmoMoveType.MoveYZ + i)
                        ? selectionColour
                        : GetColourU32((int)ImGuizmoColour.PlaneX + i);
                    colours[i + 4] = (type == ImGuizmoMoveType.MoveScreen)
                        ? selectionColour
                        : colours[i + 4];
                }

                break;
            case ImGuizmoOperation.Rotate:
                colours[0] = (type == ImGuizmoMoveType.RotateScreen) ? selectionColour : 0xFFFFFFFF;
                for (int i = 0; i < 3; i++)
                {
                    colours[i + 1] = (type == ImGuizmoMoveType.RotateX + i)
                        ? selectionColour
                        : GetColourU32((int)ImGuizmoColour.DirectionX + i);
                }
                break;
            case ImGuizmoOperation.ScaleU:
            case ImGuizmoOperation.Scale:
                colours[0] = (type == ImGuizmoMoveType.ScaleXYZ) ? selectionColour : 0xFFFFFFFF;
                for (int i = 0; i < 3; i++)
                {
                    colours[i + 1] = (type == ImGuizmoMoveType.ScaleX + i)
                        ? selectionColour
                        : GetColourU32((int)ImGuizmoColour.DirectionX + i);
                }
                break;
            default:
                break;
            }
        }
        else
        {
            var inactiveColour = GetColourU32((int)ImGuizmoColour.Inactive);
            for (int i = 0; i < 7; i++)
            {
                colours[i] = inactiveColour;
            }
        }
    }

    private static void ComputeTripodAxisAndVisibility(int axisIndex, out Vector4 dirAxis, out Vector4 dirPlaneX,
        out Vector4 dirPlaneY, out bool belowAxisLimit, out bool belowPlaneLimit, bool localCoordinates = false)
    {
        dirAxis = DirectionUnary[axisIndex];
        dirPlaneX = DirectionUnary[(axisIndex + 1) % 3];
        dirPlaneY = DirectionUnary[(axisIndex + 2) % 3];
        var tripodState = TripodState;

        if (_using && (CurrentId == _editingId))
        {
            belowAxisLimit = tripodState.BelowAxisLimits[axisIndex] && (((1 << axisIndex) & _axisMask) == 0);
            belowPlaneLimit = tripodState.BelowAxisLimits[axisIndex] &&
                              (((1 << axisIndex) == _axisMask) || (_axisMask == 0));

            dirAxis *= tripodState.AxisFactor[axisIndex];
            dirPlaneX *= tripodState.AxisFactor[(axisIndex + 1) % 3];
            dirPlaneY *= tripodState.AxisFactor[(axisIndex + 2) % 3];
        }
        else
        {
            float lenDir = GetSegmentLengthClipSpace(Vector4.Zero, dirAxis, localCoordinates);
            float lenDirMinus = GetSegmentLengthClipSpace(Vector4.Zero, -dirAxis, localCoordinates);
            
            float lenDirPlaneX = GetSegmentLengthClipSpace(Vector4.Zero, dirPlaneX, localCoordinates);
            float lenDirMinusPlaneX = GetSegmentLengthClipSpace(Vector4.Zero, -dirPlaneX, localCoordinates);
            
            float lenDirPlaneY = GetSegmentLengthClipSpace(Vector4.Zero, dirPlaneY, localCoordinates);
            float lenDirMinusPlaneY = GetSegmentLengthClipSpace(Vector4.Zero, -dirPlaneY, localCoordinates);

            bool allowFlip = _allowAxisFlip;
            float mulAxis = (allowFlip && lenDir < lenDirMinus && MathF.Abs(lenDir - lenDirMinus) > float.Epsilon)
                ? -1.0f
                : 1.0f;
            float mulAxisX = (allowFlip && lenDirPlaneX < lenDirMinusPlaneX &&
                              MathF.Abs(lenDirPlaneX - lenDirMinusPlaneX) > float.Epsilon)
                ? -1.0f
                : 1.0f;
            float mulAxisY =
                (allowFlip && lenDirPlaneY < lenDirMinusPlaneY &&
                 MathF.Abs(lenDirPlaneY - lenDirMinusPlaneY) > float.Epsilon)
                    ? -1.0f
                    : 1.0f;

            dirAxis *= mulAxis;
            dirPlaneX *= mulAxisX;
            dirPlaneY *= mulAxisY;

            float axisLengthInClipSpace =
                GetSegmentLengthClipSpace(Vector4.Zero, dirAxis * _screenFactor, localCoordinates);

            float paraSurf = GetParallelogram(Vector4.Zero, dirPlaneX * _screenFactor, dirPlaneY * _screenFactor);
            belowPlaneLimit = (paraSurf > _axisLimit) && (((1 << axisIndex) == _axisMask) || (_axisMask == 0));
            belowAxisLimit = (axisLengthInClipSpace > _planeLimit) && (((1 << axisIndex) & _axisMask) == 0);

            tripodState.AxisFactor[axisIndex] = mulAxis;
            tripodState.AxisFactor[(axisIndex + 1) % 3] = mulAxisX;
            tripodState.AxisFactor[(axisIndex + 1) % 3] = mulAxisY;
            tripodState.BelowAxisLimits[axisIndex] = belowAxisLimit;
            tripodState.BelowPlaneLimits[axisIndex] = belowPlaneLimit;
        }
    }

    private static void DrawHatchedAxis(Vector4 axis)
    {
        if (_style.HatchedAxisLineThickness <= 0.0f)
        {
            return;
        }

        for (int j = 1; j < 10; j++)
        {
            var baseSSpace2 = WorldToPos(axis * 0.05f * (float)(j * 2) * _screenFactor, _mvp);
            var worldDirSSpace2 = WorldToPos(axis * 0.05f * (float)(j * 2 + 1) * _screenFactor, _mvp);
            _drawList.AddLine(baseSSpace2, worldDirSSpace2, GetColourU32((int)ImGuizmoColour.HatchedAxisLines),
                _style.HatchedAxisLineThickness);
        }
    }

    private static float ComputeAngleOnPlan()
    {
        float len = Maths.IntersectRayPlane(_rayOrigin, _rayVector, _translationPlan);
        var localPos = Maths.Normalize(_rayOrigin + _rayVector * len - _model.Position);

        var perpendicularVector = Maths.Cross(_rotationVectorSource, _translationPlan);
        perpendicularVector = Maths.Normalize(perpendicularVector);
        float acosAngle = Maths.Clamp(Vector4.Dot(localPos, _rotationVectorSource), -1.0f, 1.0f);
        float angle = MathF.Acos(acosAngle);
        angle *= Vector4.Dot(localPos, perpendicularVector) < 0.0f ? 1.0f : -1.0f;
        return angle;
    }
    
    private static unsafe void DrawRotationGizmo(ImGuizmoOperation op, ImGuizmoMoveType type)
    {
        if ((op & ImGuizmoOperation.Rotate) == 0)
        {
            return;
        }
        
        var drawList = _drawList;

        bool isMultipleAxisMasked = (_axisMask & (_axisMask - 1)) != 0;
        bool isNoAxisMasked = _axisMask == 0;
        
        Span<uint> colours = stackalloc uint[7];
        ComputeColours(colours, type, ImGuizmoOperation.Rotate);

        Vector4 viewDirNormalized;
        Matrix4x4 viewInverse;
        if (_isOrthographic)
        {
            Matrix4x4.Invert(_viewMat, out viewInverse);
            viewDirNormalized = -viewInverse.Dir;
        }
        else
        {
            Matrix4x4.Invert(_viewMat, out viewInverse);
            float handSign = Vector4.Dot(Maths.Cross(viewInverse.Right, viewInverse.Up), viewInverse.Dir) >= 0.0f
                ? 1.0f
                : -1.0f;
            viewDirNormalized = Maths.Normalize(_cameraDir) * handSign;
        }

        viewDirNormalized = Maths.TransformVector(viewDirNormalized, _modelInverse);

        _radiusSquareCenter = ScreenRotateSize * _height;

        bool hasRSC = (op & ImGuizmoOperation.RotateScreen) != 0;
        var circlePos = stackalloc Vector2[2 * HalfCircleSegmentCount + 1];
        for (int axis = 0; axis < 3; axis++)
        {
            if ((op & (ImGuizmoOperation)((int)ImGuizmoOperation.RotateZ >> axis)) == 0)
            {
                continue;
            }

            bool isAxisMasked = ((1 << (2 - axis)) & _axisMask) != 0;

            if ((!isAxisMasked || isMultipleAxisMasked) && !isNoAxisMasked)
            {
                continue;
            }

            bool usingAxis = (_using && type == ImGuizmoMoveType.RotateZ - axis);
            int circleMul = (hasRSC && !usingAxis) ? 1 : 2;

            bool rightHanded = _projectionMat.M34 < 0.0;
            float angleStart = MathF.Atan2(viewDirNormalized[(4 - axis) % 3], viewDirNormalized[(3 - axis) % 3]) +
                               (_isOrthographic ? MathF.PI : -MathF.PI) * 0.5f + (rightHanded ? 0.0f : MathF.PI);

            for (int i = 0; i < circleMul * HalfCircleSegmentCount + 1; i++)
            {
                float ng = angleStart + circleMul * MathF.PI * ((float)i / (circleMul * HalfCircleSegmentCount));
                var axisPos = new Vector4(MathF.Cos(ng), MathF.Sin(ng), 0.0f, 0.0f);
                var pos = new Vector4(axisPos[axis], axisPos[(axis + 1) % 3], axisPos[(axis + 2) % 3], 0.0f) *
                          _screenFactor * RotationDisplayFactor;
                circlePos[i] = WorldToPos(pos, _mvp);
            }

            if (!_using || usingAxis)
            {
                drawList.AddPolyline(circlePos, circleMul * HalfCircleSegmentCount + 1, colours[3 - axis],
                    _style.RotationLineThickness, 0);
            }

            float radiusAxis = (WorldToPos(_model.Position, _viewProjection) - circlePos[0]).Length();
            if (radiusAxis > _radiusSquareCenter)
            {
                _radiusSquareCenter = radiusAxis;
            }
        }

        if (hasRSC && (!_using || type == ImGuizmoMoveType.RotateScreen) && (!isMultipleAxisMasked && isNoAxisMasked))
        {
            drawList.AddCircle(WorldToPos(_model.Position, _viewProjection), _radiusSquareCenter, colours[0], 64,
                _style.RotationOuterLineThickness);
        }

        if (_using && (CurrentId == _editingId) && IsRotateType(type))
        {
            circlePos[0] = WorldToPos(_model.Position, _viewProjection);
            for (int i = 1; i < HalfCircleSegmentCount + 1; i++)
            {
                float ng = _rotationAngle * ((float)(i - 1) / (HalfCircleSegmentCount - 1));
                var rotateVectorMatrix =
                    Matrix4x4.CreateFromAxisAngle(
                        new Vector3(_translationPlan.X, _translationPlan.Y, _translationPlan.Z), ng);
                var pos = Maths.TransformPoint(_rotationVectorSource, rotateVectorMatrix);
                pos *= _screenFactor * RotationDisplayFactor;
                circlePos[i] = WorldToPos(pos + _model.Position, _viewProjection);
            }

            drawList.AddConvexPolyFilled(circlePos, HalfCircleSegmentCount + 1,
                GetColourU32((int)ImGuizmoColour.RotationUsingFill));
            drawList.AddPolyline(circlePos, HalfCircleSegmentCount + 1,
                GetColourU32((int)ImGuizmoColour.RotationUsingBorder), _style.RotationLineThickness,
                ImDrawFlags.Closed);

            var destinationPosOnScreen = circlePos[1];
            string temps = string.Format(RotationInfoMask[type - ImGuizmoMoveType.RotateX],
                (_rotationAngle / MathF.PI) * 180.0f, _rotationAngle);
            drawList.AddText(new Vector2(destinationPosOnScreen.X + 15, destinationPosOnScreen.Y + 15),
                GetColourU32((int)ImGuizmoColour.TextShadow), temps);
            drawList.AddText(new Vector2(destinationPosOnScreen.X + 14, destinationPosOnScreen.Y + 14),
                GetColourU32((int)ImGuizmoColour.Text), temps);
        }
    }
    
    private static void DrawScaleGizmo(ImGuizmoOperation op, ImGuizmoMoveType type)
    {
        var drawList = _drawList;

        if ((op & ImGuizmoOperation.Scale) == 0)
        {
            return;
        }
        
        Span<uint> colours = stackalloc uint[7];
        ComputeColours(colours, type, ImGuizmoOperation.Scale);
        
        // draw
        Vector4 scaleDisplay = Vector4.One;

        if (_using && (CurrentId == _editingId))
        {
            scaleDisplay = _scale;
        }

        for (int i = 0; i < 3; i++)
        {
            if ((op & (ImGuizmoOperation)((int)ImGuizmoOperation.ScaleX << i)) == 0)
            {
                continue;
            }

            bool usingAxes = (_using && type == ImGuizmoMoveType.ScaleX + i);
            if (!_using || usingAxes)
            {
                ComputeTripodAxisAndVisibility(i, out var dirAxis, out var dirPlaneX, out var dirPlaneY,
                    out bool belowAxisLimit, out bool belowPlaneLimit, true);
                
                // draw axis
                if (belowAxisLimit)
                {
                    bool hasTranslateOnAxis = op.HasFlag((ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i));
                    float markerScale = hasTranslateOnAxis ? 1.4f : 1.0f;
                    var baseSSpace = WorldToPos(dirAxis * 0.1f * _screenFactor, _mvp);
                    var worldDirSSpaceNoScale = WorldToPos(dirAxis * markerScale * _screenFactor, _mvp);
                    var worldDirSSpace = WorldToPos((dirAxis * markerScale * scaleDisplay[i]) * _screenFactor, _mvp);

                    if (_using && (CurrentId == _editingId))
                    {
                        var scaleLineColour = GetColourU32((int)ImGuizmoColour.ScaleLine);
                        drawList.AddLine(baseSSpace, worldDirSSpaceNoScale, scaleLineColour, _style.ScaleLineThickness);
                        drawList.AddCircleFilled(worldDirSSpaceNoScale, _style.ScaleLineCircleSize, scaleLineColour);
                    }

                    if (!hasTranslateOnAxis || _using)
                    {
                        drawList.AddLine(baseSSpace, worldDirSSpace, colours[i + 1], _style.ScaleLineThickness);
                    }
                    drawList.AddCircleFilled(worldDirSSpace, _style.ScaleLineCircleSize, colours[i + 1]);

                    if (TripodState.AxisFactor[i] < 0.0f)
                    {
                        DrawHatchedAxis(dirAxis * scaleDisplay[i]);
                    }
                }
            }
        }
        
        // draw screen circle
        drawList.AddCircleFilled(_screenSquareCenter, _style.CenterCircleSize, colours[0], 32);

        if (_using && (CurrentId == _editingId) && IsScaleType(type))
        {
            var destinationPosOnScreen = WorldToPos(_model.Position, _viewProjection);

            int componentInfoIndex = (type - ImGuizmoMoveType.ScaleX) * 3;
            string temps = string.Format(ScaleInfoMask[type - ImGuizmoMoveType.ScaleX],
                scaleDisplay[TranslationInfoIndex[componentInfoIndex]]);
            drawList.AddText(new Vector2(destinationPosOnScreen.X + 15, destinationPosOnScreen.Y + 15),
                GetColourU32((int)ImGuizmoColour.TextShadow), temps);
            drawList.AddText(new Vector2(destinationPosOnScreen.X + 14, destinationPosOnScreen.Y + 14),
                GetColourU32((int)ImGuizmoColour.Text), temps);
        }
    }

    private static void DrawScaleUniversalGizmo(ImGuizmoOperation op, ImGuizmoMoveType type)
    {
        var drawList = _drawList;
        
        if ((op & ImGuizmoOperation.ScaleU) == 0)
        {
            return;
        }
        
        Span<uint> colours = stackalloc uint[7];
        ComputeColours(colours, type, ImGuizmoOperation.ScaleU);
        
        // draw
        Vector4 scaleDisplay = Vector4.One;

        if (_using && (CurrentId == _editingId))
        {
            scaleDisplay = _scale;
        }

        for (int i = 0; i < 3; i++)
        {
            if ((op & (ImGuizmoOperation)((int)ImGuizmoOperation.ScaleXu << i)) == 0)
            {
                continue;
            }

            bool usingAxes = (_using && type == ImGuizmoMoveType.ScaleX + i);
            if (!_using || usingAxes)
            {
                ComputeTripodAxisAndVisibility(i, out var dirAxis, out var dirPlaneX, out var dirPlaneY,
                    out bool belowAxisLimit, out bool belowPlaneLimit, true);
                
                // draw axis
                if (belowAxisLimit)
                {
                    bool hasTranslateOnAxis = op.HasFlag((ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i));
                    float markerScale = hasTranslateOnAxis ? 1.4f : 1.0f;
                    var worldDirSSpace = WorldToPos((dirAxis * markerScale * scaleDisplay[i]) * _screenFactor, _mvpLocal);

                    drawList.AddCircleFilled(worldDirSSpace, _style.ScaleLineCircleSize, colours[i + 1]);
                }
            }
        }
        
        // draw screen circle
        drawList.AddCircle(_screenSquareCenter, 20.0f, colours[0], 32, _style.CenterCircleSize);

        if (_using && (CurrentId == _editingId) && IsScaleType(type))
        {
            var destinationPosOnScreen = WorldToPos(_model.Position, _viewProjection);

            int componentInfoIndex = (type - ImGuizmoMoveType.ScaleX) * 3;
            string temps = string.Format(ScaleInfoMask[type - ImGuizmoMoveType.ScaleX],
                scaleDisplay[TranslationInfoIndex[componentInfoIndex]]);
            drawList.AddText(new Vector2(destinationPosOnScreen.X + 15, destinationPosOnScreen.Y + 15),
                GetColourU32((int)ImGuizmoColour.TextShadow), temps);
            drawList.AddText(new Vector2(destinationPosOnScreen.X + 14, destinationPosOnScreen.Y + 14),
                GetColourU32((int)ImGuizmoColour.Text), temps);
        }
    }
    
    private static unsafe void DrawTranslationGizmo(ImGuizmoOperation op, ImGuizmoMoveType type)
    {
        var drawList = _drawList;
        if (_drawList.IsNull)
        {
            return;
        }

        if ((op & ImGuizmoOperation.Translate) == 0)
        {
            return;
        }

        Span<uint> colours = stackalloc uint[7];
        ComputeColours(colours, type, ImGuizmoOperation.Translate);

        var origin = WorldToPos(_model.Position, _viewProjection);
        
        // draw
        Vector2* screenQuadsPts = stackalloc Vector2[4];
        for (int i = 0; i < 3; i++)
        {
            ComputeTripodAxisAndVisibility(i, out var dirAxis, out var dirPlaneX, out var dirPlaneY, out bool belowAxisLimit,
                out bool belowPlaneLimit);

            if (!_using || (_using && type == ImGuizmoMoveType.MoveX + i))
            {
                // draw axis
                if (belowAxisLimit && (op & (ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i)) != 0)
                {
                    var baseSSpace = WorldToPos(dirAxis * 0.1f * _screenFactor, _mvp);
                    var worldDirSSpace = WorldToPos(dirAxis * _screenFactor, _mvp);

                    drawList.AddLine(baseSSpace, worldDirSSpace, colours[i + 1], _style.TranslationLineThickness);
                    
                    // arrow head begin
                    var dir = origin - worldDirSSpace;

                    float d = dir.Length();
                    dir /= d;
                    dir *= _style.TranslationLineArrowSize;

                    var orthogonalDir = new Vector2(dir.Y, -dir.X);
                    var a = worldDirSSpace + dir;
                    drawList.AddTriangleFilled(worldDirSSpace - dir, a + orthogonalDir, a - orthogonalDir,
                        colours[i + 1]);
                    // arrow head end

                    if (TripodState.AxisFactor[i] < 0.0f)
                    {
                        DrawHatchedAxis(dirAxis);
                    }
                }
            }

            if (!_using || (_using && type == ImGuizmoMoveType.MoveYZ + i))
            {
                if (belowPlaneLimit && op.HasFlag(TranslatePlans[i]))
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var cornerWorldPos =
                            (dirPlaneX * QuadUv[j * 2] + dirPlaneY * QuadUv[j * 2 + 1]) * _screenFactor;
                        screenQuadsPts[j] = WorldToPos(cornerWorldPos, _mvp);
                    }

                    _drawList.AddPolyline(screenQuadsPts, 4, GetColourU32((int)ImGuizmoColour.DirectionX + i), 1.0f,
                        ImDrawFlags.Closed);
                    _drawList.AddConvexPolyFilled(screenQuadsPts, 4, colours[i + 4]);
                }
            }
        }

        drawList.AddCircleFilled(_screenSquareCenter, _style.CenterCircleSize, colours[0], 32);

        if (_using && (CurrentId == _editingId) && IsTranslateType(type))
        {
            uint translationLineColour = GetColourU32((int)ImGuizmoColour.TranslationLine);

            var sourcePosOnScreen = WorldToPos(_matrixOrigin, _viewProjection);
            var destinationPosOnScreen = WorldToPos(_model.Position, _viewProjection);
            drawList.AddCircle(sourcePosOnScreen, 6.0f, translationLineColour);
            drawList.AddCircle(destinationPosOnScreen, 6.0f, translationLineColour);
            drawList.AddLine(sourcePosOnScreen, destinationPosOnScreen, translationLineColour, 2.0f);

            var deltaInfo = _model.Position - _matrixOrigin;
            int componentInfoIndex = (type - ImGuizmoMoveType.MoveX) * 3;
            string temps = string.Format(TranslationInfoMask[type - ImGuizmoMoveType.MoveX],
                deltaInfo[TranslationInfoIndex[componentInfoIndex]],
                deltaInfo[TranslationInfoIndex[componentInfoIndex + 1]],
                deltaInfo[TranslationInfoIndex[componentInfoIndex + 2]]);
            drawList.AddText(new Vector2(destinationPosOnScreen.X + 15, destinationPosOnScreen.Y + 15),
                GetColourU32((int)ImGuizmoColour.TextShadow), temps);
            drawList.AddText(new Vector2(destinationPosOnScreen.X + 14, destinationPosOnScreen.Y + 14),
                GetColourU32((int)ImGuizmoColour.Text), temps);
        }
    }

    private static bool CanActivate
    {
        get
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ImGui.IsAnyItemHovered() && !ImGui.IsAnyItemActive())
            {
                return true;
            }

            return false;
        }
    }

    private static ImGuizmoMoveType GetRotateType(ImGuizmoOperation op)
    {
        if (_using)
        {
            return ImGuizmoMoveType.None;
        }

        bool isNoAxisMasked = _axisMask == 0;
        bool isMultipleAxisMasked = (_axisMask & (_axisMask - 1)) != 0;
        
        var io = ImGui.GetIO();
        var type = ImGuizmoMoveType.None;

        var deltaScreen = new Vector4(io.MousePos.X - _screenSquareCenter.X, io.MousePos.Y - _screenSquareCenter.Y,
            0.0f, 0.0f);
        float dist = deltaScreen.CLength();
        if (((op & ImGuizmoOperation.RotateScreen) == 0) && dist >= (_radiusSquareCenter - 4.0f) &&
            dist < (_radiusSquareCenter + 4.0f))
        {
            if (!isNoAxisMasked)
                return ImGuizmoMoveType.None;
            type = ImGuizmoMoveType.RotateScreen;
        }

        Span<Vector4> planNormals =
        [
            _model.Right, _model.Up, _model.Dir
        ];

        var modelViewPos = Maths.TransformPoint(_model.Position, _viewMat);

        for (int i = 0; i < 3 && type == ImGuizmoMoveType.None; i++)
        {
            if ((op & (ImGuizmoOperation)((int)ImGuizmoOperation.RotateX << i)) == 0)
            {
                continue;
            }

            bool isAxisMasked = ((1 << i) & _axisMask) != 0;
            var pickupPlan = Maths.BuildPlan(_model.Position, planNormals[i]);

            float len = Maths.IntersectRayPlane(_rayOrigin, _rayVector, pickupPlan);
            var intersectWorldPos = _rayOrigin + _rayVector * len;
            var intersectViewPos = Maths.TransformPoint(intersectWorldPos, _viewMat);

            if (MathF.Abs(modelViewPos.Z) - MathF.Abs(intersectViewPos.Z) < -float.Epsilon)
            {
                continue;
            }

            var localPos = intersectWorldPos - _model.Position;
            var idealPosOnCircle = Maths.Normalize(localPos);
            idealPosOnCircle = Maths.TransformVector(idealPosOnCircle, _modelInverse);
            var idealPosOnCircleScreen = WorldToPos(idealPosOnCircle * RotationDisplayFactor * _screenFactor, _mvp);

            var distanceOnScreen = idealPosOnCircleScreen - io.MousePos;

            float distance = new Vector4(distanceOnScreen.X, distanceOnScreen.Y, 0.0f, 0.0f).CLength();
            if (distance < 8.0f)
            {
                if ((!isAxisMasked || isMultipleAxisMasked) && !isNoAxisMasked)
                    break;
                type = ImGuizmoMoveType.RotateX + i;
            }
        }

        return type;
    }

    private static ImGuizmoMoveType GetScaleType(ImGuizmoOperation op)
    {
        if (_using)
        {
            return ImGuizmoMoveType.None;
        }

        var io = ImGui.GetIO();
        var type = ImGuizmoMoveType.None;
        
        // screen
        if (io.MousePos.X >= _screenSquareMin.X && io.MousePos.X <= _screenSquareMax.X &&
            io.MousePos.Y >= _screenSquareMin.Y && io.MousePos.Y <= _screenSquareMax.Y &&
            op.HasFlag(ImGuizmoOperation.Scale))
        {
            type = ImGuizmoMoveType.ScaleXYZ;
        }
        
        // compute
        for (int i = 0; i < 3 && type == ImGuizmoMoveType.None; i++)
        {
            if ((op & (ImGuizmoOperation)((int)ImGuizmoOperation.ScaleX << i)) == 0)
            {
                continue;
            }

            bool isAxisMasked = ((1 << i) & _axisMask) != 0;

            ComputeTripodAxisAndVisibility(i, out var dirAxis, out var dirPlaneX, out var dirPlaneY,
                out var belowAxisLimit, out var belowPlaneLimit, true);
            dirAxis = Maths.TransformVector(dirAxis, _modelLocal);
            dirPlaneX = Maths.TransformVector(dirPlaneX, _modelLocal);
            dirPlaneY = Maths.TransformVector(dirPlaneY, _modelLocal);

            float len = Maths.IntersectRayPlane(_rayOrigin, _rayVector, Maths.BuildPlan(_modelLocal.Position, dirAxis));
            var posOnPlan = _rayOrigin + _rayVector * len;

            float startOffset = op.HasFlag((ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i)) ? 1.0f : 0.1f;
            float endOffset = op.HasFlag((ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i)) ? 1.4f : 1.0f;
            var posOnPlanScreen = WorldToPos(posOnPlan, _viewProjection);
            var axisStartOnScreen =
                WorldToPos(_modelLocal.Position + dirAxis * _screenFactor * startOffset, _viewProjection);
            var axisEndOnScreen =
                WorldToPos(_modelLocal.Position + dirAxis * _screenFactor * endOffset, _viewProjection);

            var closestPointOnAxis = Maths.PointOnSegment(new Vector4(posOnPlanScreen, 0.0f, 0.0f),
                new Vector4(axisStartOnScreen, 0.0f, 0.0f), new Vector4(axisEndOnScreen, 0.0f, 0.0f));

            if ((closestPointOnAxis - new Vector4(posOnPlanScreen, 0.0f, 0.0f)).CLength() < 12.0f)
            {
                if (!isAxisMasked)
                    type = ImGuizmoMoveType.ScaleX + i;
            }
        }
        
        // universal

        var deltaScreen = new Vector4(io.MousePos.X - _screenSquareCenter.X, io.MousePos.Y - _screenSquareCenter.Y,
            0.0f, 0.0f);
        float dist = deltaScreen.CLength();
        if (((op & ImGuizmoOperation.ScaleU) != 0) && dist is >= 17.0f and <= 23.0f)
        {
            type = ImGuizmoMoveType.ScaleXYZ;
        }
        
        for (int i = 0; i < 3 && type == ImGuizmoMoveType.None; i++)
        {
            if ((op & (ImGuizmoOperation)((int)ImGuizmoOperation.ScaleXu << i)) == 0)
            {
                continue;
            }

            ComputeTripodAxisAndVisibility(i, out var dirAxis, out var dirPlaneX, out var dirPlaneY,
                out var belowAxisLimit, out var belowPlaneLimit, true);

            if (belowAxisLimit)
            {
                bool hasTranslateOnAxis = op.HasFlag((ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i));
                float markerScale = hasTranslateOnAxis ? 1.4f : 1.0f;

                var worldDirSpace = WorldToPos((dirAxis * markerScale) * _screenFactor, _mvpLocal);

                float distance = (worldDirSpace - io.MousePos).Length();
                if (distance < 12.0f)
                {
                    type = ImGuizmoMoveType.ScaleX + i;
                }
            }
        }

        return type;
    }
    
    private static ImGuizmoMoveType GetMoveType(ImGuizmoOperation op, out Vector4 gizmoHitProportion)
    {
        gizmoHitProportion = Vector4.Zero;
        
        if ((op & ImGuizmoOperation.Translate) == 0 || _using || !_mouseOver)
        {
            return ImGuizmoMoveType.None;
        }

        bool isNoAxesMasked = _axisMask == 0;
        bool isMultipleAxesMasked = (_axisMask & (_axisMask - 1)) != 0;
        
        var io = ImGui.GetIO();
        var type = ImGuizmoMoveType.None;
        
        // screen
        if (io.MousePos.X >= _screenSquareMin.X && io.MousePos.X <= _screenSquareMax.X &&
            io.MousePos.Y >= _screenSquareMin.Y && io.MousePos.Y <= _screenSquareMax.Y)
        {
            type = ImGuizmoMoveType.MoveScreen;
        }

        var screenCoord = new Vector4(io.MousePos - new Vector2(_x, _y), 0.0f, 0.0f);
        
        // compute
        for (int i = 0; i < 3 && type == ImGuizmoMoveType.None; i++)
        {
            bool isAxisMasked = ((1 << i) & _axisMask) != 0;
            ComputeTripodAxisAndVisibility(i, out var dirAxis, out var dirPlaneX, out var dirPlaneY,
                out var belowAxisLimit, out var belowPlaneLimit);
            dirAxis = Maths.TransformVector(dirAxis, _model);
            dirPlaneX = Maths.TransformVector(dirPlaneX, _model);
            dirPlaneY = Maths.TransformVector(dirPlaneY, _model);

            float len = Maths.IntersectRayPlane(_rayOrigin, _rayVector, Maths.BuildPlan(_model.Position, dirAxis));
            var posOnPlan = _rayOrigin + _rayVector * len;

            var axisStartOnScreen = WorldToPos(_model.Position + dirAxis * _screenFactor * 0.1f, _viewProjection) -
                                    new Vector2(_x, _y);
            var axisEndOnScreen = WorldToPos(_model.Position + dirAxis * _screenFactor, _viewProjection) -
                                  new Vector2(_x, _y);

            var closestPointOnAxis = Maths.PointOnSegment(screenCoord, new Vector4(axisStartOnScreen, 0.0f, 0.0f),
                new Vector4(axisEndOnScreen, 0.0f, 0.0f));
            if ((closestPointOnAxis - screenCoord).CLength() < 12.0f &&
                (op & (ImGuizmoOperation)((int)ImGuizmoOperation.TranslateX << i)) != 0)
            {
                if (isAxisMasked)
                    break;
                type = ImGuizmoMoveType.MoveX + i;
            }

            float dx = Maths.Dot3(dirPlaneX, posOnPlan - _model.Position) * (1.0f / _screenFactor);
            float dy = Maths.Dot3(dirPlaneY, posOnPlan - _model.Position) * (1.0f / _screenFactor);
            if (belowPlaneLimit && dx >= QuadUv[0] && dx <= QuadUv[4] && dy >= QuadUv[1] && dy <= QuadUv[3] &&
                op.HasFlag(TranslatePlans[i]))
            {
                if ((!isAxisMasked || isMultipleAxesMasked) && !isNoAxesMasked)
                    break;
                type = ImGuizmoMoveType.MoveYZ + i;
            }

            gizmoHitProportion = new Vector4(dx, dy, 0.0f, 0.0f);
        }

        return type;
    }
    
    private static bool HandleTranslation(ref Matrix4x4 matrix, ref Matrix4x4 deltaMatrix, ImGuizmoOperation op,
        ref ImGuizmoMoveType type)
    {
        if (((op & ImGuizmoOperation.Translate) == 0) || type != ImGuizmoMoveType.None)
        {
            return false;
        }

        var io = ImGui.GetIO();
        bool applyRotationLocally = _mode == ImGuizmoMode.Local || _currentHandleType == ImGuizmoMoveType.MoveScreen;
        bool modified = false;
        
        // move
        if (_using && (CurrentId == _editingId) && IsTranslateType(_currentHandleType))
        {
            ImGui.SetNextFrameWantCaptureMouse(true);

            float signedLength = Maths.IntersectRayPlane(_rayOrigin, _rayVector, _translationPlan);
            float len = MathF.Abs(signedLength);
            var newPos = _rayOrigin + _rayVector * len;
            
            // compute delta
            var newOrigin = newPos - _relativeOrigin * _screenFactor;
            var delta = newOrigin - _model.Position;
            
            // 1 axis constraint
            if (_currentHandleType is >= ImGuizmoMoveType.MoveX and <= ImGuizmoMoveType.MoveZ)
            {
                int axisIndex = _currentHandleType - ImGuizmoMoveType.MoveX;
                var axisValue = _model.GetRow(axisIndex);
                float lengthOnAxis = Vector4.Dot(axisValue, delta);
                delta = axisValue * lengthOnAxis;
            }
            
            // snap
            // TODO: Snapping

            if (delta != _translationLastDelta)
            {
                modified = true;
            }

            _translationLastDelta = delta;
            
            // compute matrix and delta
            var deltaMatrixTranslation = Matrix4x4.CreateTranslation(delta.X, delta.Y, delta.Z);
            deltaMatrix = deltaMatrixTranslation;

            var res = _modelSource * deltaMatrixTranslation;
            matrix = res;

            if (!io.MouseDown[0])
            {
                _using = false;
            }

            type = _currentHandleType;
        }
        else
        {
            type = _overGizmoHotspot ? ImGuizmoMoveType.None : GetMoveType(op, out var gizmoHitProportion);
            _overGizmoHotspot |= type != ImGuizmoMoveType.None;
            if (type != ImGuizmoMoveType.None)
            {
                ImGui.SetNextFrameWantCaptureMouse(true);
            }

            if (CanActivate && type != ImGuizmoMoveType.None)
            {
                _tripodState.CopyTo(_activeTripodState);
                _using = true;
                _editingId = CurrentId;
                _currentHandleType = type;
                Span<Vector4> movePlanNormal =
                [
                    _model.Right, _model.Up, _model.Dir, _model.Right, _model.Up, _model.Dir, -_cameraDir
                ];

                var cameraToModelNormalized = Maths.Normalize(_model.Position - _cameraEye);
                for (int i = 0; i < 3; i++)
                {
                    var orthoVector = Maths.Cross(movePlanNormal[i], cameraToModelNormalized);
                    movePlanNormal[i] = Maths.Cross(movePlanNormal[i], orthoVector);
                    movePlanNormal[i] = Maths.Normalize(movePlanNormal[i]);
                }
                
                // pickup plan
                _translationPlan = Maths.BuildPlan(_model.Position, movePlanNormal[type - ImGuizmoMoveType.MoveX]);
                float len = Maths.IntersectRayPlane(_rayOrigin, _rayVector, _translationPlan);
                _translationPlanOrigin = _rayOrigin + _rayVector * len;
                _matrixOrigin = _model.Position;

                _relativeOrigin = (_translationPlanOrigin - _model.Position) * (1.0f / _screenFactor);
            }
        }

        return modified;
    }

    private static bool HandleScale(ref Matrix4x4 matrix, ref Matrix4x4 deltaMatrix, ImGuizmoOperation op,
        ref ImGuizmoMoveType type)
    {
        if ((((op & (ImGuizmoOperation)((int)ImGuizmoOperation.Scale)) == 0) &&
             ((op & (ImGuizmoOperation)((int)ImGuizmoOperation.ScaleU)) == 0)) ||
            type != ImGuizmoMoveType.None || !_mouseOver)
        {
            return false;
        }
        
        var io = ImGui.GetIO();
        bool modified = false;

        if (!_using)
        {
            // find new possible way to scale
            type = _overGizmoHotspot ? ImGuizmoMoveType.None : GetScaleType(op);
            _overGizmoHotspot |= type != ImGuizmoMoveType.None;

            if (type != ImGuizmoMoveType.None)
            {
                ImGui.SetNextFrameWantCaptureMouse(true);
            }

            if (CanActivate && type != ImGuizmoMoveType.None)
            {
                _tripodState.CopyTo(_activeTripodState);
                _using = true;
                _editingId = CurrentId;
                _currentHandleType = type;

                Span<Vector4> movePlanNormal =
                [
                    _modelLocal.Up, _modelLocal.Dir, _modelLocal.Right, _modelLocal.Dir, _modelLocal.Up, _modelLocal.Right, -_cameraDir
                ];


                _translationPlan =
                    Maths.BuildPlan(_modelLocal.Position, movePlanNormal[type - ImGuizmoMoveType.ScaleX]);
                float len = Maths.IntersectRayPlane(_rayOrigin, _rayVector, _translationPlan);
                _translationPlanOrigin = _rayOrigin + _rayVector * len;
                _matrixOrigin = _modelLocal.Position;
                _scale = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
                _relativeOrigin = (_translationPlanOrigin - _modelLocal.Position) * (1.0f / _screenFactor);
                _scaleValueOrigin = new Vector4(_modelSource.Right.CLength(), _modelSource.Up.CLength(),
                    _modelSource.Dir.CLength(), 0.0f);
                _saveMousePosX = io.MousePos.X;
            }
        }
        
        // scale
        if (_using && (CurrentId == _editingId) && IsScaleType(_currentHandleType))
        {
            ImGui.SetNextFrameWantCaptureMouse(true);

            float len = Maths.IntersectRayPlane(_rayOrigin, _rayVector, _translationPlan);
            var newPos = _rayOrigin + _rayVector * len;
            var newOrigin = newPos - _relativeOrigin * _screenFactor;
            var delta = newOrigin - _modelLocal.Position;
            
            // 1 axis constraint
            if (_currentHandleType is >= ImGuizmoMoveType.ScaleX and <= ImGuizmoMoveType.ScaleZ)
            {
                int axisIndex = _currentHandleType - ImGuizmoMoveType.ScaleX;
                var axisValue = _modelLocal.GetRow(axisIndex);
                float lengthOnAxis = Vector4.Dot(axisValue, delta);
                delta = axisValue * lengthOnAxis;

                var baseVector = _translationPlanOrigin - _modelLocal.Position;
                float ratio = Vector4.Dot(axisValue, baseVector + delta) / Vector4.Dot(axisValue, baseVector);

                _scale[axisIndex] = MathF.Max(ratio, 0.001f);
            }
            else
            {
                float scaleDelta = (io.MousePos.X - _saveMousePosX) * 0.01f;
                _scale = new Vector4(MathF.Max(1.0f + scaleDelta, 0.001f));
            }
            
            // snap
            // TODO: Snapping
            
            // no 0 allowed
            for (int i = 0; i < 3; i++)
            {
                _scale[i] = MathF.Max(_scale[i], 0.001f);
            }

            if (_scaleLast != _scale)
            {
                modified = true;
            }

            _scaleLast = _scale;

            var deltaMatrixScale = Matrix4x4.CreateScale(new Vector3(
                _scale.X * _scaleValueOrigin.X,
                _scale.Y * _scaleValueOrigin.Y,
                _scale.Z * _scaleValueOrigin.Z
            ));

            var res = deltaMatrixScale * _modelLocal;
            matrix = res;
            
            // delta matrix
            var deltaScale = _scale * _scaleValueOrigin;

            var originalScaleDivider = new Vector4(
                1.0f / _modelScaleOrigin.X,
                1.0f / _modelScaleOrigin.Y,
                1.0f / _modelScaleOrigin.Z,
                0.0f
            );

            deltaScale = deltaScale * originalScaleDivider;

            deltaMatrixScale = Matrix4x4.CreateScale(new Vector3(deltaScale.X, deltaScale.Y, deltaScale.Z));
            deltaMatrix = deltaMatrixScale;

            if (!io.MouseDown[0])
            {
                _using = false;
                _scale = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
            }

            type = _currentHandleType;
        }

        return modified;
    }
    
    private static bool HandleRotation(ref Matrix4x4 matrix, ref Matrix4x4 deltaMatrix, ImGuizmoOperation op,
        ref ImGuizmoMoveType type)
    {
        if (((op & ImGuizmoOperation.Rotate) == 0)
            || type != ImGuizmoMoveType.None || !_mouseOver)
        {
            return false;
        }
        
        var io = ImGui.GetIO();
        bool applyRotationLocally = _mode == ImGuizmoMode.Local;
        bool modified = false;

        if (!_using)
        {
            // find new possible way to scale
            type = _overGizmoHotspot ? ImGuizmoMoveType.None : GetRotateType(op);
            _overGizmoHotspot |= type != ImGuizmoMoveType.None;

            if (type != ImGuizmoMoveType.None)
            {
                ImGui.SetNextFrameWantCaptureMouse(true);
            }

            if (CanActivate && type != ImGuizmoMoveType.None)
            {
                _using = true;
                _editingId = CurrentId;
                _currentHandleType = type;

                Span<Vector4> rotatePlaneNormal =
                [
                    _model.Right, _model.Up, _model.Dir, -_cameraDir
                ];

                if (applyRotationLocally)
                {
                    _translationPlan =
                        Maths.BuildPlan(_model.Position, rotatePlaneNormal[type - ImGuizmoMoveType.RotateX]);
                }
                else
                {
                    _translationPlan = Maths.BuildPlan(_modelSource.Position,
                        DirectionUnary[type - ImGuizmoMoveType.RotateX]);
                }

                float len = Maths.IntersectRayPlane(_rayOrigin, _rayVector, _translationPlan);
                var localPos = _rayOrigin + _rayVector * len - _model.Position;
                _rotationVectorSource = Maths.Normalize(localPos);
                _rotationAngleOrigin = ComputeAngleOnPlan();
            }
        }
        
        // rotation
        if (_using && (CurrentId == _editingId) && IsRotateType(_currentHandleType))
        {
            ImGui.SetNextFrameWantCaptureMouse(true);

            _rotationAngle = ComputeAngleOnPlan();
            
            // TODO: Snapping

            var rotationAxisLocalSpace =
                Maths.TransformVector(_translationPlan with { W = 0.0f }, _modelInverse);
            rotationAxisLocalSpace = Maths.Normalize(rotationAxisLocalSpace);

            var deltaRotation = Matrix4x4.CreateFromAxisAngle(
                new Vector3(rotationAxisLocalSpace.X, rotationAxisLocalSpace.Y, rotationAxisLocalSpace.Z),
                _rotationAngle - _rotationAngleOrigin);
            if (Math.Abs(_rotationAngle - _rotationAngleOrigin) > float.Epsilon)
            {
                modified = true;
            }

            _rotationAngleOrigin = _rotationAngle;
            
            var scaleOrigin = Matrix4x4.CreateScale(_modelScaleOrigin.X, _modelScaleOrigin.Y, _modelScaleOrigin.Z);

            if (applyRotationLocally)
            {
                matrix = scaleOrigin * deltaRotation * _modelLocal;
            }
            else
            {
                var res = _modelSource;
                res.W = Vector4.Zero;

                matrix = res * deltaRotation;
                matrix.W = _modelSource.Position;
            }

            deltaMatrix = _modelInverse * deltaRotation * _model;

            if (!io.MouseDown[0])
            {
                _using = false;
                _editingId = uint.MaxValue;
            }

            type = _currentHandleType;
        }

        return modified;
    }
    
    private static ImGuiWindowPtr FindWindowById(uint id)
    {
        var g = ImGui.GetCurrentContext();
        for (int i = 0; i < g.Windows.Size; i++)
        {
            var window = g.Windows[i];
            if (window.ID == id)
                return window;
        }

        return null;
    }
    
    private static bool IsHoveringWindow
    {
        get
        {
            var g = ImGui.GetCurrentContext();
            var window = FindWindowById(_windowId);
            if (g.HoveredWindow == window)
                return true;
            if (!_alternativeWindow.IsNull && g.HoveredWindow == _alternativeWindow)
                return true;
            if (!g.HoveredWindow.IsNull)
                return false;
            if (ImGui.IsMouseHoveringRect(window.InnerRect.Min, window.InnerRect.Max, false))
                return true;
            return false;
        }
    }
    
    public static void SetRect(float x, float y, float width, float height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _xMax = _x + _width;
        _yMax = _y + _height;
        _displayRatio = width / height;
    }

    public static void SetOrthographic(bool isOrthographic)
    {
        _isOrthographic = isOrthographic;
    }

    public static bool Manipulate(Matrix4x4 view, Matrix4x4 projection, ImGuizmoOperation operation, ImGuizmoMode mode,
        ref Matrix4x4 matrix, out Matrix4x4 deltaMatrix)
    {
        Vector3 s;
        _drawList.PushClipRect(new Vector2(_x, _y), new Vector2(_x + _width, _y + _height), false);
        
        Matrix4x4.Decompose(matrix, out s, out _, out _);
        if (MathF.Abs(s.X - 1.0f) < 0.01f)
        {
            Console.WriteLine("Before Compute context");
        }
        
        ComputeContext(view, projection, ref matrix, (operation & ImGuizmoOperation.Scale) != 0 ? ImGuizmoMode.Local : mode);
        
        Matrix4x4.Decompose(matrix, out s, out _, out _);
        if (MathF.Abs(s.X - 1.0f) < 0.01f)
        {
            Console.WriteLine("After Compute context");
        }
        
        deltaMatrix = Matrix4x4.Identity;
        
        // behind camera
        var camSpacePosition = Maths.TransformPoint(Vector4.Zero, _mvp);
        if (!_isOrthographic && camSpacePosition.Z < 0.001f && !_using)
        {
            return false;
        }

        var type = ImGuizmoMoveType.None;
        bool manipulated = false;
        if (_enable)
        {
            if (!_usingBounds)
            {
                Matrix4x4.Decompose(matrix, out s, out _, out _);
                if (MathF.Abs(s.X - 1.0f) < 0.01f)
                {
                    Console.WriteLine("Before Handle");
                }
                
                manipulated = HandleTranslation(ref matrix, ref deltaMatrix, operation, ref type);
                
                
                Matrix4x4.Decompose(matrix, out s, out _, out _);
                if (MathF.Abs(s.X - 1.0f) < 0.01f)
                {
                    Console.WriteLine("After Handle Translation");
                }
                
                manipulated |= HandleScale(ref matrix, ref deltaMatrix, operation, ref type);
                
                
                Matrix4x4.Decompose(matrix, out s, out _, out _);
                if (MathF.Abs(s.X - 1.0f) < 0.01f)
                {
                    Console.WriteLine("After Handle Scale");
                }
                
                manipulated |= HandleRotation(ref matrix, ref deltaMatrix, operation, ref type);
                
                Matrix4x4.Decompose(matrix, out s, out _, out _);
                if (MathF.Abs(s.X - 1.0f) < 0.01f)
                {
                    Console.WriteLine("After Handle Rotate");
                }
            }
        }

        Matrix4x4.Decompose(matrix, out s, out _, out _);
        if (MathF.Abs(s.X - 1.0f) < 0.01f)
        {
            Console.WriteLine("After Handle");
        }
        
        // TODO: Local bounds
        
        _operation = operation;
        _hoveredHandleType = (!_using && !_usingBounds) ? type : ImGuizmoMoveType.None;
        if (!_usingBounds)
        {
            DrawRotationGizmo(operation, type);
            DrawTranslationGizmo(operation, type);
            DrawScaleGizmo(operation, type);
            DrawScaleUniversalGizmo(operation, type);
        }
        
        _drawList.PopClipRect();
        return manipulated;
    }
    
}