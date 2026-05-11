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

    public static bool IsThermalValve(ThingDef? thingDef)
    {
        return thingDef?.thingClass != null
            && typeof(Building_ThermalValve).IsAssignableFrom(thingDef.thingClass);
    }

    public static bool IsThermalNetworkBuildable(ThingDef? thingDef)
    {
        return IsThermalPipe(thingDef) || IsThermalValve(thingDef);
    }

    public static bool IsThermalNetworkThingClass(System.Type? thingClass)
    {
        return thingClass != null
            && (typeof(Building_ThermalPipe).IsAssignableFrom(thingClass)
                || typeof(Building_ThermalValve).IsAssignableFrom(thingClass));
    }

    public static bool HasThermalPipeOrBlueprintAt(IntVec3 cell, Map map)
    {
        return HasThermalNetworkBuildableOrBlueprintAt(cell, map);
    }

    public static bool HasThermalNetworkBuildableOrBlueprintAt(IntVec3 cell, Map map)
    {
        if (!cell.InBounds(map))
        {
            return false;
        }

        List<Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            ThingDef thingDef = things[i].def;
            if (IsThermalNetworkBuildable(thingDef)
                || IsThermalNetworkBuildable(thingDef.entityDefToBuild as ThingDef))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasOpenThermalNetworkCellAt(IntVec3 cell, Map map)
    {
        if (!cell.InBounds(map))
        {
            return false;
        }

        List<Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            if (IsActiveThermalNetworkThing(things[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsActiveThermalNetworkThing(Thing thing)
    {
        if (IsThermalPipe(thing.def))
        {
            return true;
        }

        return thing is Building_ThermalValve thermalValve && thermalValve.IsOpen;
    }

    public static bool IsPlannedThermalNetworkThing(Thing thing)
    {
        return IsThermalNetworkBuildable(thing.def.entityDefToBuild as ThingDef);
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

    public static bool HasThermalNetworkBuildingAt(IntVec3 cell, Map map)
    {
        if (!cell.InBounds(map))
        {
            return false;
        }

        List<Thing> things = cell.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            if (IsThermalNetworkBuildable(things[i].def))
            {
                return true;
            }
        }

        return false;
    }
}
