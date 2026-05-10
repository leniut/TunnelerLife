using RimWorld;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Temporary completed rockfill building that converts itself into natural rough stone when spawned.
/// </summary>
public sealed class Building_Rockfill : Building
{
    /// <summary>
    /// Converts newly completed rockfill into the natural rough rock matching its selected stone block stuff.
    /// </summary>
    /// <param name="map">The map where this building was spawned.</param>
    /// <param name="respawningAfterLoad">Whether this spawn is part of loading an existing save.</param>
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        if (respawningAfterLoad)
        {
            return;
        }

        string? rockDefName = RockfillMaterialResolver.ResolveRockDefNameFromRockfillDefName(def.defName);
        if (rockDefName == null)
        {
            Messages.Message(
                "TunnelerLife_RockfillUnsupportedMaterial".Translate(),
                MessageTypeDefOf.RejectInput,
                historical: false);
            Destroy(DestroyMode.Vanish);
            return;
        }

        ThingDef rockDef = DefDatabase<ThingDef>.GetNamedSilentFail(rockDefName);
        IntVec3 cell = Position;

        Destroy(DestroyMode.Vanish);
        GenSpawn.Spawn(rockDef, cell, map, WipeMode.Vanish);
    }
}
