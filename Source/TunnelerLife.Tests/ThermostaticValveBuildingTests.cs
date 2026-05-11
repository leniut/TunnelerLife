using System.Reflection;
using TunnelerLife;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermostaticValveBuildingTests
{
    [Fact]
    public void ThermostaticValve_InheritsThermalValveAndControlsOpenStateAutomatically()
    {
        Assert.True(typeof(Building_ThermostaticValve).IsSubclassOf(typeof(Building_ThermalValve)));
        Assert.True(
            typeof(Building_ThermalValve)
                .GetProperty(nameof(Building_ThermalValve.IsOpen))
                ?.GetMethod
                ?.IsVirtual);
        MethodInfo? thermostatIsOpenGetter = typeof(Building_ThermostaticValve)
            .GetProperty(nameof(Building_ThermalValve.IsOpen))
            ?.GetMethod;

        Assert.Equal(typeof(Building_ThermostaticValve), thermostatIsOpenGetter?.DeclaringType);
        Assert.Equal(typeof(Building_ThermalValve), thermostatIsOpenGetter?.GetBaseDefinition().DeclaringType);
    }

    [Fact]
    public void ThermostaticValve_StoresModeAndRunsRareTick()
    {
        Assert.Equal(
            typeof(Building_ThermostaticValve),
            typeof(Building_ThermostaticValve).GetMethod(nameof(Building.TickRare))?.DeclaringType);
        Assert.Equal(
            typeof(Building_ThermostaticValve),
            typeof(Building_ThermostaticValve).GetMethod(nameof(Building.ExposeData))?.DeclaringType);
        Assert.Equal(
            typeof(Building_ThermostaticValve),
            typeof(Building_ThermostaticValve)
                .GetMethod(nameof(Building.GetGizmos), BindingFlags.Instance | BindingFlags.Public)
                ?.DeclaringType);
    }
}
