namespace TunnelerLife;

/// <summary>
/// Temperature snapshot from both sides of a thermostatic valve.
/// </summary>
internal readonly struct ThermalNetworkSideTemperatures
{
    public ThermalNetworkSideTemperatures(float? controlledTemperature, float? sourceTemperature)
    {
        ControlledTemperature = controlledTemperature;
        SourceTemperature = sourceTemperature;
    }

    public float? ControlledTemperature { get; }

    public float? SourceTemperature { get; }
}
