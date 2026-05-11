using System;
using System.Collections.Generic;
using Verse;

namespace TunnelerLife;

internal static class ThermalPipeOverlayGeometry
{
    private static readonly IntVec3[] ForwardDirections =
    [
        IntVec3.North,
        IntVec3.East
    ];

    private static readonly IntVec3[] CardinalDirections =
    [
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    ];

    public static IEnumerable<ThermalPipeOverlaySegment> GetConnectedSegments(IEnumerable<IntVec3> pipeCells)
    {
        HashSet<IntVec3> cells = [..pipeCells];
        foreach (IntVec3 cell in cells)
        {
            foreach (IntVec3 direction in ForwardDirections)
            {
                IntVec3 adjacentCell = cell + direction;
                if (cells.Contains(adjacentCell))
                {
                    yield return new ThermalPipeOverlaySegment(cell, adjacentCell);
                }
            }
        }
    }

    public static IEnumerable<ThermalPipeOverlayCell> GetLinkedCells(IEnumerable<IntVec3> pipeCells)
    {
        return GetLinkedCells(pipeCells, pipeCells);
    }

    public static IEnumerable<ThermalPipeOverlayCell> GetLinkedCells(
        IEnumerable<IntVec3> visibleCells,
        IEnumerable<IntVec3> activeCells)
    {
        HashSet<IntVec3> visibleCellSet = [..visibleCells];
        HashSet<IntVec3> activeCellSet = [..activeCells];
        foreach (IntVec3 cell in visibleCellSet)
        {
            yield return new ThermalPipeOverlayCell(cell, GetLinkDirections(cell, activeCellSet));
        }
    }

    public static IEnumerable<IntVec3> GetIsolatedCells(IEnumerable<IntVec3> pipeCells)
    {
        HashSet<IntVec3> cells = [..pipeCells];
        foreach (IntVec3 cell in cells)
        {
            bool hasNeighbor = false;
            foreach (IntVec3 direction in CardinalDirections)
            {
                if (cells.Contains(cell + direction))
                {
                    hasNeighbor = true;
                    break;
                }
            }

            if (!hasNeighbor)
            {
                yield return cell;
            }
        }
    }

    private static LinkDirections GetLinkDirections(IntVec3 cell, HashSet<IntVec3> cells)
    {
        if (!cells.Contains(cell))
        {
            return LinkDirections.None;
        }

        LinkDirections directions = LinkDirections.None;
        if (cells.Contains(cell + IntVec3.North))
        {
            directions |= LinkDirections.Up;
        }

        if (cells.Contains(cell + IntVec3.East))
        {
            directions |= LinkDirections.Right;
        }

        if (cells.Contains(cell + IntVec3.South))
        {
            directions |= LinkDirections.Down;
        }

        if (cells.Contains(cell + IntVec3.West))
        {
            directions |= LinkDirections.Left;
        }

        return directions;
    }
}

internal readonly struct ThermalPipeOverlaySegment : IEquatable<ThermalPipeOverlaySegment>
{
    public ThermalPipeOverlaySegment(IntVec3 from, IntVec3 to)
    {
        From = from;
        To = to;
    }

    public IntVec3 From { get; }

    public IntVec3 To { get; }

    public bool Equals(ThermalPipeOverlaySegment other)
    {
        return From == other.From && To == other.To;
    }

    public override bool Equals(object? obj)
    {
        return obj is ThermalPipeOverlaySegment other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (From.GetHashCode() * 397) ^ To.GetHashCode();
        }
    }
}

internal readonly struct ThermalPipeOverlayCell : IEquatable<ThermalPipeOverlayCell>
{
    public ThermalPipeOverlayCell(IntVec3 cell, LinkDirections linkDirections)
    {
        Cell = cell;
        LinkDirections = linkDirections;
    }

    public IntVec3 Cell { get; }

    public LinkDirections LinkDirections { get; }

    public bool Equals(ThermalPipeOverlayCell other)
    {
        return Cell == other.Cell && LinkDirections == other.LinkDirections;
    }

    public override bool Equals(object? obj)
    {
        return obj is ThermalPipeOverlayCell other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Cell.GetHashCode() * 397) ^ LinkDirections.GetHashCode();
        }
    }
}
