using System;
using System.Globalization;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// RimWorld mod entry point and settings UI.
/// </summary>
public sealed class TunnelerLifeMod : Mod
{
    private const float TabHeight = 34f;
    private const float TabWidth = 160f;
    private const float ContentTopGap = 6f;
    private const float ContentPadding = 16f;
    private const float ChildIndent = 28f;
    private const float DescriptionIndent = 18f;
    private static readonly Color DescriptionColor = new(0.72f, 0.72f, 0.72f, 1f);

    private SettingsTab selectedSettingsTab = SettingsTab.Builders;

    public static TunnelerLifeSettings Settings { get; private set; } = new();

    public TunnelerLifeMod(ModContentPack content)
        : base(content)
    {
        Settings = GetSettings<TunnelerLifeSettings>();
        TunnelerLifeFeatureAvailability.ApplySettings(Settings);
    }

    public override string SettingsCategory()
    {
        return "Tunneler Life";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        bool previousWallRebuilding = Settings.EnableWallRebuilding;
        bool previousThermalSystem = Settings.EnableThermalSystem;
        bool previousHiddenThermalPipes = Settings.AllowHiddenThermalPipes;
        bool previousWaterproofThermalPipes = Settings.AllowWaterproofThermalPipes;

        DrawTabs(inRect);
        Rect contentRect = new(
            inRect.x,
            inRect.y + TabHeight + ContentTopGap,
            inRect.width,
            inRect.height - TabHeight - ContentTopGap);
        Widgets.DrawMenuSection(contentRect);

        Rect listingRect = new(
            contentRect.x + ContentPadding,
            contentRect.y + ContentPadding,
            contentRect.width - ContentPadding * 2f,
            contentRect.height - ContentPadding * 2f);
        Listing_Standard listing = new();
        listing.Begin(listingRect);
        DrawDescription(listing, "TunnelerLife_SettingsIntro");
        listing.GapLine();

        if (selectedSettingsTab == SettingsTab.Builders)
        {
            DrawBuildersTab(listing);
        }
        else
        {
            DrawCommandsTab(listing);
        }

        listing.End();
        Settings.NormalizeValues();

        if (previousWallRebuilding != Settings.EnableWallRebuilding
            || previousThermalSystem != Settings.EnableThermalSystem
            || previousHiddenThermalPipes != Settings.AllowHiddenThermalPipes
            || previousWaterproofThermalPipes != Settings.AllowWaterproofThermalPipes)
        {
            TunnelerLifeFeatureAvailability.ApplySettings(Settings);
        }
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        TunnelerLifeFeatureAvailability.ApplySettings(Settings);
    }

    private void DrawTabs(Rect inRect)
    {
        Rect buildersTab = new(inRect.x, inRect.y, TabWidth, TabHeight);
        Rect commandsTab = new(inRect.x + TabWidth + 4f, inRect.y, TabWidth, TabHeight);
        DrawTabButton(buildersTab, SettingsTab.Builders, "TunnelerLife_SettingsTabBuilders");
        DrawTabButton(commandsTab, SettingsTab.Commands, "TunnelerLife_SettingsTabCommands");
    }

    private void DrawTabButton(Rect rect, SettingsTab tab, string labelKey)
    {
        if (selectedSettingsTab == tab)
        {
            Widgets.DrawHighlight(rect);
        }

        if (Widgets.ButtonText(rect, labelKey.Translate()))
        {
            selectedSettingsTab = tab;
        }
    }

    private static void DrawBuildersTab(Listing_Standard listing)
    {
        DrawDescription(listing, "TunnelerLife_SettingsBuildersIntro");
        listing.GapLine();

        DrawCheckbox(
            listing,
            "TunnelerLife_SettingsEnableWallRebuilding",
            "TunnelerLife_SettingsEnableWallRebuildingDesc",
            ref Settings.EnableWallRebuilding);
        listing.GapLine();

        DrawCheckbox(
            listing,
            "TunnelerLife_SettingsEnableThermalSystem",
            "TunnelerLife_SettingsEnableThermalSystemDesc",
            ref Settings.EnableThermalSystem);
        DrawDescription(listing, "TunnelerLife_SettingsSectionPipeVariantsDesc", ChildIndent);
        DrawCheckbox(
            listing,
            "TunnelerLife_SettingsEnableHiddenThermalPipes",
            "TunnelerLife_SettingsEnableHiddenThermalPipesDesc",
            ref Settings.AllowHiddenThermalPipes,
            ChildIndent);
        DrawCheckbox(
            listing,
            "TunnelerLife_SettingsEnableWaterproofThermalPipes",
            "TunnelerLife_SettingsEnableWaterproofThermalPipesDesc",
            ref Settings.AllowWaterproofThermalPipes,
            ChildIndent);
    }

    private static void DrawCommandsTab(Listing_Standard listing)
    {
        DrawDescription(listing, "TunnelerLife_SettingsCommandsIntro");
        listing.GapLine();

        DrawSectionHeader(
            listing,
            "TunnelerLife_SettingsSectionThermalSystem",
            "TunnelerLife_SettingsSectionThermalSystemDesc");
        DrawThermalSlider(
            listing,
            "TunnelerLife_SettingsThermostatTolerance",
            "TunnelerLife_SettingsThermostatToleranceDesc",
            ref Settings.ThermostatTolerance,
            TunnelerLifeSettings.MinThermostatTolerance,
            TunnelerLifeSettings.MaxThermostatTolerance,
            0.1f,
            value => value.ToString("0.#", CultureInfo.InvariantCulture) + " C",
            ChildIndent);
        DrawThermalSlider(
            listing,
            "TunnelerLife_SettingsThermalTransferStrength",
            "TunnelerLife_SettingsThermalTransferStrengthDesc",
            ref Settings.ThermalTransferStrength,
            TunnelerLifeSettings.MinThermalTransferStrength,
            TunnelerLifeSettings.MaxThermalTransferStrength,
            0.05f,
            value => (value * 100f).ToString("0", CultureInfo.InvariantCulture) + "%",
            ChildIndent);
        DrawCheckbox(
            listing,
            "TunnelerLife_SettingsShowThermalOverlay",
            "TunnelerLife_SettingsShowThermalOverlayDesc",
            ref Settings.ShowThermalOverlay,
            ChildIndent);
        DrawCheckbox(
            listing,
            "TunnelerLife_SettingsShowVentDirectionMarkers",
            "TunnelerLife_SettingsShowVentDirectionMarkersDesc",
            ref Settings.ShowVentDirectionMarkers,
            ChildIndent);
        DrawCheckbox(
            listing,
            "TunnelerLife_SettingsEnableThermalDebugInfo",
            "TunnelerLife_SettingsEnableThermalDebugInfoDesc",
            ref Settings.EnableThermalDebugInfo,
            ChildIndent);
    }

    private static void DrawSectionHeader(
        Listing_Standard listing,
        string titleKey,
        string descriptionKey,
        float indent = 0f)
    {
        DrawLabel(listing, titleKey.Translate().ToString(), GameFont.Small, Color.white, indent);
        DrawDescription(listing, descriptionKey, indent + DescriptionIndent);
    }

    private static void DrawThermalSlider(
        Listing_Standard listing,
        string labelKey,
        string descriptionKey,
        ref float value,
        float min,
        float max,
        float step,
        Func<float, string> formatValue,
        float indent = 0f)
    {
        DrawLabel(listing, labelKey.Translate(formatValue(value)).ToString(), GameFont.Small, Color.white, indent);
        Rect sliderRect = listing.GetRect(22f);
        sliderRect.xMin += indent;
        value = RoundToStep(Widgets.HorizontalSlider(sliderRect, value, min, max), step);
        DrawDescription(listing, descriptionKey, indent + DescriptionIndent);
        listing.Gap(4f);
    }

    private static void DrawCheckbox(
        Listing_Standard listing,
        string labelKey,
        string descriptionKey,
        ref bool value,
        float indent = 0f)
    {
        Rect rowRect = listing.GetRect(28f);
        rowRect.xMin += indent;
        Widgets.CheckboxLabeled(rowRect, labelKey.Translate(), ref value);
        DrawDescription(listing, descriptionKey, indent + DescriptionIndent);
        listing.Gap(4f);
    }

    private static void DrawDescription(Listing_Standard listing, string descriptionKey, float indent = 0f)
    {
        DrawLabel(
            listing,
            descriptionKey.Translate().ToString(),
            GameFont.Tiny,
            DescriptionColor,
            indent);
    }

    private static void DrawLabel(
        Listing_Standard listing,
        string text,
        GameFont font,
        Color color,
        float indent)
    {
        GameFont previousFont = Text.Font;
        Color previousColor = GUI.color;
        Text.Font = font;
        GUI.color = color;

        Rect rect = listing.GetRect(Text.CalcHeight(text, listing.ColumnWidth - indent));
        rect.xMin += indent;
        Widgets.Label(rect, text);

        GUI.color = previousColor;
        Text.Font = previousFont;
    }

    private static float RoundToStep(float value, float step)
    {
        return Mathf.Round(value / step) * step;
    }

    private enum SettingsTab
    {
        Builders,
        Commands
    }
}
