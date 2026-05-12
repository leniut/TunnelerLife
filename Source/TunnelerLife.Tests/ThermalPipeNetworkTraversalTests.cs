using System.Linq;
using TunnelerLife;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalPipeNetworkTraversalTests
{
    [Fact]
    public void FindConnectedCells_StopsAtInactiveSwitchCell()
    {
        IntVec3[] startingCells =
        [
            new(0, 0, 0)
        ];
        IntVec3[] activeNetworkCells =
        [
            new(0, 0, 0),
            new(2, 0, 0)
        ];

        IntVec3[] connectedCells = ThermalPipeNetworkTraversal
            .FindConnectedCells(startingCells, activeNetworkCells.Contains)
            .ToArray();

        Assert.Equal([new IntVec3(0, 0, 0)], connectedCells);
    }

    [Fact]
    public void FindConnectedCells_CrossesActiveSwitchCell()
    {
        IntVec3[] startingCells =
        [
            new(0, 0, 0)
        ];
        IntVec3[] activeNetworkCells =
        [
            new(0, 0, 0),
            new(1, 0, 0),
            new(2, 0, 0)
        ];

        IntVec3[] connectedCells = ThermalPipeNetworkTraversal
            .FindConnectedCells(startingCells, activeNetworkCells.Contains)
            .ToArray();

        Assert.Equal(
            [
                new IntVec3(0, 0, 0),
                new IntVec3(1, 0, 0),
                new IntVec3(2, 0, 0)
            ],
            connectedCells);
    }

    [Fact]
    public void FindConnectedCells_UsesDirectionalEdgeFilter()
    {
        IntVec3 startCell = new(0, 0, 0);
        IntVec3 switchCell = new(1, 0, 0);
        IntVec3 blockedCell = new(2, 0, 0);
        IntVec3[] activeNetworkCells =
        [
            startCell,
            switchCell,
            blockedCell
        ];

        IntVec3[] connectedCells = ThermalPipeNetworkTraversal
            .FindConnectedCells(
                [startCell],
                activeNetworkCells.Contains,
                (fromCell, toCell) => fromCell != switchCell || toCell != blockedCell)
            .ToArray();

        Assert.Equal([startCell, switchCell], connectedCells);
    }
}
