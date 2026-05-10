using System.Text;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Wall vent that exposes one adjacent room to a connected thermal pipe network.
/// </summary>
public sealed class Building_ThermalVent : Building
{
    private CompFlickable? flickableComp;

    public override Graphic Graphic => flickableComp?.CurrentGraphic ?? base.Graphic;

    private static readonly IntVec3[] CardinalDirections =
    [
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    ];

    public IntVec3 OutletCell => Position + IntVec3.South.RotatedBy(Rotation);

    public bool IsOpen => FlickUtility.WantsToBeOn(this);

    public IEnumerable<IntVec3> AdjacentPipeCells
    {
        get
        {
            if (!Spawned)
            {
                yield break;
            }

            foreach (IntVec3 direction in CardinalDirections)
            {
                IntVec3 cell = Position + direction;
                if (ThermalPipeUtility.HasThermalPipeAt(cell, Map))
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
            if (!Spawned || !OutletCell.InBounds(Map) || OutletCell.Impassable(Map))
            {
                return null;
            }

            return OutletCell.GetRoom(Map);
        }
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

    public override string GetInspectString()
    {
        StringBuilder builder = new();
        builder.Append(base.GetInspectString());

        if (!IsOpen)
        {
            AppendLineIfNeeded(builder);
            builder.Append("VentClosed".Translate());
        }

        return builder.ToString();
    }

    public bool ConnectsToPipeCell(IntVec3 pipeCell)
    {
        foreach (IntVec3 direction in CardinalDirections)
        {
            if (Position + direction == pipeCell)
            {
                return true;
            }
        }

        return false;
    }

    private static void AppendLineIfNeeded(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }
    }
}
