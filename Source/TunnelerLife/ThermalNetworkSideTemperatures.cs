using Verse;

namespace TunnelerLife;

/// <summary>
/// Temperature snapshot from both sides of a thermostatic valve.
/// </summary>
internal readonly struct ThermalNetworkSideTemperatures
{
    public ThermalNetworkSideTemperatures(
        float? controlledTemperature,
        float? sourceTemperature,
        IntVec3? sourceCell = null)
    {
        ControlledTemperature = controlledTemperature;
        SourceTemperature = sourceTemperature;
        SourceCell = sourceCell;
    }

    public float? ControlledTemperature { get; }

    public float? SourceTemperature { get; }

    public IntVec3? SourceCell { get; }
}
