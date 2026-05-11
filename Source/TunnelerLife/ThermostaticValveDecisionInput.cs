namespace TunnelerLife;

/// <summary>
/// Immutable temperature snapshot used to decide whether a thermostatic valve should open.
/// </summary>
public readonly struct ThermostaticValveDecisionInput
{
    public ThermostaticValveDecisionInput(
        ThermostaticValveMode mode,
        float targetTemperature,
        float controlledTemperature,
        float? sourceTemperature,
        bool hasPower,
        bool previousIsOpen)
    {
        Mode = mode;
        TargetTemperature = targetTemperature;
        ControlledTemperature = controlledTemperature;
        SourceTemperature = sourceTemperature;
        HasPower = hasPower;
        PreviousIsOpen = previousIsOpen;
    }

    public ThermostaticValveMode Mode { get; }

    public float TargetTemperature { get; }

    public float ControlledTemperature { get; }

    public float? SourceTemperature { get; }

    public bool HasPower { get; }

    public bool PreviousIsOpen { get; }
}
