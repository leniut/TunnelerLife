using RimWorld;
using System.Text;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Marker building for cells that belong to a thermal pipe network.
/// </summary>
public sealed class Building_ThermalPipe : Building
{
    public override string GetInspectString()
    {
        StringBuilder builder = new();
        builder.Append(base.GetInspectString());

        ThermalNetworkInspectorFormatter.AppendSummaryTo(builder, ThermalNetworkDiagnostics.InspectPipe(this));

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
            () => ThermalNetworkDiagnostics.InspectPipe(this));
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
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

internal static class ThermalPipeMeshUtility
{
    public static void DirtyNetworkCellAndNeighbors(Map map, IntVec3 position)
    {
        if (map == null)
        {
            return;
        }

        map.mapDrawer.MapMeshDirty(position, MapMeshFlagDefOf.Things, regenAdjacentCells: true, regenAdjacentSections: false);
    }
}
