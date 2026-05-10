using RimWorld;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Prevents duplicate thermal pipes without blocking vanilla power conduits on the same cell.
/// </summary>
public sealed class PlaceWorker_ThermalPipe : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(
        BuildableDef checkingDef,
        IntVec3 loc,
        Rot4 rot,
        Map map,
        Thing? thingToIgnore = null,
        Thing? thing = null)
    {
        foreach (Thing existingThing in loc.GetThingList(map))
        {
            if (existingThing == thingToIgnore)
            {
                continue;
            }

            if (ThermalPipeUtility.IsThermalPipe(existingThing.def)
                || ThermalPipeUtility.IsThermalPipe(existingThing.def.entityDefToBuild as ThingDef))
            {
                return "TunnelerLife_ThermalPipeAlreadyHere".Translate();
            }
        }

        return true;
    }
}
