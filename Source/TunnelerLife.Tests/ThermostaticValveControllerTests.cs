using TunnelerLife;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermostaticValveControllerTests
{
    [Theory]
    [InlineData(21f, 18f, 24f)]
    [InlineData(21f, 24f, 18f)]
    public void Decide_OpensWhenSourceMovesControlledRoomTowardTarget(
        float targetTemperature,
        float controlledTemperature,
        float sourceTemperature)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                targetTemperature,
                controlledTemperature,
                sourceTemperature,
                hasPower: true));

        Assert.True(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.Open, decision.Status);
    }

    [Theory]
    [InlineData(21f, 18f, 15f)]
    [InlineData(21f, 10f, 15f)]
    [InlineData(21f, 24f, 30f)]
    [InlineData(21f, 50.8f, 43.8f)]
    public void Decide_ClosesWhenSourceIsOnWrongSideOfTarget(
        float targetTemperature,
        float controlledTemperature,
        float sourceTemperature)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                targetTemperature,
                controlledTemperature,
                sourceTemperature,
                hasPower: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.BlockedNoUsefulSource, decision.Status);
    }

    [Theory]
    [InlineData(21f, 10f)]
    [InlineData(21f, 50.8f)]
    public void Decide_ClosesWhenSourceEqualsTarget(
        float targetTemperature,
        float controlledTemperature)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                targetTemperature,
                controlledTemperature,
                sourceTemperature: targetTemperature,
                hasPower: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.BlockedNoUsefulSource, decision.Status);
    }

    [Theory]
    [InlineData(21f, 19f, 30f)]
    [InlineData(21f, 20.6f, 30f)]
    [InlineData(21f, 21.4f, 10f)]
    [InlineData(21f, 23f, 10f)]
    public void Decide_ClosesWhenControlledRoomIsWithinTemperatureTolerance(
        float targetTemperature,
        float controlledTemperature,
        float sourceTemperature)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                targetTemperature,
                controlledTemperature,
                sourceTemperature,
                hasPower: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.Closed, decision.Status);
    }

    [Theory]
    [InlineData(21f, 10f, 21.4f)]
    [InlineData(21f, 10f, 23f)]
    [InlineData(21f, 50.8f, 19f)]
    [InlineData(21f, 50.8f, 20.6f)]
    public void Decide_ClosesWhenSourceIsWithinTemperatureToleranceOfTarget(
        float targetTemperature,
        float controlledTemperature,
        float sourceTemperature)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                targetTemperature,
                controlledTemperature,
                sourceTemperature,
                hasPower: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.BlockedNoUsefulSource, decision.Status);
    }

    [Theory]
    [InlineData(21f, 15f)]
    [InlineData(21f, 21f)]
    [InlineData(21f, 30f)]
    public void Decide_ClosesWhenControlledRoomIsAtTarget(
        float targetTemperature,
        float sourceTemperature)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                targetTemperature,
                controlledTemperature: targetTemperature,
                sourceTemperature: sourceTemperature,
                hasPower: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.Closed, decision.Status);
    }

    [Fact]
    public void Decide_ClosesWhenUnpowered()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                targetTemperature: 21f,
                controlledTemperature: 10f,
                sourceTemperature: 30f,
                hasPower: false));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.BlockedNoPower, decision.Status);
    }

    [Fact]
    public void Decide_ClosesWhenNoSourceTemperatureIsAvailable()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                targetTemperature: 21f,
                controlledTemperature: 30f,
                sourceTemperature: null,
                hasPower: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.BlockedNoUsefulSource, decision.Status);
    }

    [Fact]
    public void Decide_UsesInputTemperatureTolerance()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                targetTemperature: 21f,
                controlledTemperature: 19.5f,
                sourceTemperature: 30f,
                hasPower: true,
                temperatureTolerance: 1f));

        Assert.True(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.Open, decision.Status);
    }
}
