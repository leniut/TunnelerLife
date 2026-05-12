using Verse;

namespace TunnelerLife;

/// <summary>
/// User-configurable feature switches for the Tunneler Life mod.
/// </summary>
public sealed class TunnelerLifeSettings : ModSettings
{
    public const float DefaultThermostatTolerance = 2f;
    public const float MinThermostatTolerance = 0.5f;
    public const float MaxThermostatTolerance = 5f;
    public const float DefaultThermalTransferStrength = 1f;
    public const float MinThermalTransferStrength = 0.25f;
    public const float MaxThermalTransferStrength = 2f;

    public bool EnableWallRebuilding = true;

    public bool EnableThermalSystem = true;

    public bool ShowThermalOverlay = true;

    public bool ShowVentDirectionMarkers = true;

    public bool AllowHiddenThermalPipes = true;

    public bool AllowWaterproofThermalPipes = true;

    public bool EnableThermalDebugInfo;

    public float ThermostatTolerance = DefaultThermostatTolerance;

    public float ThermalTransferStrength = DefaultThermalTransferStrength;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref EnableWallRebuilding, "enableWallRebuilding", true);
        Scribe_Values.Look(ref EnableThermalSystem, "enableThermalSystem", true);
        Scribe_Values.Look(ref ShowThermalOverlay, "showThermalOverlay", true);
        Scribe_Values.Look(ref ShowVentDirectionMarkers, "showVentDirectionMarkers", true);
        Scribe_Values.Look(ref AllowHiddenThermalPipes, "allowHiddenThermalPipes", true);
        Scribe_Values.Look(ref AllowWaterproofThermalPipes, "allowWaterproofThermalPipes", true);
        Scribe_Values.Look(ref EnableThermalDebugInfo, "enableThermalDebugInfo", false);
        Scribe_Values.Look(ref ThermostatTolerance, "thermostatTolerance", DefaultThermostatTolerance);
        Scribe_Values.Look(ref ThermalTransferStrength, "thermalTransferStrength", DefaultThermalTransferStrength);
        NormalizeValues();
    }

    public void NormalizeValues()
    {
        ThermostatTolerance = Clamp(ThermostatTolerance, MinThermostatTolerance, MaxThermostatTolerance);
        ThermalTransferStrength = Clamp(
            ThermalTransferStrength,
            MinThermalTransferStrength,
            MaxThermalTransferStrength);
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }
}
