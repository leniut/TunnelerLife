using Verse;

namespace TunnelerLife;

/// <summary>
/// Validates rockfill placement before the temporary construction building is created.
/// </summary>
public sealed class PlaceWorker_Rockfill : PlaceWorker
{
    /// <summary>
    /// Allows rockfill placement only on in-bounds, buildable terrain without an existing edifice.
    /// </summary>
    /// <param name="checkingDef">The rockfill buildable definition being placed.</param>
    /// <param name="loc">The target map cell.</param>
    /// <param name="rot">The selected placement rotation.</param>
    /// <param name="map">The target map.</param>
    /// <param name="thingToIgnore">A thing ignored by the placement validator.</param>
    /// <param name="thing">The thing being placed, when available.</param>
    /// <returns>An acceptance report describing whether the rockfill may be placed.</returns>
    public override AcceptanceReport AllowsPlacing(
        BuildableDef checkingDef,
        IntVec3 loc,
        Rot4 rot,
        Map map,
        Thing thingToIgnore = null!,
        Thing thing = null!)
    {
        if (!TunnelerLifeFeatureAvailability.WallRebuildingEnabled)
        {
            return TunnelerLifeFeatureAvailability.WallRebuildingDisabledReport;
        }

        if (!loc.InBounds(map))
        {
            return false;
        }

        if (loc.GetEdifice(map) != null)
        {
            return "TunnelerLife_RockfillCannotPlaceOnBuilding".Translate();
        }

        TerrainDef terrainDef = map.terrainGrid.TerrainAt(loc);
        if (terrainDef.IsWater || terrainDef.passability == Traversability.Impassable)
        {
            return "TunnelerLife_RockfillCannotPlaceOnWater".Translate();
        }

        return true;
    }
}
