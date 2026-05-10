using System.Collections.Generic;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Shared helpers for recognizing thermal pipe buildings and construction plans.
/// </summary>
public static class ThermalPipeUtility
{
    public static bool IsThermalPipe(ThingDef? thingDef)
    {
        return thingDef?.thingClass != null
            && typeof(Building_ThermalPipe).IsAssignableFrom(thingDef.thingClass);
    }

    public static bool HasThermalPipeOrBlueprintAt(IntVec3 cell, Map map)
    {
        if (!cell.InBounds(map))
        {
            return false;
        }

        List<Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            ThingDef thingDef = things[i].def;
            if (IsThermalPipe(thingDef) || IsThermalPipe(thingDef.entityDefToBuild as ThingDef))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasThermalPipeAt(IntVec3 cell, Map map)
    {
        if (!cell.InBounds(map))
        {
            return false;
        }

        List<Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            if (IsThermalPipe(things[i].def))
            {
                return true;
            }
        }

        return false;
    }
}
