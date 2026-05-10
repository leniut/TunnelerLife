using System.Text;
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

    public IntVec3 PipeCell => Position + IntVec3.North.RotatedBy(Rotation);

    public IntVec3 RoomCell => Position + IntVec3.South.RotatedBy(Rotation);

    public bool IsOpen => FlickUtility.WantsToBeOn(this);

    public Room? ConnectedRoom
    {
        get
        {
            if (!Spawned || !RoomCell.InBounds(Map))
            {
                return null;
            }

            return RoomCell.GetRoom(Map);
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
        return PipeCell == pipeCell;
    }

    private static void AppendLineIfNeeded(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }
    }
}
