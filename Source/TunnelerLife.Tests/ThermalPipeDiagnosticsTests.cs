using TunnelerLife;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalPipeDiagnosticsTests
{
    [Fact]
    public void AverageNetworkTemperature_ReturnsMeanTemperature()
    {
        Assert.Equal(20f, ThermalNetworkDiagnostics.AverageNetworkTemperature([10f, 20f, 30f]));
    }

    [Fact]
    public void AverageNetworkTemperature_ReturnsNullWhenNoTemperatureIsAvailable()
    {
        Assert.Null(ThermalNetworkDiagnostics.AverageNetworkTemperature([]));
    }

    [Fact]
    public void ThermalPipe_OverridesInspectStringForNetworkDiagnostics()
    {
        Assert.Equal(
            typeof(Building_ThermalPipe),
            typeof(Building_ThermalPipe).GetMethod(nameof(Building_ThermalPipe.GetInspectString))?.DeclaringType);
    }
}
