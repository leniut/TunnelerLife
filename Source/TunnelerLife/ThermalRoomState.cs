namespace TunnelerLife;

/// <summary>
/// Snapshot of a room used by passive thermal equalization.
/// </summary>
public readonly struct ThermalRoomState
{
    public ThermalRoomState(float temperature, int cellCount, bool usesOutdoorTemperature, int exchangePortCount = 1)
    {
        Temperature = temperature;
        CellCount = cellCount < 1 ? 1 : cellCount;
        UsesOutdoorTemperature = usesOutdoorTemperature;
        ExchangePortCount = exchangePortCount < 1 ? 1 : exchangePortCount;
    }

    public float Temperature { get; }

    public int CellCount { get; }

    public bool UsesOutdoorTemperature { get; }

    public int ExchangePortCount { get; }
}
