namespace TunnelerLife;

/// <summary>
/// Pure thermostat decision logic shared by the building and tests.
/// </summary>
public static class ThermostaticValveController
{
    private const float TemperatureTolerance = 2f;

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

        float sourceTemperature = input.SourceTemperature.Value;

        if (IsBelowTarget(input.ControlledTemperature, input.TargetTemperature))
        {
            return IsAboveTarget(sourceTemperature, input.TargetTemperature)
                ? new ThermostaticValveDecision(true, ThermostaticValveStatus.Open)
                : new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoUsefulSource);
        }

        if (IsAboveTarget(input.ControlledTemperature, input.TargetTemperature))
        {
            return IsBelowTarget(sourceTemperature, input.TargetTemperature)
                ? new ThermostaticValveDecision(true, ThermostaticValveStatus.Open)
                : new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoUsefulSource);
        }

        return new ThermostaticValveDecision(false, ThermostaticValveStatus.Closed);
    }

    private static bool IsBelowTarget(float temperature, float targetTemperature)
    {
        return temperature < targetTemperature - TemperatureTolerance;
    }

    private static bool IsAboveTarget(float temperature, float targetTemperature)
    {
        return temperature > targetTemperature + TemperatureTolerance;
    }
}
