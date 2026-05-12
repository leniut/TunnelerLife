using Verse;

namespace TunnelerLife;

/// <summary>
/// A reachable source network entered through one side of a thermostatic valve.
/// </summary>
internal readonly struct ThermalNetworkSourceCandidate
{
    public ThermalNetworkSourceCandidate(IntVec3 sideCell, float temperature)
    {
        SideCell = sideCell;
        Temperature = temperature;
    }

    public IntVec3 SideCell { get; }

    public float Temperature { get; }
}
