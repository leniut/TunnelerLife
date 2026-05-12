using System.Linq;
using TunnelerLife;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalNetworkSideSelectorTests
{
    [Fact]
    public void ResolveControlledSideCells_UsesEveryDirectVentAsOutput()
    {
        IntVec3 valveCell = new(10, 0, 10);
        IntVec3[] directVentCells =
        [
            valveCell + IntVec3.West,
            valveCell + IntVec3.East
        ];

        IntVec3[] controlledCells = ThermalNetworkRoomScanner
            .ResolveControlledSideCells(directVentCells)
            .ToArray();

        Assert.Equal(directVentCells, controlledCells);
    }

    [Fact]
    public void ResolveSourceSideCells_UsesOnlyOpenNetworkInputsAndExcludesVentOutputs()
    {
        IntVec3 valveCell = new(10, 0, 10);
        IntVec3 outputVentCell = valveCell + IntVec3.West;
        IntVec3 northPipeCell = valveCell + IntVec3.North;
        IntVec3 southPipeCell = valveCell + IntVec3.South;
        IntVec3 emptyCell = valveCell + IntVec3.East;
        IntVec3[] controlledCells = [outputVentCell];
        IntVec3[] openNetworkCells = [northPipeCell, southPipeCell];

        IntVec3[] sourceCells = ThermalNetworkRoomScanner
            .ResolveSourceSideCells(valveCell, controlledCells, openNetworkCells.Contains)
            .ToArray();

        Assert.Equal(2, sourceCells.Length);
        Assert.DoesNotContain(outputVentCell, sourceCells);
        Assert.DoesNotContain(emptyCell, sourceCells);
        Assert.Contains(northPipeCell, sourceCells);
        Assert.Contains(southPipeCell, sourceCells);
    }

    [Fact]
    public void SelectSourceTemperature_WhenControlledRoomIsTooColdUsesWarmestReachableRoom()
    {
        float? selected = ThermalNetworkRoomScanner.SelectSourceTemperature(
            [15f, 28f, 22f],
            targetTemperature: 21f,
            controlledTemperature: 19f);

        Assert.Equal(28f, selected);
    }

    [Fact]
    public void SelectSourceTemperature_WhenControlledRoomIsTooWarmUsesColdestReachableRoom()
    {
        float? selected = ThermalNetworkRoomScanner.SelectSourceTemperature(
            [15f, 28f, 22f],
            targetTemperature: 21f,
            controlledTemperature: 24f);

        Assert.Equal(15f, selected);
    }

    [Fact]
    public void SelectSourceTemperature_ReturnsNullWhenNoRoomIsReachable()
    {
        float? selected = ThermalNetworkRoomScanner.SelectSourceTemperature(
            [],
            targetTemperature: 21f,
            controlledTemperature: 24f);

        Assert.Null(selected);
    }

    [Fact]
    public void GetDirectVentProbeCells_UsesValveCellAsNetworkCellForAdjacentVents()
    {
        IntVec3 valveCell = new(10, 0, 10);
        IntVec3 controlledSideCell = valveCell + IntVec3.West;

        ThermalNetworkVentProbe probe = ThermalNetworkRoomScanner
            .GetDirectVentProbeCells(valveCell, [controlledSideCell])
            .Single();

        Assert.Equal(valveCell, probe.NetworkCell);
        Assert.Equal(controlledSideCell, probe.VentCell);
    }
}
