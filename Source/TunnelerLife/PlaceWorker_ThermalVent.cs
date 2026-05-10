using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Allows a thermal vent to expose one room to a pipe network on the opposite side.
/// </summary>
public sealed class PlaceWorker_ThermalVent : PlaceWorker
{
    private static readonly Color PipeSideColor = new(1f, 0.85f, 0.05f, 0.8f);

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing? thing = null)
    {
        Map currentMap = Find.CurrentMap;
        IntVec3 roomCell = GetRoomCell(center, rot);
        IntVec3 pipeCell = GetPipeCell(center, rot);

        GenDraw.DrawFieldEdges([roomCell], Color.white);
        GenDraw.DrawFieldEdges([pipeCell], PipeSideColor);

        Room room = roomCell.GetRoom(currentMap);
        if (room != null && !room.UsesOutdoorTemperature)
        {
            GenDraw.DrawFieldEdges(room.Cells.ToList(), Color.white);
        }
    }

    public override AcceptanceReport AllowsPlacing(
        BuildableDef checkingDef,
        IntVec3 loc,
        Rot4 rot,
        Map map,
        Thing? thingToIgnore = null,
        Thing? thing = null)
    {
        IntVec3 roomCell = GetRoomCell(loc, rot);
        IntVec3 pipeCell = GetPipeCell(loc, rot);

        if (!roomCell.InBounds(map) || roomCell.Impassable(map))
        {
            return "TunnelerLife_ThermalVentNeedsRoomSide".Translate();
        }

        if (!ThermalPipeUtility.HasThermalPipeOrBlueprintAt(pipeCell, map))
        {
            return "TunnelerLife_ThermalVentNeedsPipeSide".Translate();
        }

        return true;
    }

    private static IntVec3 GetPipeCell(IntVec3 center, Rot4 rot)
    {
        return center + IntVec3.North.RotatedBy(rot);
    }

    private static IntVec3 GetRoomCell(IntVec3 center, Rot4 rot)
    {
        return center + IntVec3.South.RotatedBy(rot);
    }
}
