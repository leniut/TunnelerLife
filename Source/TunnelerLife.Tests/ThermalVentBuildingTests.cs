using System.Linq;
using System.Reflection;
using TunnelerLife;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalVentBuildingTests
{
    [Fact]
    public void PipeCells_UseEverySideExceptAirSide()
    {
        Building_ThermalVent vent = new()
        {
            Position = new IntVec3(10, 0, 10),
            Rotation = Rot4.North
        };

        Assert.Equal(vent.Position + IntVec3.South, vent.OutletCell);
        Assert.Equal(
            [
                vent.Position + IntVec3.North,
                vent.Position + IntVec3.East,
                vent.Position + IntVec3.West
            ],
            vent.PipeCells.ToArray());
    }

    [Fact]
    public void ConnectsToPipeCell_AcceptsEverySideExceptAirSide()
    {
        Building_ThermalVent vent = new()
        {
            Position = new IntVec3(10, 0, 10),
            Rotation = Rot4.North
        };

        Assert.True(vent.ConnectsToPipeCell(vent.Position + IntVec3.North));
        Assert.False(vent.ConnectsToPipeCell(vent.Position + IntVec3.South));
        Assert.True(vent.ConnectsToPipeCell(vent.Position + IntVec3.East));
        Assert.True(vent.ConnectsToPipeCell(vent.Position + IntVec3.West));
    }

    [Fact]
    public void ThermalVent_HasPullPushFlowModeGizmo()
    {
        Assert.Equal(ThermalVentFlowMode.PullFromAirSide, new Building_ThermalVent().FlowMode);
        Assert.Equal(
            typeof(Building_ThermalVent),
            typeof(Building_ThermalVent).GetMethod(nameof(Building.GetGizmos))?.DeclaringType);

        MethodInfo? toggleMethod = typeof(Building_ThermalVent).GetMethod(
            "ToggleFlowMode",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(toggleMethod);
    }

    [Fact]
    public void ThermalVent_OverridesInspectStringForConnectionDiagnostics()
    {
        Assert.Equal(
            typeof(Building_ThermalVent),
            typeof(Building_ThermalVent).GetMethod(nameof(Building.GetInspectString))?.DeclaringType);
    }
}
