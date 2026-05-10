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
        if (!origin.Spawned || !origin.IsOpen || !HasThermalPipeAt(origin.PipeCell, origin.Map))
        {
            return false;
        }

        List<Building_ThermalVent> vents = FindConnectedOpenVents(origin);
        if (vents.Count < 2 || origin.thingIDNumber != vents.Min(vent => vent.thingIDNumber))
        {
            return false;
        }

        List<Room> rooms = vents
            .Select(vent => vent.ConnectedRoom)
            .Where(room => room != null)
            .Distinct()
            .ToList()!;

        if (rooms.Count < 2)
        {
            return false;
        }

        EqualizeRooms(rooms, origin.Map.Biome.inVacuum);
        return true;
    }

    private static List<Building_ThermalVent> FindConnectedOpenVents(Building_ThermalVent origin)
    {
        Map map = origin.Map;
        Queue<IntVec3> pending = new();
        HashSet<IntVec3> visitedPipes = [];
        HashSet<Building_ThermalVent> vents = [];

        pending.Enqueue(origin.PipeCell);
        visitedPipes.Add(origin.PipeCell);

        while (pending.Count > 0)
        {
            IntVec3 pipeCell = pending.Dequeue();
            AddConnectedVents(pipeCell, map, vents);

            foreach (IntVec3 direction in CardinalDirections)
            {
                IntVec3 adjacentPipeCell = pipeCell + direction;
                if (adjacentPipeCell.InBounds(map)
                    && !visitedPipes.Contains(adjacentPipeCell)
                    && HasThermalPipeAt(adjacentPipeCell, map))
                {
                    visitedPipes.Add(adjacentPipeCell);
                    pending.Enqueue(adjacentPipeCell);
                }
            }
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

    private static bool HasThermalPipeAt(IntVec3 cell, Map map)
    {
        if (!cell.InBounds(map))
        {
            return false;
        }

        List<Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            if (things[i] is Building_ThermalPipe)
            {
                return true;
            }
        }

        return false;
    }

    private static void EqualizeRooms(IReadOnlyList<Room> rooms, bool inVacuum)
    {
        ThermalRoomState[] states = rooms
            .Select(room => new ThermalRoomState(room.Temperature, room.CellCount, room.UsesOutdoorTemperature))
            .ToArray();
        float[] deltas = ThermalExchangeCalculator.CalculateTemperatureDeltas(states, EqualizationRate, inVacuum);

        for (int i = 0; i < rooms.Count; i++)
        {
            if (!rooms[i].UsesOutdoorTemperature)
            {
                rooms[i].Temperature += deltas[i];
            }
        }
    }
}
