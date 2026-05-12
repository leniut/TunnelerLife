using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Finds pipe-connected thermal vents and applies passive room temperature equalization.
/// </summary>
public static class ThermalVentNetworkService
{
    private const float EqualizationRate = 14f;

    private static readonly IntVec3[] CardinalDirections =
    [
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    ];

    public static bool TryEqualizeNetwork(Building_ThermalVent origin)
    {
        if (!origin.Spawned || !origin.IsOpen)
        {
            return false;
        }

        List<Building_ThermalVent> vents = FindConnectedOpenVents(origin);
        if (vents.Count < 2 || origin.thingIDNumber != vents.Min(vent => vent.thingIDNumber))
        {
            return false;
        }

        List<RoomPortGroup> sourceRoomPortGroups = GetRoomPortGroups(
            vents.Where(vent => vent.FlowMode == ThermalVentFlowMode.PullFromAirSide));
        List<RoomPortGroup> outputRoomPortGroups = GetRoomPortGroups(
            vents.Where(vent => vent.FlowMode == ThermalVentFlowMode.PushToAirSide));

        if (sourceRoomPortGroups.Count == 0 || outputRoomPortGroups.Count == 0)
        {
            return false;
        }

        EqualizeRooms(sourceRoomPortGroups, outputRoomPortGroups, origin.Map.Biome.inVacuum);
        return true;
    }

    private static List<Building_ThermalVent> FindConnectedOpenVents(Building_ThermalVent origin)
    {
        Map map = origin.Map;
        HashSet<Building_ThermalVent> vents = [];

        foreach (IntVec3 pipeCell in ThermalPipeNetworkTraversal.FindConnectedCells(
            origin.AdjacentPipeCells,
            cell => cell.InBounds(map) && ThermalPipeUtility.HasOpenThermalNetworkCellAt(cell, map),
            (fromCell, toCell) => ThermalPipeUtility.CanTraverseThermalNetworkEdge(fromCell, toCell, map)))
        {
            AddConnectedVents(pipeCell, map, vents);
        }

        return vents.ToList();
    }

    private static void AddConnectedVents(IntVec3 pipeCell, Map map, HashSet<Building_ThermalVent> vents)
    {
        foreach (IntVec3 direction in CardinalDirections)
        {
            IntVec3 ventCell = pipeCell + direction;
            if (!ventCell.InBounds(map))
            {
                continue;
            }

            List<Thing> things = ventCell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Building_ThermalVent vent
                    && vent.IsOpen
                    && vent.ConnectsToPipeCell(pipeCell))
                {
                    vents.Add(vent);
                }
            }
        }
    }

    private static List<RoomPortGroup> GetRoomPortGroups(IEnumerable<Building_ThermalVent> vents)
    {
        Dictionary<Room, int> portCounts = [];
        foreach (Building_ThermalVent vent in vents)
        {
            Room? room = vent.ConnectedRoom;
            if (room == null)
            {
                continue;
            }

            portCounts.TryGetValue(room, out int currentCount);
            portCounts[room] = currentCount + 1;
        }

        return portCounts.Select(pair => new RoomPortGroup(pair.Key, pair.Value)).ToList();
    }

    private static void EqualizeRooms(
        IReadOnlyList<RoomPortGroup> sourceRoomPortGroups,
        IReadOnlyList<RoomPortGroup> outputRoomPortGroups,
        bool inVacuum)
    {
        RoomPortGroup[] roomPortGroups = sourceRoomPortGroups
            .Concat(outputRoomPortGroups)
            .ToArray();
        ThermalRoomState[] states = roomPortGroups
            .Select(group => new ThermalRoomState(
                group.Room.Temperature,
                group.Room.CellCount,
                group.Room.UsesOutdoorTemperature,
                group.ExchangePortCount))
            .ToArray();
        float[] deltas = ThermalExchangeCalculator.CalculateTemperatureDeltas(states, EqualizationRate, inVacuum);

        for (int i = sourceRoomPortGroups.Count; i < roomPortGroups.Length; i++)
        {
            Room room = roomPortGroups[i].Room;
            if (!room.UsesOutdoorTemperature)
            {
                room.Temperature += deltas[i];
            }
        }
    }

    private readonly struct RoomPortGroup(Room room, int exchangePortCount)
    {
        public Room Room { get; } = room;

        public int ExchangePortCount { get; } = exchangePortCount;
    }
}
