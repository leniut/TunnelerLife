using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Linked thermal pipe graphic that draws pipe stubs into adjacent valve cells.
/// </summary>
public sealed class Graphic_LinkedThermalPipe : Graphic_Linked
{
    public Graphic_LinkedThermalPipe()
    {
    }

    public Graphic_LinkedThermalPipe(Graphic subGraphic)
        : base(subGraphic)
    {
    }

    public override void Init(GraphicRequest req)
    {
        subGraphic = GraphicDatabase.Get<Graphic_Single>(
            req.path,
            req.shader,
            req.drawSize,
            req.color,
            req.colorTwo,
            req.graphicData,
            req.maskPath);
        data = subGraphic.data;
        path = subGraphic.path;
        maskPath = subGraphic.maskPath;
        color = subGraphic.color;
        colorTwo = subGraphic.colorTwo;
        drawSize = subGraphic.drawSize;
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new Graphic_LinkedThermalPipe(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
        {
            data = data
        };
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        if (!parent.Spawned)
        {
            return false;
        }

        if (!c.InBounds(parent.Map))
        {
            return (parent.def.graphicData.linkFlags & LinkFlags.MapEdge) != 0;
        }

        return ThermalPipeUtility.HasThermalNetworkBuildingAt(c, parent.Map) || base.ShouldLinkWith(c, parent);
    }

    public override void Print(SectionLayer layer, Thing thing, float extraRotation)
    {
        base.Print(layer, thing, extraRotation);
        for (int index = 0; index < GenAdj.CardinalDirections.Length; index++)
        {
            IntVec3 adjacentCell = thing.Position + GenAdj.CardinalDirections[index];
            if (!adjacentCell.InBounds(thing.Map) || !HasUnlinkedThermalValve(adjacentCell, thing.Map))
            {
                continue;
            }

            Material material = LinkedDrawMatFrom(thing, adjacentCell);
            Printer_Plane.PrintPlane(
                layer,
                adjacentCell.ToVector3ShiftedWithAltitude(thing.def.Altitude),
                Vector2.one,
                material,
                extraRotation);
        }
    }

    private static bool HasUnlinkedThermalValve(IntVec3 cell, Map map)
    {
        List<Thing> things = map.thingGrid.ThingsListAt(cell);
        for (int index = 0; index < things.Count; index++)
        {
            ThingDef thingDef = things[index].def;
            if (ThermalPipeUtility.IsThermalValve(thingDef) && !(thingDef.graphicData?.Linked ?? false))
            {
                return true;
            }
        }

        return false;
    }
}
