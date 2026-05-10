using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Draws only Tunneler Life thermal pipes while the Tunneler Life architect category is open.
/// </summary>
public sealed class ThermalPipeOverlayMapComponent : MapComponent
{
    private const string TunnelerLifeCategoryDefName = "TunnelerLife";

    private static readonly Material PipeOverlayMaterial = MaterialPool.MatFrom(
        BaseContent.WhiteTex,
        ShaderDatabase.Transparent,
        new Color(1f, 0.82f, 0.05f, 0.58f),
        3600);

    private readonly HashSet<IntVec3> drawnCells = [];

    public ThermalPipeOverlayMapComponent(Map map)
        : base(map)
    {
    }

    public override void MapComponentDraw()
    {
        if (!ShouldDrawThermalPipeOverlay())
        {
            return;
        }

        drawnCells.Clear();
        foreach (Thing thing in map.listerThings.AllThings)
        {
            ThingDef? buildDef = thing.def.entityDefToBuild as ThingDef;
            if ((ThermalPipeUtility.IsThermalPipe(thing.def) || ThermalPipeUtility.IsThermalPipe(buildDef))
                && drawnCells.Add(thing.Position))
            {
                DrawPipeCell(thing.Position);
            }
        }
    }

    private static bool ShouldDrawThermalPipeOverlay()
    {
        MainTabWindow_Architect architectWindow = Find.WindowStack.WindowOfType<MainTabWindow_Architect>();
        return architectWindow?.selectedDesPanel?.def?.defName == TunnelerLifeCategoryDefName;
    }

    private static void DrawPipeCell(IntVec3 cell)
    {
        Vector3 position = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay);
        Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, PipeOverlayMaterial, 0);
    }
}
