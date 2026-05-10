using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Shows the outlet direction and nearby thermal pipes without restricting where thermal vents can be placed.
/// </summary>
public sealed class PlaceWorker_ThermalVent : PlaceWorker
{
    private static readonly Color PipeSideColor = new(1f, 0.85f, 0.05f, 0.8f);
    private static readonly Color OutletColor = new(0.2f, 0.85f, 1f, 0.8f);

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing? thing = null)
    {
        Map currentMap = Find.CurrentMap;
        List<IntVec3> pipeCells = GetAdjacentPipeCells(center, currentMap);
        if (pipeCells.Count > 0)
        {
            GenDraw.DrawFieldEdges(pipeCells, PipeSideColor);
        }

        GenDraw.DrawFieldEdges([GetOutletCell(center, rot)], OutletColor);
    }

    public override AcceptanceReport AllowsPlacing(
        BuildableDef checkingDef,
        IntVec3 loc,
        Rot4 rot,
        Map map,
        Thing? thingToIgnore = null,
        Thing? thing = null)
    {
        return true;
    }

    private static List<IntVec3> GetAdjacentPipeCells(IntVec3 center, Map map)
    {
        List<IntVec3> pipeCells = [];
        IntVec3[] directions =
        [
            IntVec3.North,
            IntVec3.East,
            IntVec3.South,
            IntVec3.West
        ];

        foreach (IntVec3 direction in directions)
        {
            IntVec3 cell = center + direction;
            if (ThermalPipeUtility.HasThermalPipeOrBlueprintAt(cell, map))
            {
                pipeCells.Add(cell);
            }
        }

        return pipeCells;
    }

    private static IntVec3 GetOutletCell(IntVec3 center, Rot4 rot)
    {
        return center + IntVec3.South.RotatedBy(rot);
    }
}
