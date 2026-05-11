using TunnelerLife;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalPipeUtilityTests
{
    [Fact]
    public void IsThermalNetworkThingClass_ReturnsTrueForPipesAndSwitches()
    {
        Assert.True(ThermalPipeUtility.IsThermalNetworkThingClass(typeof(Building_ThermalPipe)));
        Assert.True(ThermalPipeUtility.IsThermalNetworkThingClass(typeof(Building_ThermalValve)));
        Assert.True(ThermalPipeUtility.IsThermalNetworkThingClass(typeof(Building_ThermalPipeSwitch)));
        Assert.False(ThermalPipeUtility.IsThermalNetworkThingClass(typeof(Building_ThermalVent)));
    }

    [Fact]
    public void ThermalNetworkBuildings_DirtyAdjacentMeshForCustomLinkedPipeRendering()
    {
        Assert.Equal(
            typeof(Building_ThermalPipe),
            typeof(Building_ThermalPipe).GetMethod(nameof(Building_ThermalPipe.SpawnSetup))?.DeclaringType);
        Assert.Equal(
            typeof(Building_ThermalPipe),
            typeof(Building_ThermalPipe).GetMethod(nameof(Building_ThermalPipe.DeSpawn))?.DeclaringType);
        Assert.Equal(
            typeof(Building_ThermalValve),
            typeof(Building_ThermalValve).GetMethod(nameof(Building_ThermalValve.SpawnSetup))?.DeclaringType);
        Assert.Equal(
            typeof(Building_ThermalValve),
            typeof(Building_ThermalValve).GetMethod(nameof(Building_ThermalValve.DeSpawn))?.DeclaringType);
    }

    [Fact]
    public void ThermalPipeGraphic_UsesThingGridInsteadOfVanillaLinkGridForPipeConnections()
    {
        Assert.Equal(
            typeof(Graphic_LinkedThermalPipe),
            typeof(Graphic_LinkedThermalPipe).GetMethod(nameof(Graphic_LinkedThermalPipe.ShouldLinkWith))?.DeclaringType);
        Assert.True(ThermalPipeUtility.IsThermalNetworkThingClass(typeof(Building_ThermalPipe)));
        Assert.True(ThermalPipeUtility.IsThermalNetworkThingClass(typeof(Building_ThermalValve)));
    }
}
