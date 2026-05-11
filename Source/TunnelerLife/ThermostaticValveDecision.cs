namespace TunnelerLife;

/// <summary>
/// Result of evaluating a thermostatic valve against current room and source temperatures.
/// </summary>
public readonly struct ThermostaticValveDecision
{
    public ThermostaticValveDecision(bool isOpen, ThermostaticValveStatus status)
    {
        IsOpen = isOpen;
        Status = status;
    }

    public bool IsOpen { get; }

    public ThermostaticValveStatus Status { get; }
}
