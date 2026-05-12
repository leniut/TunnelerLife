using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TunnelerLife;

/// <summary>
/// On-demand thermal network diagnostics for inspect panes.
/// </summary>
internal static class ThermalNetworkDiagnostics
{
    public static ThermalNetworkDiagnosticSnapshot InspectPipe(Building_ThermalPipe pipe)
    {
        if (!pipe.Spawned)
        {
            return new ThermalNetworkDiagnosticSnapshot(null, 0);
        }

        Map map = pipe.Map;
        IEnumerable<IntVec3> connectedCells = ThermalPipeNetworkTraversal.FindConnectedCells(
            [pipe.Position],
            cell => cell.InBounds(map) && ThermalPipeUtility.HasOpenThermalNetworkCellAt(cell, map),
            (fromCell, toCell) => ThermalPipeUtility.CanTraverseThermalNetworkEdge(fromCell, toCell, map));
        IReadOnlyList<ThermalNetworkRoomPort> roomPorts = ThermalNetworkRoomScanner.FindRoomPorts(map, connectedCells);
        IReadOnlyList<ThermalNetworkRoomPort> sourceRoomPorts = ThermalNetworkRoomScanner.FindRoomPorts(
            map,
            connectedCells,
            ThermalVentFlowMode.PullFromAirSide);

        return new ThermalNetworkDiagnosticSnapshot(
            AverageNetworkTemperature(sourceRoomPorts.Select(port => port.Temperature)),
            roomPorts.Count);
    }

    public static float? AverageNetworkTemperature(IEnumerable<float> temperatures)
    {
        int count = 0;
        float total = 0f;
        foreach (float temperature in temperatures)
        {
            count++;
            total += temperature;
        }

        return count == 0 ? null : total / count;
    }
}
