namespace ImGuizmoSharp.Utils;

internal record TripodState
{

    internal readonly bool[] BelowAxisLimits = new bool[3];
    internal readonly bool[] BelowPlaneLimits = new bool[3];
    internal readonly float[] AxisFactor = [1.0f, 1.0f, 1.0f];

    internal void CopyTo(TripodState other)
    {
        for (int i = 0; i < 3; i++)
        {
            other.BelowAxisLimits[i] = BelowAxisLimits[i];
            other.BelowPlaneLimits[i] = BelowPlaneLimits[i];
            other.AxisFactor[i] = AxisFactor[i];
        }
    }
    
}