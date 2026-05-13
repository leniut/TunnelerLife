using RimWorld;
using System.Text;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Flickable valve that can open or close thermal transfer through one network cell.
/// </summary>
public class Building_ThermalValve : Building
{
    private CompFlickable? flickableComp;

    public override Graphic Graphic => flickableComp?.CurrentGraphic ?? base.Graphic;

    public virtual bool IsOpen => FlickUtility.WantsToBeOn(this);

    public override string GetInspectString()
    {
        StringBuilder builder = new(base.GetInspectString());
        ThermalNetworkInspectorFormatter.AppendSummaryTo(builder, ThermalNetworkDiagnostics.InspectValve(this));
        return builder.ToString();
    }

    public override System.Collections.Generic.IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        yield return ThermalNetworkInspectorCommand.Create(
            LabelCap,
            () => ThermalNetworkDiagnostics.InspectValve(this));
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        flickableComp = GetComp<CompFlickable>();
        ThermalPipeMeshUtility.DirtyNetworkCellAndNeighbors(map, Position);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        Map map = Map;
        IntVec3 position = Position;
        base.DeSpawn(mode);
        ThermalPipeMeshUtility.DirtyNetworkCellAndNeighbors(map, position);
    }
}
