using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TunnelerLife;

/// <summary>
/// On-demand thermal network diagnostics for inspect panes.
/// </summary>
internal static class ThermalNetworkDiagnostics
{
    private static readonly IntVec3[] CardinalDirections =
    [
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    ];

    public static ThermalNetworkDiagnosticSnapshot InspectPipe(Building_ThermalPipe pipe)
    {
        if (!pipe.Spawned)
        {
            return EmptySnapshot();
        }

        return InspectNetwork(pipe.Map, [pipe.Position]);
    }

    public static ThermalNetworkDiagnosticSnapshot InspectVent(Building_ThermalVent vent)
    {
        return !vent.Spawned ? EmptySnapshot() : InspectNetwork(vent.Map, vent.AdjacentPipeCells);
    }

    public static ThermalNetworkDiagnosticSnapshot InspectValve(Building_ThermalValve valve)
    {
        if (!valve.Spawned)
        {
            return EmptySnapshot();
        }

        ThermalNetworkBlockerDiagnostic? selectedBlocker = valve.IsOpen ? null : CreateBlockerDiagnostic(valve);
        IEnumerable<IntVec3> startingCells = valve.IsOpen
            ? [valve.Position]
            : AdjacentOpenNetworkCells(valve.Map, valve.Position);
        return InspectNetwork(valve.Map, startingCells, selectedBlocker);
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

    private static ThermalNetworkDiagnosticSnapshot InspectNetwork(
        Map map,
        IEnumerable<IntVec3> startingCells,
        ThermalNetworkBlockerDiagnostic? selectedBlocker = null)
    {
        IntVec3[] connectedCells = ThermalPipeNetworkTraversal.FindConnectedCells(
                startingCells,
                cell => cell.InBounds(map) && ThermalPipeUtility.HasOpenThermalNetworkCellAt(cell, map),
                (fromCell, toCell) => ThermalPipeUtility.CanTraverseThermalNetworkEdge(fromCell, toCell, map))
            .Distinct()
            .ToArray();
        Building_ThermalVent[] vents = FindConnectedVents(map, connectedCells).ToArray();
        Building_ThermalVent[] activeVents = vents.Where(vent => vent.IsOpen).ToArray();
        List<ThermalNetworkBlockerDiagnostic> blockers = FindAdjacentBlockers(map, connectedCells).ToList();
        if (selectedBlocker.HasValue && !ContainsBlocker(blockers, selectedBlocker.Value))
        {
            blockers.Insert(0, selectedBlocker.Value);
        }

        return new ThermalNetworkDiagnosticSnapshot(
            AverageNetworkTemperature(activeVents
                .Where(vent => vent.FlowMode == ThermalVentFlowMode.PullFromAirSide)
                .Select(vent => vent.ConnectedRoom?.Temperature)
                .Where(temperature => temperature.HasValue)
                .Select(temperature => temperature!.Value)),
            connectedCells.Length,
            SummarizeRooms(activeVents),
            vents.Length,
            activeVents.Count(vent => vent.FlowMode == ThermalVentFlowMode.PullFromAirSide),
            activeVents.Count(vent => vent.FlowMode == ThermalVentFlowMode.PushToAirSide),
            blockers);
    }

    private static ThermalNetworkDiagnosticSnapshot EmptySnapshot()
    {
        return new ThermalNetworkDiagnosticSnapshot(
            null,
            networkCellCount: 0,
            rooms: [],
            ventCount: 0,
            inputVentCount: 0,
            outputVentCount: 0,
            blockers: []);
    }

    private static IEnumerable<IntVec3> AdjacentOpenNetworkCells(Map map, IntVec3 center)
    {
        foreach (IntVec3 direction in CardinalDirections)
        {
            IntVec3 adjacentCell = center + direction;
            if (adjacentCell.InBounds(map) && ThermalPipeUtility.HasOpenThermalNetworkCellAt(adjacentCell, map))
            {
                yield return adjacentCell;
            }
        }
    }

    private static IEnumerable<Building_ThermalVent> FindConnectedVents(Map map, IReadOnlyCollection<IntVec3> networkCells)
    {
        HashSet<Building_ThermalVent> vents = [];
        foreach (IntVec3 networkCell in networkCells)
        {
            foreach (IntVec3 direction in CardinalDirections)
            {
                IntVec3 ventCell = networkCell + direction;
                if (!ventCell.InBounds(map))
                {
                    continue;
                }

                List<Thing> things = ventCell.GetThingList(map);
                for (int index = 0; index < things.Count; index++)
                {
                    if (things[index] is Building_ThermalVent vent
                        && vent.ConnectsToPipeCell(networkCell)
                        && vents.Add(vent))
                    {
                        yield return vent;
                    }
                }
            }
        }
    }

    private static IReadOnlyList<ThermalNetworkRoomDiagnostic> SummarizeRooms(IEnumerable<Building_ThermalVent> vents)
    {
        Dictionary<Room, RoomAccumulator> rooms = [];
        foreach (Building_ThermalVent vent in vents)
        {
            Room? room = vent.ConnectedRoom;
            if (room == null)
            {
                continue;
            }

            if (!rooms.TryGetValue(room, out RoomAccumulator accumulator))
            {
                accumulator = new RoomAccumulator(RoomLabel(room), room.Temperature);
                rooms[room] = accumulator;
            }

            if (vent.FlowMode == ThermalVentFlowMode.PullFromAirSide)
            {
                accumulator.InputVentCount++;
            }
            else
            {
                accumulator.OutputVentCount++;
            }
        }

        return rooms.Values
            .Select(room => new ThermalNetworkRoomDiagnostic(
                room.Label,
                room.Temperature,
                room.InputVentCount,
                room.OutputVentCount))
            .ToArray();
    }

    private static string RoomLabel(Room room)
    {
        string? label = room.Role?.label;
        if (!string.IsNullOrWhiteSpace(label))
        {
            return label!;
        }

        return "TunnelerLife_ThermalInspectorRoom".Translate().ToString();
    }

    private static IEnumerable<ThermalNetworkBlockerDiagnostic> FindAdjacentBlockers(
        Map map,
        IReadOnlyCollection<IntVec3> networkCells)
    {
        HashSet<Building_ThermalValve> blockers = [];
        foreach (IntVec3 networkCell in networkCells)
        {
            foreach (IntVec3 direction in CardinalDirections)
            {
                IntVec3 adjacentCell = networkCell + direction;
                if (!adjacentCell.InBounds(map))
                {
                    continue;
                }

                List<Thing> things = adjacentCell.GetThingList(map);
                for (int index = 0; index < things.Count; index++)
                {
                    if (things[index] is Building_ThermalValve valve
                        && !valve.IsOpen
                        && blockers.Add(valve))
                    {
                        yield return CreateBlockerDiagnostic(valve);
                    }
                }
            }
        }
    }

    private static ThermalNetworkBlockerDiagnostic CreateBlockerDiagnostic(Building_ThermalValve valve)
    {
        if (valve is Building_ThermostaticValve thermostaticValve)
        {
            return new ThermalNetworkBlockerDiagnostic(
                "TunnelerLife_ThermalInspectorThermostaticValve".Translate(),
                ThermostaticStatusLabel(thermostaticValve.Status));
        }

        return new ThermalNetworkBlockerDiagnostic(
            "TunnelerLife_ThermalInspectorThermalValve".Translate(),
            "TunnelerLife_ThermalInspectorBlockedClosed".Translate());
    }

    private static string ThermostaticStatusLabel(ThermostaticValveStatus status)
    {
        return status switch
        {
            ThermostaticValveStatus.Open => "TunnelerLife_ThermostaticValveStatusOpen".Translate(),
            ThermostaticValveStatus.BlockedNoPower => "TunnelerLife_ThermostaticValveStatusNoPower".Translate(),
            ThermostaticValveStatus.BlockedNoUsefulSource => "TunnelerLife_ThermostaticValveStatusNoUsefulSource".Translate(),
            _ => "TunnelerLife_ThermostaticValveStatusClosed".Translate()
        };
    }

    private static bool ContainsBlocker(
        IEnumerable<ThermalNetworkBlockerDiagnostic> blockers,
        ThermalNetworkBlockerDiagnostic blocker)
    {
        return blockers.Any(existing => existing.Label == blocker.Label && existing.Status == blocker.Status);
    }

    private sealed class RoomAccumulator(string label, float temperature)
    {
        public string Label { get; } = label;

        public float Temperature { get; } = temperature;

        public int InputVentCount { get; set; }

        public int OutputVentCount { get; set; }
    }
}
