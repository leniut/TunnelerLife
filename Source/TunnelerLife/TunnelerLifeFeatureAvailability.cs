using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Centralizes user-configurable feature switches for build menu visibility and runtime guards.
/// </summary>
public static class TunnelerLifeFeatureAvailability
{
    private const string CategoryDefName = "TunnelerLife";
    private const string HiddenThermalPipeDefName = "TunnelerLife_HiddenThermalPipe";
    private const string WaterproofThermalPipeDefName = "TunnelerLife_WaterproofThermalPipe";

    private static readonly HashSet<string> ThermalSystemDefNames = new(StringComparer.Ordinal)
    {
        "TunnelerLife_ThermalPipe",
        HiddenThermalPipeDefName,
        WaterproofThermalPipeDefName,
        "TunnelerLife_ThermalPipeSwitch",
        "TunnelerLife_ThermalVent",
        "TunnelerLife_ThermostaticValve"
    };

    private static readonly FieldInfo? ResolvedDesignatorsField =
        typeof(DesignationCategoryDef).GetField("resolvedDesignators", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? DesignatorBuildEntityDefField =
        typeof(Designator_Build).GetField("entDef", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo? ResolveDesignatorsMethod =
        typeof(DesignationCategoryDef).GetMethod("ResolveDesignators", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static List<Designator>? originalDesignators;
    private static bool warnedAboutDesignatorRefresh;

    public static bool WallRebuildingEnabled => TunnelerLifeMod.Settings.EnableWallRebuilding;

    public static bool ThermalSystemEnabled => TunnelerLifeMod.Settings.EnableThermalSystem;

    public static bool ShowThermalOverlay => TunnelerLifeMod.Settings.ShowThermalOverlay;

    public static bool ShowVentDirectionMarkers => TunnelerLifeMod.Settings.ShowVentDirectionMarkers;

    public static bool ThermalDebugInfoEnabled => TunnelerLifeMod.Settings.EnableThermalDebugInfo;

    public static float ThermostatTolerance => TunnelerLifeMod.Settings.ThermostatTolerance;

    public static float ThermalTransferStrength => TunnelerLifeMod.Settings.ThermalTransferStrength;

    public static AcceptanceReport WallRebuildingDisabledReport =>
        "TunnelerLife_WallRebuildingDisabled".Translate();

    public static AcceptanceReport ThermalSystemDisabledReport =>
        "TunnelerLife_ThermalSystemDisabled".Translate();

    public static AcceptanceReport HiddenThermalPipesDisabledReport =>
        "TunnelerLife_HiddenThermalPipesDisabled".Translate();

    public static AcceptanceReport WaterproofThermalPipesDisabledReport =>
        "TunnelerLife_WaterproofThermalPipesDisabled".Translate();

    public static bool IsWallRebuildingDefName(string? defName)
    {
        return defName != null
            && defName.StartsWith("TunnelerLife_Rockfill_", StringComparison.Ordinal);
    }

    public static bool IsThermalSystemDefName(string? defName)
    {
        return defName != null && ThermalSystemDefNames.Contains(defName);
    }

    public static bool IsBuildableEnabled(BuildableDef? buildableDef, TunnelerLifeSettings settings)
    {
        return buildableDef == null || IsBuildableDefNameEnabled(buildableDef.defName, settings);
    }

    public static bool IsBuildableDefNameEnabled(string? defName, TunnelerLifeSettings settings)
    {
        if (defName == null)
        {
            return true;
        }

        if (!settings.EnableWallRebuilding && IsWallRebuildingDefName(defName))
        {
            return false;
        }

        if (!settings.EnableThermalSystem && IsThermalSystemDefName(defName))
        {
            return false;
        }

        if (!settings.AllowHiddenThermalPipes && defName == HiddenThermalPipeDefName)
        {
            return false;
        }

        if (!settings.AllowWaterproofThermalPipes && defName == WaterproofThermalPipeDefName)
        {
            return false;
        }

        return true;
    }

    public static AcceptanceReport DisabledReportForBuildable(BuildableDef? buildableDef)
    {
        string? defName = buildableDef?.defName;
        if (!ThermalSystemEnabled && IsThermalSystemDefName(defName))
        {
            return ThermalSystemDisabledReport;
        }

        if (!TunnelerLifeMod.Settings.AllowHiddenThermalPipes && defName == HiddenThermalPipeDefName)
        {
            return HiddenThermalPipesDisabledReport;
        }

        if (!TunnelerLifeMod.Settings.AllowWaterproofThermalPipes && defName == WaterproofThermalPipeDefName)
        {
            return WaterproofThermalPipesDisabledReport;
        }

        if (!WallRebuildingEnabled && IsWallRebuildingDefName(defName))
        {
            return WallRebuildingDisabledReport;
        }

        return "TunnelerLife_BuildableDisabled".Translate();
    }

    public static void ApplySettings(TunnelerLifeSettings settings)
    {
        settings.NormalizeValues();
        try
        {
            ApplyDesignatorVisibility(settings);
        }
        catch (Exception exception)
        {
            WarnAboutDesignatorRefresh(exception);
        }
    }

    private static void ApplyDesignatorVisibility(TunnelerLifeSettings settings)
    {
        if (ResolvedDesignatorsField == null)
        {
            return;
        }

        DesignationCategoryDef categoryDef = DefDatabase<DesignationCategoryDef>.GetNamedSilentFail(CategoryDefName);
        if (categoryDef == null)
        {
            return;
        }

        List<Designator>? currentDesignators = ResolvedDesignatorsField.GetValue(categoryDef) as List<Designator>;
        if (currentDesignators == null)
        {
            ResolveDesignatorsMethod?.Invoke(categoryDef, null);
            currentDesignators = ResolvedDesignatorsField.GetValue(categoryDef) as List<Designator>;
        }

        if (currentDesignators == null)
        {
            return;
        }

        originalDesignators ??= currentDesignators.ToList();
        List<Designator> filteredDesignators = originalDesignators
            .Where(designator => IsDesignatorEnabled(designator, settings))
            .ToList();

        ResolvedDesignatorsField.SetValue(categoryDef, filteredDesignators);
    }

    private static bool IsDesignatorEnabled(Designator designator, TunnelerLifeSettings settings)
    {
        if (!settings.EnableWallRebuilding && designator is Designator_Rockfill)
        {
            return false;
        }

        return !TryGetBuildableDef(designator, out BuildableDef? buildableDef)
            || IsBuildableEnabled(buildableDef, settings);
    }

    private static bool TryGetBuildableDef(Designator designator, out BuildableDef? buildableDef)
    {
        buildableDef = null;
        if (designator is not Designator_Build || DesignatorBuildEntityDefField == null)
        {
            return false;
        }

        buildableDef = DesignatorBuildEntityDefField.GetValue(designator) as BuildableDef;
        return buildableDef != null;
    }

    private static void WarnAboutDesignatorRefresh(Exception exception)
    {
        if (warnedAboutDesignatorRefresh)
        {
            return;
        }

        warnedAboutDesignatorRefresh = true;
        Log.Warning($"[TunnelerLife] Could not refresh build menu after settings change: {exception}");
    }
}
