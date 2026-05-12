namespace TunnelerLife;

/// <summary>
/// Immutable temperature snapshot used to decide whether a thermostatic valve should open.
/// </summary>
public readonly struct ThermostaticValveDecisionInput
{
    public ThermostaticValveDecisionInput(
        float targetTemperature,
        float controlledTemperature,
        float? sourceTemperature,
        bool hasPower,
        float temperatureTolerance = TunnelerLifeSettings.DefaultThermostatTolerance)
    {
        TargetTemperature = targetTemperature;
        ControlledTemperature = controlledTemperature;
        SourceTemperature = sourceTemperature;
        HasPower = hasPower;
        TemperatureTolerance = temperatureTolerance;
    }

    public float TargetTemperature { get; }

    public float ControlledTemperature { get; }

    public float? SourceTemperature { get; }

    public bool HasPower { get; }

    public float TemperatureTolerance { get; }
}
