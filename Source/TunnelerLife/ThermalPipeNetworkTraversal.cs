using System;
using System.Collections.Generic;
using Verse;

namespace TunnelerLife;

internal static class ThermalPipeNetworkTraversal
{
    private static readonly IntVec3[] CardinalDirections =
    [
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    ];

    public static IEnumerable<IntVec3> FindConnectedCells(
        IEnumerable<IntVec3> startingCells,
        Func<IntVec3, bool> isOpenNetworkCell)
    {
        Queue<IntVec3> pending = new();
        HashSet<IntVec3> visited = [];

        foreach (IntVec3 startingCell in startingCells)
        {
            if (isOpenNetworkCell(startingCell) && visited.Add(startingCell))
            {
                pending.Enqueue(startingCell);
            }
        }

        while (pending.Count > 0)
        {
            IntVec3 cell = pending.Dequeue();
            yield return cell;

            foreach (IntVec3 direction in CardinalDirections)
            {
                IntVec3 adjacentCell = cell + direction;
                if (!visited.Contains(adjacentCell) && isOpenNetworkCell(adjacentCell))
                {
                    visited.Add(adjacentCell);
                    pending.Enqueue(adjacentCell);
                }
            }
        }
    }
}
