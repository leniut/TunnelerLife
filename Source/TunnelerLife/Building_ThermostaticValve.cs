using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Powered thermal valve that opens only when the connected source can move the controlled room toward a target temperature.
/// </summary>
public sealed class Building_ThermostaticValve : Building_ThermalValve
{
    private static readonly Material PoweredLampMaterial =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.25f, 0.95f, 0.18f), false);

    private static readonly Material UnpoweredLampMaterial =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 0.1f, 0.05f), false);

    private bool automaticOpen;
    private CompPowerTrader? powerComp;
    private CompTempControl? tempControlComp;
    private ThermostaticValveMode mode = ThermostaticValveMode.Heating;
    private ThermostaticValveStatus status = ThermostaticValveStatus.Closed;

    public override bool IsOpen => automaticOpen;

    public ThermostaticValveMode Mode => mode;

    public ThermostaticValveStatus Status => status;

    private bool HasPower => powerComp?.PowerOn ?? false;

    private float TargetTemperature => tempControlComp?.targetTemperature ?? 21f;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = GetComp<CompPowerTrader>();
        tempControlComp = GetComp<CompTempControl>();
        EvaluateThermostat();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref automaticOpen, "automaticOpen", false);
        Scribe_Values.Look(ref mode, "thermostaticValveMode", ThermostaticValveMode.Heating);
        Scribe_Values.Look(ref status, "thermostaticValveStatus", ThermostaticValveStatus.Closed);
    }

    public override void TickRare()
    {
        base.TickRare();
        EvaluateThermostat();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        yield return new Command_Action
        {
            defaultLabel = mode == ThermostaticValveMode.Heating
                ? "TunnelerLife_CommandSetThermostaticValveCoolingLabel".Translate()
                : "TunnelerLife_CommandSetThermostaticValveHeatingLabel".Translate(),
            defaultDesc = mode == ThermostaticValveMode.Heating
                ? "TunnelerLife_CommandSetThermostaticValveCoolingDesc".Translate()
                : "TunnelerLife_CommandSetThermostaticValveHeatingDesc".Translate(),
            action = ToggleMode
        };
    }

    public override string GetInspectString()
    {
        StringBuilder builder = new(base.GetInspectString());
        ThermalNetworkSideTemperatures temperatures = Spawned
            ? ThermalNetworkRoomScanner.GetSideTemperatures(this, mode)
            : new ThermalNetworkSideTemperatures(null, null);

        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermostaticValveModeInspect".Translate(ModeLabel));
        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermostaticValveTargetInspect".Translate(FormatTemperature(TargetTemperature)));
        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermostaticValveControlledInspect".Translate(FormatTemperature(temperatures.ControlledTemperature)));
        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermostaticValveSourceInspect".Translate(FormatTemperature(temperatures.SourceTemperature)));
        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermostaticValveStatusInspect".Translate(StatusLabel));

        return builder.ToString();
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        DrawPowerLamp(drawLoc);
    }

    private string ModeLabel => mode == ThermostaticValveMode.Heating
        ? "TunnelerLife_ThermostaticValveModeHeating".Translate()
        : "TunnelerLife_ThermostaticValveModeCooling".Translate();

    private string StatusLabel
    {
        get
        {
            return status switch
            {
                ThermostaticValveStatus.Open => "TunnelerLife_ThermostaticValveStatusOpen".Translate(),
                ThermostaticValveStatus.BlockedNoPower => "TunnelerLife_ThermostaticValveStatusNoPower".Translate(),
                ThermostaticValveStatus.BlockedNoUsefulSource => "TunnelerLife_ThermostaticValveStatusNoUsefulSource".Translate(),
                _ => "TunnelerLife_ThermostaticValveStatusClosed".Translate()
            };
        }
    }

    private void ToggleMode()
    {
        mode = mode == ThermostaticValveMode.Heating
            ? ThermostaticValveMode.Cooling
            : ThermostaticValveMode.Heating;
        EvaluateThermostat();
    }

    private void EvaluateThermostat()
    {
        if (!Spawned)
        {
            return;
        }

        if (!HasPower)
        {
            ApplyDecision(new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoPower));
            return;
        }

        ThermalNetworkSideTemperatures sideTemperatures = ThermalNetworkRoomScanner.GetSideTemperatures(this, mode);
        if (!sideTemperatures.ControlledTemperature.HasValue)
        {
            ApplyDecision(new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoUsefulSource));
            return;
        }

        ApplyDecision(ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                mode,
                TargetTemperature,
                sideTemperatures.ControlledTemperature.Value,
                sideTemperatures.SourceTemperature,
                hasPower: true,
                automaticOpen)));
    }

    private void ApplyDecision(ThermostaticValveDecision decision)
    {
        if (automaticOpen != decision.IsOpen)
        {
            automaticOpen = decision.IsOpen;
            ThermalPipeMeshUtility.DirtyNetworkCellAndNeighbors(Map, Position);
        }

        status = decision.Status;
    }

    private void DrawPowerLamp(Vector3 drawLoc)
    {
        if (!HasPower && (Find.TickManager.TicksGame / 30) % 2 != 0)
        {
            return;
        }

        Vector3 lampPosition = drawLoc;
        lampPosition.x += 0.28f;
        lampPosition.z += 0.28f;
        lampPosition.y = AltitudeLayer.MetaOverlays.AltitudeFor();

        Matrix4x4 matrix = default;
        matrix.SetTRS(lampPosition, Quaternion.identity, new Vector3(0.12f, 1f, 0.12f));
        Graphics.DrawMesh(MeshPool.plane10, matrix, HasPower ? PoweredLampMaterial : UnpoweredLampMaterial, 0);
    }

    private static string FormatTemperature(float? temperature)
    {
        return temperature.HasValue
            ? FormatTemperature(temperature.Value)
            : "TunnelerLife_ThermostaticValveTemperatureUnavailable".Translate();
    }

    private static string FormatTemperature(float temperature)
    {
        return temperature.ToString("0.#", CultureInfo.InvariantCulture) + " C";
    }

    private static void AppendLineIfNeeded(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }
    }
}
