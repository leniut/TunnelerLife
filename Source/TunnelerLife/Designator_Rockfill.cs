using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Orders-tab dropdown designator for placing rockfill with a selected stone block material.
/// </summary>
public sealed class Designator_Rockfill : Designator_Dropdown
{
    private const string RockfillDefName = "TunnelerLife_Rockfill";
    private const string IconPath = "UI/Designators/Rockfill";

    /// <summary>
    /// Creates the Orders command and its compact material menu.
    /// </summary>
    public Designator_Rockfill()
    {
        defaultLabel = "Rockfill";
        defaultDesc = "Rebuild mined-out tunnel cells into natural rough stone using selected stone blocks.";
        icon = ContentFinder<Texture2D>.Get(IconPath, false);
        useMouseIcon = true;

        ThingDef rockfillDef = DefDatabase<ThingDef>.GetNamed(RockfillDefName);
        foreach (string stuffDefName in RockfillMaterialResolver.SupportedStuffDefNames)
        {
            ThingDef stuffDef = DefDatabase<ThingDef>.GetNamedSilentFail(stuffDefName);
            if (stuffDef != null)
            {
                Add(new Designator_RockfillBuild(rockfillDef, stuffDef));
            }
        }
    }

    private sealed class Designator_RockfillBuild : Designator_Build
    {
        private static readonly FieldInfo StuffDefField =
            typeof(Designator_Build).GetField("stuffDef", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(nameof(Designator_Build), "stuffDef");

        private static readonly MethodInfo UpdateIconMethod =
            typeof(Designator_Build).GetMethod("UpdateIcon", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(nameof(Designator_Build), "UpdateIcon");

        private readonly ThingDef selectedStuffDef;

        public Designator_RockfillBuild(BuildableDef rockfillDef, ThingDef selectedStuffDef)
            : base(rockfillDef)
        {
            this.selectedStuffDef = selectedStuffDef ?? throw new ArgumentNullException(nameof(selectedStuffDef));
            StuffDefField.SetValue(this, selectedStuffDef);

            defaultLabel = $"{selectedStuffDef.LabelCap} rockfill";
            defaultDesc = $"Rebuild selected tunnel cells into rough natural stone using {selectedStuffDef.label}.";
            icon = ContentFinder<Texture2D>.Get(IconPath, false);
            useMouseIcon = true;

            if (selectedStuffDef.stuffProps?.color != null)
            {
                defaultIconColor = selectedStuffDef.stuffProps.color;
            }

            UpdateIconMethod.Invoke(this, null);
        }

        public override string Label => $"{selectedStuffDef.LabelCap} rockfill";

        public override string Desc => $"Rebuild selected tunnel cells into rough natural stone using {selectedStuffDef.label}.";
    }
}
