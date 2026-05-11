using System;

namespace TunnelerLife;

/// <summary>
/// Pure thermostat decision logic shared by the building and tests.
/// </summary>
public static class ThermostaticValveController
{
    public const float HysteresisDegrees = 1f;

    public static ThermostaticValveDecision Decide(ThermostaticValveDecisionInput input)
    {
        if (!input.HasPower)
        {
            return new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoPower);
        }

        if (!input.SourceTemperature.HasValue)
        {
            return new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoUsefulSource);
        }

        if (Math.Abs(input.ControlledTemperature - input.TargetTemperature) <= HysteresisDegrees)
        {
            return new ThermostaticValveDecision(
                input.PreviousIsOpen,
                input.PreviousIsOpen ? ThermostaticValveStatus.Open : ThermostaticValveStatus.Closed);
        }

        return input.Mode == ThermostaticValveMode.Heating
            ? DecideHeating(input)
            : DecideCooling(input);
    }

    private static ThermostaticValveDecision DecideHeating(ThermostaticValveDecisionInput input)
    {
        if (input.ControlledTemperature >= input.TargetTemperature)
        {
            return new ThermostaticValveDecision(false, ThermostaticValveStatus.Closed);
        }

        if (input.SourceTemperature > input.ControlledTemperature)
        {
            return new ThermostaticValveDecision(true, ThermostaticValveStatus.Open);
        }

        return new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoUsefulSource);
    }

    private static ThermostaticValveDecision DecideCooling(ThermostaticValveDecisionInput input)
    {
        if (input.ControlledTemperature <= input.TargetTemperature)
        {
            return new ThermostaticValveDecision(false, ThermostaticValveStatus.Closed);
        }

        if (input.SourceTemperature < input.ControlledTemperature)
        {
            return new ThermostaticValveDecision(true, ThermostaticValveStatus.Open);
        }

        return new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoUsefulSource);
    }
}
