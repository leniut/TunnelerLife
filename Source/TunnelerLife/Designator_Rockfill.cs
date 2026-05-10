using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Orders-tab designator that opens a chunk-filtered material menu before placing rockfill.
/// </summary>
public sealed class Designator_Rockfill : Designator
{
    private const string IconPath = "UI/Designators/Rockfill";

    private readonly List<Designator_RockfillBuild> buildDesignators = [];

    /// <summary>
    /// Creates the Orders command and its compact chunk material menu.
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
                buildDesignators.Add(new Designator_RockfillBuild(rockfillDef, chunkDef));
            }
        }
    }

    public override void ProcessInput(Event ev)
    {
        List<FloatMenuOption> options = buildDesignators
            .Select(designator => (Designator: designator, Count: CountAvailableChunks(designator.ChunkDef)))
            .Where(option => option.Count > 0)
            .Select(option => new FloatMenuOption(
                $"{option.Designator.ChunkDef.LabelCap} ({option.Count})",
                () => option.Designator.ProcessInput(ev),
                option.Designator.ChunkDef))
            .ToList();

        if (options.Count == 0)
        {
            Messages.Message("TunnelerLife_RockfillNoChunksAvailable".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        Find.WindowStack.Add(new FloatMenu(options));
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        return AcceptanceReport.WasRejected;
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
    }

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

        public ThingDef ChunkDef => chunkDef;

        public Designator_RockfillBuild(BuildableDef rockfillDef, ThingDef chunkDef)
            : base(rockfillDef)
        {
            this.chunkDef = chunkDef;

            defaultLabel = $"{chunkDef.LabelCap} rockfill";
            defaultDesc = $"Rebuild selected tunnel cells into rough natural stone using {chunkDef.label}.";
            icon = ContentFinder<Texture2D>.Get(IconPath, false);
            useMouseIcon = true;
        }

        public override bool Visible => true;

        public override string Label => $"{chunkDef.LabelCap} rockfill";

        public override string Desc => $"Rebuild selected tunnel cells into rough natural stone using {chunkDef.label}.";
    }
}
