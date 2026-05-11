namespace TunnelerLife;

/// <summary>
/// Describes the last automatic decision made by a thermostatic thermal valve.
/// </summary>
public enum ThermostaticValveStatus
{
    Closed,
    Open,
    BlockedNoPower,
    BlockedNoUsefulSource
}
