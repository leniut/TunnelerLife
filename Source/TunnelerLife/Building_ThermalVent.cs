using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RimWorld;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Wall vent that exposes one adjacent room to a connected thermal pipe network.
/// </summary>
public sealed class Building_ThermalVent : Building
{
    private static readonly IntVec3[] CardinalDirections =
    [
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    ];

    private CompFlickable? flickableComp;
    private ThermalVentFlowMode flowMode;

    public override Graphic Graphic => flickableComp?.CurrentGraphic ?? base.Graphic;

    private IntVec3 OutletDirection => IntVec3.South.RotatedBy(Rotation);

    public IntVec3 OutletCell => Position + OutletDirection;

    public bool IsOpen => FlickUtility.WantsToBeOn(this);

    public ThermalVentFlowMode FlowMode => flowMode;

    public IEnumerable<IntVec3> PipeCells
    {
        get
        {
            foreach (IntVec3 direction in CardinalDirections)
            {
                if (direction != OutletDirection)
                {
                    yield return Position + direction;
                }
            }
        }
    }

    public IEnumerable<IntVec3> AdjacentPipeCells
    {
        get
        {
            if (!Spawned)
            {
                yield break;
            }

            foreach (IntVec3 cell in PipeCells)
            {
                if (ThermalPipeUtility.HasOpenThermalNetworkCellAt(cell, Map))
                {
                    yield return cell;
                }
            }
        }
    }

    public Room? ConnectedRoom
    {
        get
        {
            if (!Spawned || !OutletCell.InBounds(Map))
            {
                return null;
            }

        return OutletCell.GetRoom(Map);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref flowMode, "thermalVentFlowMode", ThermalVentFlowMode.PullFromAirSide);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        flickableComp = GetComp<CompFlickable>();
    }

    public override void TickRare()
    {
        if (IsOpen)
        {
            ThermalVentNetworkService.TryEqualizeNetwork(this);
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        yield return new Command_Action
        {
            defaultLabel = FlowModeLabel,
            defaultDesc = "TunnelerLife_CommandToggleThermalVentFlowDesc".Translate(),
            action = ToggleFlowMode
        };
    }

    public override string GetInspectString()
    {
        StringBuilder builder = new();
        builder.Append(base.GetInspectString());

        if (!IsOpen)
        {
            AppendLineIfNeeded(builder);
            builder.Append("VentClosed".Translate());
        }

        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermalVentFlowModeInspect".Translate(FlowModeLabel));
        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermalVentAirSideInspect".Translate(
            DirectionLabel(OutletDirection),
            RoomConnectionLabel()));
        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermalVentPipeSidesInspect".Translate(
            PipeSideLabels(),
            AdjacentPipeCells.Count().ToString(CultureInfo.InvariantCulture)));

        return builder.ToString();
    }

    public bool ConnectsToPipeCell(IntVec3 pipeCell)
    {
        return PipeCells.Contains(pipeCell);
    }

    private void ToggleFlowMode()
    {
        flowMode = flowMode == ThermalVentFlowMode.PullFromAirSide
            ? ThermalVentFlowMode.PushToAirSide
            : ThermalVentFlowMode.PullFromAirSide;
    }

    private string FlowModeLabel
    {
        get
        {
            return flowMode == ThermalVentFlowMode.PullFromAirSide
                ? "TunnelerLife_ThermalVentFlowPull".Translate()
                : "TunnelerLife_ThermalVentFlowPush".Translate();
        }
    }

    private string RoomConnectionLabel()
    {
        Room? room = ConnectedRoom;
        return room != null
            ? "TunnelerLife_ThermalVentRoomConnected".Translate(FormatTemperature(room.Temperature))
            : "TunnelerLife_ThermalVentRoomMissing".Translate();
    }

    private static string DirectionLabel(IntVec3 direction)
    {
        if (direction == IntVec3.North)
        {
            return "TunnelerLife_DirectionNorth".Translate();
        }

        if (direction == IntVec3.East)
        {
            return "TunnelerLife_DirectionEast".Translate();
        }

        if (direction == IntVec3.South)
        {
            return "TunnelerLife_DirectionSouth".Translate();
        }

        return "TunnelerLife_DirectionWest".Translate();
    }

    private static string FormatTemperature(float temperature)
    {
        return temperature.ToString("0.#", CultureInfo.InvariantCulture) + " C";
    }

    private string PipeSideLabels()
    {
        return string.Join(", ", PipeCells.Select(cell => DirectionLabel(cell - Position)));
    }

    private static void AppendLineIfNeeded(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }
    }
}
