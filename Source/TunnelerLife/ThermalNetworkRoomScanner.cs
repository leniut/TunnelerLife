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
        foreach (IntVec3 direction in CardinalDirections)
        {
            IntVec3 sideCell = valveCell + direction;
            if (sideCell != controlledSideCell)
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

        IReadOnlyList<ThermalNetworkRoomPort> controlledPorts = FindRoomPorts(
            map,
            ThermalPipeNetworkTraversal.FindConnectedCells(
                [GetControlledSideCell(valve.Position, valve.Rotation)],
                isOpenNetworkCell));
        IReadOnlyList<ThermalNetworkRoomPort> sourcePorts = FindRoomPorts(
            map,
            ThermalPipeNetworkTraversal.FindConnectedCells(
                GetSourceSideCells(valve.Position, valve.Rotation),
                isOpenNetworkCell));

        return new ThermalNetworkSideTemperatures(
            AverageTemperature(controlledPorts),
            SelectSourceTemperature(sourcePorts.Select(port => port.Temperature), mode));
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
