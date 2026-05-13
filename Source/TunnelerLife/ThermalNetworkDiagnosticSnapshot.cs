using System.Collections.Generic;

namespace TunnelerLife;

/// <summary>
/// On-demand diagnostic summary for one reachable thermal pipe network.
/// </summary>
internal readonly struct ThermalNetworkDiagnosticSnapshot
{
    public ThermalNetworkDiagnosticSnapshot(float? networkTemperature, int connectedRoomCount)
        : this(
            networkTemperature,
            networkCellCount: 0,
            rooms: [],
            ventCount: 0,
            inputVentCount: 0,
            outputVentCount: 0,
            blockers: [])
    {
        ConnectedRoomCount = connectedRoomCount;
    }

    public ThermalNetworkDiagnosticSnapshot(
        float? networkTemperature,
        int networkCellCount,
        IReadOnlyList<ThermalNetworkRoomDiagnostic> rooms,
        int ventCount,
        int inputVentCount,
        int outputVentCount,
        IReadOnlyList<ThermalNetworkBlockerDiagnostic> blockers)
    {
        NetworkTemperature = networkTemperature;
        NetworkCellCount = networkCellCount;
        Rooms = rooms;
        ConnectedRoomCount = rooms.Count;
        VentCount = ventCount;
        InputVentCount = inputVentCount;
        OutputVentCount = outputVentCount;
        Blockers = blockers;
    }

    public float? NetworkTemperature { get; }

    public int ConnectedRoomCount { get; }

    public int NetworkCellCount { get; }

    public IReadOnlyList<ThermalNetworkRoomDiagnostic> Rooms { get; }

    public int VentCount { get; }

    public int InputVentCount { get; }

    public int OutputVentCount { get; }

    public IReadOnlyList<ThermalNetworkBlockerDiagnostic> Blockers { get; }

    public bool HasInputAndOutput => InputVentCount > 0 && OutputVentCount > 0;
}

internal readonly struct ThermalNetworkRoomDiagnostic(
    string label,
    float temperature,
    int inputVentCount,
    int outputVentCount)
{
    public string Label { get; } = label;

    public float Temperature { get; } = temperature;

    public int InputVentCount { get; } = inputVentCount;

    public int OutputVentCount { get; } = outputVentCount;
}

internal readonly struct ThermalNetworkBlockerDiagnostic(string label, string status)
{
    public string Label { get; } = label;

    public string Status { get; } = status;
}
