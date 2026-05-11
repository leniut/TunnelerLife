using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TunnelerLife;

internal static class ThermalNetworkRoomScanner
{
    private static readonly IntVec3[] CardinalDirections =
    [
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    ];

    public static IntVec3 GetControlledSideCell(IntVec3 valveCell, Rot4 rotation)
    {
        return valveCell + IntVec3.South.RotatedBy(rotation);
    }

    public static IEnumerable<IntVec3> GetSourceSideCells(IntVec3 valveCell, Rot4 rotation)
    {
        IntVec3 controlledSideCell = GetControlledSideCell(valveCell, rotation);
        return ResolveSourceSideCells(valveCell, [controlledSideCell]);
    }

    public static IEnumerable<IntVec3> ResolveControlledSideCells(
        IntVec3 valveCell,
        Rot4 rotation,
        IEnumerable<IntVec3> directVentCells)
    {
        IntVec3 rotatedControlledCell = GetControlledSideCell(valveCell, rotation);
        IntVec3[] directVentCellArray = directVentCells.ToArray();
        if (!directVentCellArray.Contains(rotatedControlledCell) && directVentCellArray.Length == 1)
        {
            yield return directVentCellArray[0];
            yield break;
        }

        yield return rotatedControlledCell;
    }

    public static IEnumerable<IntVec3> ResolveSourceSideCells(
        IntVec3 valveCell,
        IEnumerable<IntVec3> controlledSideCells)
    {
        HashSet<IntVec3> controlledCellSet = [.. controlledSideCells];
        foreach (IntVec3 direction in CardinalDirections)
        {
            IntVec3 sideCell = valveCell + direction;
            if (!controlledCellSet.Contains(sideCell))
            {
                yield return sideCell;
            }
        }
    }

    public static float? SelectSourceTemperature(
        IEnumerable<float> sourceTemperatures,
        ThermostaticValveMode mode)
    {
        float? selectedTemperature = null;
        foreach (float sourceTemperature in sourceTemperatures)
        {
            selectedTemperature = selectedTemperature.HasValue
                ? SelectMoreUsefulTemperature(selectedTemperature.Value, sourceTemperature, mode)
                : sourceTemperature;
        }

        return selectedTemperature;
    }

    public static IReadOnlyList<ThermalNetworkRoomPort> FindRoomPorts(
        Map map,
        IEnumerable<IntVec3> networkCells)
    {
        List<ThermalNetworkRoomPort> roomPorts = [];
        HashSet<Room> visitedRooms = [];

        foreach (IntVec3 networkCell in networkCells)
        {
            foreach (IntVec3 direction in CardinalDirections)
            {
                IntVec3 roomCell = networkCell + direction;
                if (!roomCell.InBounds(map))
                {
                    continue;
                }

                Room? room = GetConnectedVentRoom(networkCell, roomCell, map);
                if (room != null && visitedRooms.Add(room))
                {
                    roomPorts.Add(new ThermalNetworkRoomPort(networkCell, roomCell, room, room.Temperature));
                }
            }
        }

        return roomPorts;
    }

    public static IEnumerable<ThermalNetworkVentProbe> GetDirectVentProbeCells(
        IntVec3 valveCell,
        IEnumerable<IntVec3> sideCells)
    {
        foreach (IntVec3 sideCell in sideCells)
        {
            yield return new ThermalNetworkVentProbe(valveCell, sideCell);
        }
    }

    public static ThermalNetworkSideTemperatures GetSideTemperatures(
        Building_ThermostaticValve valve,
        ThermostaticValveMode mode)
    {
        if (!valve.Spawned)
        {
            return new ThermalNetworkSideTemperatures(null, null);
        }

        Map map = valve.Map;
        Func<IntVec3, bool> isOpenNetworkCell = cell =>
            cell.InBounds(map) && ThermalPipeUtility.HasOpenThermalNetworkCellAt(cell, map);
        IntVec3[] directVentCells = FindDirectVentCells(map, valve.Position).ToArray();
        IntVec3[] controlledSideCells = ResolveControlledSideCells(
            valve.Position,
            valve.Rotation,
            directVentCells).ToArray();
        IntVec3[] sourceSideCells = ResolveSourceSideCells(valve.Position, controlledSideCells).ToArray();

        IReadOnlyList<ThermalNetworkRoomPort> controlledPorts = FindSideRoomPorts(
            map,
            valve.Position,
            controlledSideCells,
            isOpenNetworkCell);
        IReadOnlyList<ThermalNetworkRoomPort> sourcePorts = FindSideRoomPorts(
            map,
            valve.Position,
            sourceSideCells,
            isOpenNetworkCell);

        return new ThermalNetworkSideTemperatures(
            AverageTemperature(controlledPorts),
            SelectSourceTemperature(sourcePorts.Select(port => port.Temperature), mode));
    }

    private static IEnumerable<IntVec3> FindDirectVentCells(Map map, IntVec3 valveCell)
    {
        foreach (IntVec3 direction in CardinalDirections)
        {
            IntVec3 ventCell = valveCell + direction;
            if (ventCell.InBounds(map) && GetConnectedVentRoom(valveCell, ventCell, map) != null)
            {
                yield return ventCell;
            }
        }
    }

    private static IReadOnlyList<ThermalNetworkRoomPort> FindSideRoomPorts(
        Map map,
        IntVec3 valveCell,
        IEnumerable<IntVec3> sideCells,
        Func<IntVec3, bool> isOpenNetworkCell)
    {
        IntVec3[] sideCellArray = sideCells.ToArray();
        List<ThermalNetworkRoomPort> roomPorts = [];
        HashSet<Room> visitedRooms = [];

        AddUniqueRoomPorts(
            roomPorts,
            visitedRooms,
            FindDirectVentRoomPorts(map, GetDirectVentProbeCells(valveCell, sideCellArray)));
        AddUniqueRoomPorts(
            roomPorts,
            visitedRooms,
            FindRoomPorts(
                map,
                ThermalPipeNetworkTraversal.FindConnectedCells(sideCellArray, isOpenNetworkCell)));

        return roomPorts;
    }

    private static IReadOnlyList<ThermalNetworkRoomPort> FindDirectVentRoomPorts(
        Map map,
        IEnumerable<ThermalNetworkVentProbe> ventProbes)
    {
        List<ThermalNetworkRoomPort> roomPorts = [];
        HashSet<Room> visitedRooms = [];

        foreach (ThermalNetworkVentProbe ventProbe in ventProbes)
        {
            if (!ventProbe.VentCell.InBounds(map))
            {
                continue;
            }

            Room? room = GetConnectedVentRoom(ventProbe.NetworkCell, ventProbe.VentCell, map);
            if (room != null && visitedRooms.Add(room))
            {
                roomPorts.Add(new ThermalNetworkRoomPort(
                    ventProbe.NetworkCell,
                    ventProbe.VentCell,
                    room,
                    room.Temperature));
            }
        }

        return roomPorts;
    }

    private static void AddUniqueRoomPorts(
        List<ThermalNetworkRoomPort> destination,
        HashSet<Room> visitedRooms,
        IEnumerable<ThermalNetworkRoomPort> candidates)
    {
        foreach (ThermalNetworkRoomPort candidate in candidates)
        {
            if (visitedRooms.Add(candidate.Room))
            {
                destination.Add(candidate);
            }
        }
    }

    private static Room? GetConnectedVentRoom(IntVec3 networkCell, IntVec3 ventCell, Map map)
    {
        List<Thing> things = ventCell.GetThingList(map);
        for (int index = 0; index < things.Count; index++)
        {
            if (things[index] is Building_ThermalVent vent
                && vent.IsOpen
                && vent.ConnectsToPipeCell(networkCell))
            {
                return vent.ConnectedRoom;
            }
        }

        return null;
    }

    private static float? AverageTemperature(IReadOnlyList<ThermalNetworkRoomPort> roomPorts)
    {
        if (roomPorts.Count == 0)
        {
            return null;
        }

        float totalTemperature = 0f;
        for (int index = 0; index < roomPorts.Count; index++)
        {
            totalTemperature += roomPorts[index].Temperature;
        }

        return totalTemperature / roomPorts.Count;
    }

    private static float SelectMoreUsefulTemperature(
        float currentTemperature,
        float candidateTemperature,
        ThermostaticValveMode mode)
    {
        return mode == ThermostaticValveMode.Heating
            ? Math.Max(currentTemperature, candidateTemperature)
            : Math.Min(currentTemperature, candidateTemperature);
    }
}
