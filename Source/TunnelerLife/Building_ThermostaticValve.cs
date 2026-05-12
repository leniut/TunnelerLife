using System.Globalization;
using System.Text;
using RimWorld;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Powered thermal valve that opens only when the connected source can move the controlled room toward a target temperature.
/// </summary>
public sealed class Building_ThermostaticValve : Building_ThermalValve
{
    private const string PoweredGraphicPath = "Things/Building/TunnelerLife/ThermostaticValve_On";
    private const string UnpoweredGraphicPath = "Things/Building/TunnelerLife/ThermostaticValve_Off";

    private bool automaticOpen;
    private IntVec3? activeSourceCell;
    private CompPowerTrader? powerComp;
    private CompTempControl? tempControlComp;
    private ThermostaticValveStatus status = ThermostaticValveStatus.Closed;
    private Graphic? poweredGraphic;
    private Graphic? unpoweredGraphic;

    public override Graphic Graphic => HasPower ? PoweredGraphic : UnpoweredGraphic;

    public override bool IsOpen => automaticOpen;

    public ThermostaticValveStatus Status => status;

    private bool HasPower => powerComp?.PowerOn ?? false;

    private float TargetTemperature => tempControlComp?.targetTemperature ?? 21f;

    private Graphic PoweredGraphic => poweredGraphic ??= CreatePowerStateGraphic(PoweredGraphicPath);

    private Graphic UnpoweredGraphic => unpoweredGraphic ??= CreatePowerStateGraphic(UnpoweredGraphicPath);

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
        Scribe_Values.Look(ref status, "thermostaticValveStatus", ThermostaticValveStatus.Closed);
    }

    public override void TickRare()
    {
        base.TickRare();
        EvaluateThermostat();
    }

    public bool CanConnectToThermalNetworkCell(IntVec3 adjacentCell)
    {
        return automaticOpen
            && activeSourceCell.HasValue
            && activeSourceCell.Value == adjacentCell;
    }

    public override string GetInspectString()
    {
        StringBuilder builder = new(base.GetInspectString());
        ThermalNetworkSideTemperatures temperatures = Spawned
            ? ThermalNetworkRoomScanner.GetSideTemperatures(this, TargetTemperature)
            : new ThermalNetworkSideTemperatures(null, null);

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

    private void EvaluateThermostat()
    {
        if (!Spawned)
        {
            return;
        }

        if (!TunnelerLifeFeatureAvailability.ThermalSystemEnabled)
        {
            ApplyDecision(new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoUsefulSource));
            return;
        }

        if (!HasPower)
        {
            ApplyDecision(new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoPower));
            return;
        }

        ThermalNetworkSideTemperatures sideTemperatures = ThermalNetworkRoomScanner.GetSideTemperatures(
            this,
            TargetTemperature);
        if (!sideTemperatures.ControlledTemperature.HasValue)
        {
            ApplyDecision(new ThermostaticValveDecision(false, ThermostaticValveStatus.BlockedNoUsefulSource));
            return;
        }

        ThermostaticValveDecision decision = ThermostaticValveController.Decide(
            new ThermostaticValveDecisionInput(
                TargetTemperature,
                sideTemperatures.ControlledTemperature.Value,
                sideTemperatures.SourceTemperature,
                hasPower: true,
                temperatureTolerance: TunnelerLifeFeatureAvailability.ThermostatTolerance));
        ApplyDecision(decision, sideTemperatures.SourceCell);
    }

    private void ApplyDecision(ThermostaticValveDecision decision, IntVec3? sourceCell = null)
    {
        IntVec3? nextSourceCell = decision.IsOpen ? sourceCell : null;
        if (automaticOpen != decision.IsOpen || !SameCell(activeSourceCell, nextSourceCell))
        {
            automaticOpen = decision.IsOpen;
            activeSourceCell = nextSourceCell;
            ThermalPipeMeshUtility.DirtyNetworkCellAndNeighbors(Map, Position);
        }

        status = decision.Status;
    }

    private static bool SameCell(IntVec3? left, IntVec3? right)
    {
        return (!left.HasValue && !right.HasValue)
            || (left.HasValue && right.HasValue && left.Value == right.Value);
    }

    private Graphic CreatePowerStateGraphic(string path)
    {
        Graphic baseGraphic = base.Graphic;
        return GraphicDatabase.Get<Graphic_Single>(
            path,
            baseGraphic.Shader,
            def.graphicData.drawSize,
            baseGraphic.Color,
            baseGraphic.ColorTwo,
            def.graphicData,
            null);
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
