using RimWorld;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Prints thermal pipes as yellow cells when the map data overlay is active.
/// </summary>
public sealed class CompThermalPipeOverlay : ThingComp
{
    private static readonly Material OverlayMaterial =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 0.82f, 0.05f, 0.65f));

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        DirtyPowerGridLayer(parent.Position, parent.Map);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        DirtyPowerGridLayer(parent.Position, map);
    }

    public override void CompPrintForPowerGrid(SectionLayer layer)
    {
        Vector3 center = parent.Position.ToVector3Shifted();
        center.y = AltitudeLayer.MapDataOverlay.AltitudeFor();
        Printer_Plane.PrintPlane(layer, center, new Vector2(0.85f, 0.85f), OverlayMaterial);
    }

    private static void DirtyPowerGridLayer(IntVec3 cell, Map? map)
    {
        map?.mapDrawer.MapMeshDirty(cell, MapMeshFlagDefOf.PowerGrid);
    }
}
