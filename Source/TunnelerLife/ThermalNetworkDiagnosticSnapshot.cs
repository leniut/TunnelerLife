namespace TunnelerLife;

/// <summary>
/// On-demand diagnostic summary for one reachable thermal pipe network.
/// </summary>
internal readonly struct ThermalNetworkDiagnosticSnapshot
{
    public ThermalNetworkDiagnosticSnapshot(float? networkTemperature, int connectedRoomCount)
    {
        NetworkTemperature = networkTemperature;
        ConnectedRoomCount = connectedRoomCount;
    }

    public float? NetworkTemperature { get; }

    public int ConnectedRoomCount { get; }
}
