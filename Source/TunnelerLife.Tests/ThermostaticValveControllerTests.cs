using TunnelerLife;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermostaticValveControllerTests
{
    [Theory]
    [InlineData(ThermostaticValveMode.Heating, 21f, 19f, 25f, true)]
    [InlineData(ThermostaticValveMode.Cooling, 21f, 24f, 15f, true)]
    public void Decide_OpensOnlyWhenSourceMovesControlledRoomTowardTarget(
        ThermostaticValveMode mode,
        float targetTemperature,
        float controlledTemperature,
        float sourceTemperature,
        bool expectedOpen)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                mode,
                targetTemperature,
                controlledTemperature,
                sourceTemperature,
                hasPower: true,
                previousIsOpen: false));

        Assert.Equal(expectedOpen, decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.Open, decision.Status);
    }

    [Theory]
    [InlineData(ThermostaticValveMode.Heating, 21f, 19f, 15f)]
    [InlineData(ThermostaticValveMode.Cooling, 21f, 24f, 30f)]
    public void Decide_ClosesWhenSourceWouldPushControlledRoomAwayFromTarget(
        ThermostaticValveMode mode,
        float targetTemperature,
        float controlledTemperature,
        float sourceTemperature)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                mode,
                targetTemperature,
                controlledTemperature,
                sourceTemperature,
                hasPower: true,
                previousIsOpen: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.BlockedNoUsefulSource, decision.Status);
    }

    [Theory]
    [InlineData(ThermostaticValveMode.Heating, 21f, 20.5f, 25f, true)]
    [InlineData(ThermostaticValveMode.Heating, 21f, 20.5f, 25f, false)]
    [InlineData(ThermostaticValveMode.Cooling, 21f, 21.5f, 15f, true)]
    [InlineData(ThermostaticValveMode.Cooling, 21f, 21.5f, 15f, false)]
    public void Decide_KeepsPreviousStateInsideOneDegreeHysteresisBand(
        ThermostaticValveMode mode,
        float targetTemperature,
        float controlledTemperature,
        float sourceTemperature,
        bool previousIsOpen)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                mode,
                targetTemperature,
                controlledTemperature,
                sourceTemperature,
                hasPower: true,
                previousIsOpen));

        Assert.Equal(previousIsOpen, decision.IsOpen);
        Assert.Equal(
            previousIsOpen ? ThermostaticValveStatus.Open : ThermostaticValveStatus.Closed,
            decision.Status);
    }

    [Fact]
    public void Decide_ClosesWhenUnpowered()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                ThermostaticValveMode.Heating,
                targetTemperature: 21f,
                controlledTemperature: 10f,
                sourceTemperature: 30f,
                hasPower: false,
                previousIsOpen: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.BlockedNoPower, decision.Status);
    }

    [Fact]
    public void Decide_ClosesWhenNoSourceTemperatureIsAvailable()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                ThermostaticValveMode.Cooling,
                targetTemperature: 21f,
                controlledTemperature: 30f,
                sourceTemperature: null,
                hasPower: true,
                previousIsOpen: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.BlockedNoUsefulSource, decision.Status);
    }
}
