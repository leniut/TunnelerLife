namespace TunnelerLife;

/// <summary>
/// Snapshot of a room used by passive thermal equalization.
/// </summary>
public readonly struct ThermalRoomState
{
    public ThermalRoomState(float temperature, int cellCount, bool usesOutdoorTemperature)
    {
        Temperature = temperature;
        CellCount = cellCount < 1 ? 1 : cellCount;
        UsesOutdoorTemperature = usesOutdoorTemperature;
    }

    public float Temperature { get; }

    public int CellCount { get; }

    public bool UsesOutdoorTemperature { get; }
}
