using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Orders-tab dropdown designator for placing rockfill with available stone chunks.
/// </summary>
public sealed class Designator_Rockfill : Designator_Dropdown
{
    private const string IconPath = "UI/Designators/Rockfill";

    /// <summary>
    /// Creates the Orders command and its compact material menu.
    /// </summary>
    public Designator_Rockfill()
    {
        defaultLabel = "Rockfill";
        defaultDesc = "Rebuild mined-out tunnel cells into natural rough stone using available stone chunks.";
        icon = ContentFinder<Texture2D>.Get(IconPath, false);
        useMouseIcon = true;

        foreach (RockfillMaterial material in RockfillMaterialResolver.SupportedMaterials)
        {
            ThingDef rockfillDef = DefDatabase<ThingDef>.GetNamedSilentFail(material.RockfillDefName);
            ThingDef chunkDef = DefDatabase<ThingDef>.GetNamedSilentFail(material.ChunkDefName);
            if (rockfillDef != null && chunkDef != null)
            {
                Add(new Designator_RockfillBuild(rockfillDef, chunkDef));
            }
        }
    }

    public override bool Visible => Elements.Any(element => element.Visible);

    private static int CountAvailableChunks(ThingDef chunkDef)
    {
        Map map = Find.CurrentMap;
        if (map == null)
        {
            return 0;
        }

        IReadOnlyList<Thing> chunks = map.listerThings.ThingsOfDef(chunkDef);
        return chunks
            .Where(chunk => chunk.Spawned && !chunk.IsForbidden(Faction.OfPlayer))
            .Sum(chunk => chunk.stackCount);
    }

    private sealed class Designator_RockfillBuild : Designator_Build
    {
        private readonly ThingDef chunkDef;

        public Designator_RockfillBuild(BuildableDef rockfillDef, ThingDef chunkDef)
            : base(rockfillDef)
        {
            this.chunkDef = chunkDef;

            defaultLabel = $"{chunkDef.LabelCap} rockfill";
            defaultDesc = $"Rebuild selected tunnel cells into rough natural stone using {chunkDef.label}.";
            icon = ContentFinder<Texture2D>.Get(IconPath, false);
            useMouseIcon = true;
        }

        public override bool Visible => CountAvailableChunks(chunkDef) > 0;

        public override string Label => $"{chunkDef.LabelCap} rockfill ({CountAvailableChunks(chunkDef)})";

        public override string Desc => $"Rebuild selected tunnel cells into rough natural stone using {chunkDef.label}.";
    }
}
