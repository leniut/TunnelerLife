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

    public static IEnumerable<IntVec3> ResolveControlledSideCells(IEnumerable<IntVec3> directVentCells)
    {
        return directVentCells;
    }

    public static IEnumerable<IntVec3> ResolveSourceSideCells(
        IntVec3 valveCell,
        IEnumerable<IntVec3> controlledSideCells,
        Func<IntVec3, bool> isOpenNetworkCell)
    {
        HashSet<IntVec3> controlledCellSet = [.. controlledSideCells];
        foreach (IntVec3 direction in CardinalDirections)
        {
            IntVec3 sideCell = valveCell + direction;
            if (!controlledCellSet.Contains(sideCell) && isOpenNetworkCell(sideCell))
            {
                yield return sideCell;
            }
        }
    }

    public static float? SelectSourceTemperature(
        IEnumerable<float> sourceTemperatures,
        float targetTemperature,
        float controlledTemperature)
    {
        float? selectedTemperature = null;
        foreach (float sourceTemperature in sourceTemperatures)
        {
            selectedTemperature = selectedTemperature.HasValue
                ? SelectMoreUsefulTemperature(
                    selectedTemperature.Value,
                    sourceTemperature,
                    targetTemperature,
                    controlledTemperature)
                : sourceTemperature;
        }

        return selectedTemperature;
    }

    public static IReadOnlyList<ThermalNetworkRoomPort> FindRoomPorts(
        Map map,
        IEnumerable<IntVec3> networkCells,
        ThermalVentFlowMode? flowMode = null)
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

                Room? room = GetConnectedVentRoom(networkCell, roomCell, map, flowMode);
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
        float targetTemperature)
    {
        if (!valve.Spawned)
        {
            return new ThermalNetworkSideTemperatures(null, null);
        }

        Map map = valve.Map;
        Func<IntVec3, bool> isOpenNetworkCell = cell =>
            cell.InBounds(map) && ThermalPipeUtility.HasOpenThermalNetworkCellAt(cell, map);
        Func<IntVec3, bool> isSourceNetworkCell = cell =>
            cell != valve.Position && isOpenNetworkCell(cell);
        Func<IntVec3, IntVec3, bool> canTraverseNetworkEdge = (fromCell, toCell) =>
            ThermalPipeUtility.CanTraverseThermalNetworkEdge(fromCell, toCell, map);
        IntVec3[] directVentCells = FindDirectVentCells(
            map,
            valve.Position,
            ThermalVentFlowMode.PushToAirSide).ToArray();
        IntVec3[] controlledSideCells = ResolveControlledSideCells(directVentCells).ToArray();
        IntVec3[] sourceSideCells = ResolveSourceSideCells(
            valve.Position,
            controlledSideCells,
            isOpenNetworkCell).ToArray();

        IReadOnlyList<ThermalNetworkRoomPort> controlledPorts = FindDirectVentRoomPorts(
            map,
            GetDirectVentProbeCells(valve.Position, controlledSideCells),
            ThermalVentFlowMode.PushToAirSide);

        float? controlledTemperature = AverageTemperature(controlledPorts);
        ThermalNetworkSourceCandidate? sourceCandidate = controlledTemperature.HasValue
            ? SelectSourceCandidate(
                FindSourceCandidates(
                    map,
                    sourceSideCells,
                    isSourceNetworkCell,
                    canTraverseNetworkEdge,
                    targetTemperature,
                    controlledTemperature.Value),
                targetTemperature,
                controlledTemperature.Value)
            : null;

        return new ThermalNetworkSideTemperatures(
            controlledTemperature,
            sourceCandidate?.Temperature,
            sourceCandidate?.SideCell);
    }

    private static IEnumerable<IntVec3> FindDirectVentCells(
        Map map,
        IntVec3 valveCell,
        ThermalVentFlowMode? flowMode = null)
    {
        foreach (IntVec3 direction in CardinalDirections)
        {
            IntVec3 ventCell = valveCell + direction;
            if (ventCell.InBounds(map) && GetConnectedVentRoom(valveCell, ventCell, map, flowMode) != null)
            {
                yield return ventCell;
            }
        }
    }

    private static IReadOnlyList<ThermalNetworkRoomPort> FindNetworkSideRoomPorts(
        Map map,
        IEnumerable<IntVec3> sideCells,
        Func<IntVec3, bool> isOpenNetworkCell,
        Func<IntVec3, IntVec3, bool> canTraverseNetworkEdge)
    {
        return FindRoomPorts(
            map,
            ThermalPipeNetworkTraversal.FindConnectedCells(
                sideCells,
                isOpenNetworkCell,
                canTraverseNetworkEdge),
            ThermalVentFlowMode.PullFromAirSide);
    }

    private static IEnumerable<ThermalNetworkSourceCandidate> FindSourceCandidates(
        Map map,
        IEnumerable<IntVec3> sourceSideCells,
        Func<IntVec3, bool> isOpenNetworkCell,
        Func<IntVec3, IntVec3, bool> canTraverseNetworkEdge,
        float targetTemperature,
        float controlledTemperature)
    {
        foreach (IntVec3 sourceSideCell in sourceSideCells)
        {
            float? sourceTemperature = SelectSourceTemperature(
                FindNetworkSideRoomPorts(map, [sourceSideCell], isOpenNetworkCell, canTraverseNetworkEdge)
                    .Select(port => port.Temperature),
                targetTemperature,
                controlledTemperature);
            if (sourceTemperature.HasValue)
            {
                yield return new ThermalNetworkSourceCandidate(sourceSideCell, sourceTemperature.Value);
            }
        }
    }

    private static ThermalNetworkSourceCandidate? SelectSourceCandidate(
        IEnumerable<ThermalNetworkSourceCandidate> sourceCandidates,
        float targetTemperature,
        float controlledTemperature)
    {
        ThermalNetworkSourceCandidate? selectedCandidate = null;
        foreach (ThermalNetworkSourceCandidate sourceCandidate in sourceCandidates)
        {
            selectedCandidate = selectedCandidate.HasValue
                ? SelectMoreUsefulCandidate(
                    selectedCandidate.Value,
                    sourceCandidate,
                    targetTemperature,
                    controlledTemperature)
                : sourceCandidate;
        }

        return selectedCandidate;
    }

    private static ThermalNetworkSourceCandidate SelectMoreUsefulCandidate(
        ThermalNetworkSourceCandidate currentCandidate,
        ThermalNetworkSourceCandidate newCandidate,
        float targetTemperature,
        float controlledTemperature)
    {
        return SelectMoreUsefulTemperature(
                currentCandidate.Temperature,
                newCandidate.Temperature,
                targetTemperature,
                controlledTemperature) == currentCandidate.Temperature
            ? currentCandidate
            : newCandidate;
    }

    private static IReadOnlyList<ThermalNetworkRoomPort> FindDirectVentRoomPorts(
        Map map,
        IEnumerable<ThermalNetworkVentProbe> ventProbes,
        ThermalVentFlowMode? flowMode = null)
    {
        List<ThermalNetworkRoomPort> roomPorts = [];
        HashSet<Room> visitedRooms = [];

        foreach (ThermalNetworkVentProbe ventProbe in ventProbes)
        {
            if (!ventProbe.VentCell.InBounds(map))
            {
                continue;
            }

            Room? room = GetConnectedVentRoom(ventProbe.NetworkCell, ventProbe.VentCell, map, flowMode);
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

    private static Room? GetConnectedVentRoom(
        IntVec3 networkCell,
        IntVec3 ventCell,
        Map map,
        ThermalVentFlowMode? flowMode = null)
    {
        List<Thing> things = ventCell.GetThingList(map);
        for (int index = 0; index < things.Count; index++)
        {
            if (things[index] is Building_ThermalVent vent
                && vent.IsOpen
                && (!flowMode.HasValue || vent.FlowMode == flowMode.Value)
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
        float targetTemperature,
        float controlledTemperature)
    {
        return controlledTemperature < targetTemperature
            ? Math.Max(currentTemperature, candidateTemperature)
            : Math.Min(currentTemperature, candidateTemperature);
    }
}
