using System.Linq;
using TunnelerLife;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalNetworkSideSelectorTests
{
    [Fact]
    public void GetControlledSideCell_UsesValveRotation()
    {
        IntVec3 valveCell = new(10, 0, 10);

        Assert.Equal(valveCell + IntVec3.South, ThermalNetworkRoomScanner.GetControlledSideCell(valveCell, Rot4.North));
        Assert.Equal(valveCell + IntVec3.West, ThermalNetworkRoomScanner.GetControlledSideCell(valveCell, Rot4.East));
    }

    [Fact]
    public void GetSourceSideCells_ExcludesControlledSide()
    {
        IntVec3 valveCell = new(10, 0, 10);
        IntVec3 controlledCell = ThermalNetworkRoomScanner.GetControlledSideCell(valveCell, Rot4.North);

        IntVec3[] sourceCells = ThermalNetworkRoomScanner.GetSourceSideCells(valveCell, Rot4.North).ToArray();

        Assert.Equal(3, sourceCells.Length);
        Assert.DoesNotContain(controlledCell, sourceCells);
        Assert.Contains(valveCell + IntVec3.North, sourceCells);
        Assert.Contains(valveCell + IntVec3.East, sourceCells);
        Assert.Contains(valveCell + IntVec3.West, sourceCells);
    }

    [Fact]
    public void ResolveControlledSideCells_UsesSingleDirectVentWhenRotationPointsElsewhere()
    {
        IntVec3 valveCell = new(10, 0, 10);
        IntVec3 directVentCell = valveCell + IntVec3.West;

        IntVec3[] controlledCells = ThermalNetworkRoomScanner
            .ResolveControlledSideCells(valveCell, Rot4.North, [directVentCell])
            .ToArray();

        Assert.Equal([directVentCell], controlledCells);
    }

    [Fact]
    public void ResolveSourceSideCells_ExcludesResolvedDirectVentControlledSide()
    {
        IntVec3 valveCell = new(10, 0, 10);
        IntVec3 directVentCell = valveCell + IntVec3.West;
        IntVec3[] controlledCells = [directVentCell];

        IntVec3[] sourceCells = ThermalNetworkRoomScanner
            .ResolveSourceSideCells(valveCell, controlledCells)
            .ToArray();

        Assert.DoesNotContain(directVentCell, sourceCells);
        Assert.Contains(valveCell + IntVec3.North, sourceCells);
        Assert.Contains(valveCell + IntVec3.East, sourceCells);
        Assert.Contains(valveCell + IntVec3.South, sourceCells);
    }

    [Fact]
    public void SelectSourceTemperature_ForHeatingUsesWarmestReachableRoom()
    {
        float? selected = ThermalNetworkRoomScanner.SelectSourceTemperature(
            [15f, 28f, 22f],
            ThermostaticValveMode.Heating);

        Assert.Equal(28f, selected);
    }

    [Fact]
    public void SelectSourceTemperature_ForCoolingUsesColdestReachableRoom()
    {
        float? selected = ThermalNetworkRoomScanner.SelectSourceTemperature(
            [15f, 28f, 22f],
            ThermostaticValveMode.Cooling);

        Assert.Equal(15f, selected);
    }

    [Fact]
    public void SelectSourceTemperature_ReturnsNullWhenNoRoomIsReachable()
    {
        float? selected = ThermalNetworkRoomScanner.SelectSourceTemperature(
            [],
            ThermostaticValveMode.Cooling);

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
