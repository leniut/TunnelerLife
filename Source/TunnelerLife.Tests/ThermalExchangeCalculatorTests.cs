using Xunit;
using TunnelerLife;

namespace TunnelerLife.Tests;

public sealed class ThermalExchangeCalculatorTests
{
    [Fact]
    public void CalculateTemperatureDeltas_MovesIndoorRoomsTowardAverage()
    {
        ThermalRoomState[] rooms =
        [
            new(40f, 100, usesOutdoorTemperature: false),
            new(0f, 100, usesOutdoorTemperature: false)
        ];

        float[] deltas = ThermalExchangeCalculator.CalculateTemperatureDeltas(rooms, rate: 14f, inVacuum: false);

        Assert.Equal(-2.8f, deltas[0], precision: 3);
        Assert.Equal(2.8f, deltas[1], precision: 3);
    }

    [Fact]
    public void CalculateTemperatureDeltas_WeightsRoomsByExchangePortCount()
    {
        ThermalRoomState[] rooms =
        [
            new(40f, 100, usesOutdoorTemperature: false, exchangePortCount: 1),
            new(0f, 100, usesOutdoorTemperature: false, exchangePortCount: 3)
        ];

        float[] deltas = ThermalExchangeCalculator.CalculateTemperatureDeltas(rooms, rate: 14f, inVacuum: false);

        Assert.Equal(-4.2f, deltas[0], precision: 3);
        Assert.Equal(4.2f, deltas[1], precision: 3);
    }

    [Fact]
    public void CalculateTemperatureDeltas_DoesNotModifyOutdoorRooms()
    {
        ThermalRoomState[] rooms =
        [
            new(40f, 100, usesOutdoorTemperature: true),
            new(0f, 100, usesOutdoorTemperature: false)
        ];

        float[] deltas = ThermalExchangeCalculator.CalculateTemperatureDeltas(rooms, rate: 14f, inVacuum: false);

        Assert.Equal(0f, deltas[0], precision: 3);
        Assert.Equal(2.8f, deltas[1], precision: 3);
    }

    [Fact]
    public void CalculateTemperatureDeltas_ReturnsZeroDeltasForSingleRoom()
    {
        ThermalRoomState[] rooms =
        [
            new(10f, 100, usesOutdoorTemperature: false)
        ];

        float[] deltas = ThermalExchangeCalculator.CalculateTemperatureDeltas(rooms, rate: 14f, inVacuum: false);

        Assert.Equal([0f], deltas);
    }

    [Fact]
    public void CalculateTemperatureDeltas_NeverCrossesAverage()
    {
        ThermalRoomState[] rooms =
        [
            new(40f, 1, usesOutdoorTemperature: false),
            new(0f, 1, usesOutdoorTemperature: false)
        ];

        float[] deltas = ThermalExchangeCalculator.CalculateTemperatureDeltas(rooms, rate: 14f, inVacuum: false);

        Assert.Equal(-20f, deltas[0], precision: 3);
        Assert.Equal(20f, deltas[1], precision: 3);
    }
}
