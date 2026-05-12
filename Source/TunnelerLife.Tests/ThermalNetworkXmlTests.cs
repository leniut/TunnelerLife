using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalNetworkXmlTests
{
    [Fact]
    public void ThermalPipe_IsDragBuildableAndDoesNotUsePowerNet()
    {
        XElement pipeDef = LoadThingDef("TunnelerLife_ThermalPipe");

        Assert.Equal("TunnelerLife.Building_ThermalPipe", (string?)pipeDef.Element("thingClass"));
        Assert.Contains("inspect the reachable network temperature", (string?)pipeDef.Element("description") ?? string.Empty);
        Assert.Equal("TunnelerLife", (string?)pipeDef.Element("designationCategory"));
        Assert.Equal("Conduits", (string?)pipeDef.Element("drawStyleCategory"));
        Assert.Equal("false", (string?)pipeDef.Element("clearBuildingArea"));
        Assert.Equal(
            "Things/Building/Linked/PowerConduit_Atlas",
            (string?)pipeDef.Element("graphicData")?.Element("texPath"));
        Assert.Equal(
            "TunnelerLife.Graphic_LinkedThermalPipe",
            (string?)pipeDef.Element("graphicData")?.Element("graphicClass"));
        Assert.Null(pipeDef.Element("graphicData")?.Element("linkType"));
        Assert.Contains(
            "Custom1",
            pipeDef.Element("graphicData")?.Element("linkFlags")?.Elements("li").Select(element => element.Value) ?? []);
        Assert.Equal("(0.84,0.71,0.08,1)", (string?)pipeDef.Element("graphicData")?.Element("color"));
        Assert.Contains(
            "TunnelerLife.PlaceWorker_ThermalPipe",
            pipeDef.Element("placeWorkers")?.Elements("li").Select(element => element.Value) ?? []);
        Assert.DoesNotContain(
            pipeDef.Descendants("li"),
            element => ((string?)element.Attribute("Class")) == "CompProperties_Power");
        Assert.DoesNotContain(
            pipeDef.Descendants("li"),
            element => ((string?)element.Attribute("Class")) == "TunnelerLife.CompProperties_ThermalPipeOverlay");
        Assert.Equal("2", (string?)pipeDef.Element("costList")?.Element("Steel"));
    }

    [Fact]
    public void ThermalPipeVariants_MirrorVanillaConduitOptionsAtDoubleSteelCost()
    {
        XElement basePipeDef = LoadThingDef("TunnelerLife_ThermalPipe");
        XElement hiddenPipeDef = LoadThingDef("TunnelerLife_HiddenThermalPipe");
        XElement waterproofPipeDef = LoadThingDef("TunnelerLife_WaterproofThermalPipe");

        Assert.Equal("TunnelerLife_ThermalPipe", (string?)basePipeDef.Attribute("Name"));
        Assert.Equal("TunnelerLife_ThermalPipe", (string?)hiddenPipeDef.Attribute("ParentName"));
        Assert.Equal("hidden thermal pipe", (string?)hiddenPipeDef.Element("label"));
        Assert.Equal("4", (string?)hiddenPipeDef.Element("costList")?.Element("Steel"));
        Assert.Equal("false", (string?)hiddenPipeDef.Element("building")?.Element("canBeDamagedByAttacks"));

        Assert.Equal("TunnelerLife_ThermalPipe", (string?)waterproofPipeDef.Attribute("ParentName"));
        Assert.Equal("waterproof thermal pipe", (string?)waterproofPipeDef.Element("label"));
        Assert.Equal("20", (string?)waterproofPipeDef.Element("costList")?.Element("Steel"));
        Assert.Equal("WaterproofConduitable", (string?)waterproofPipeDef.Element("terrainAffordanceNeeded"));
    }

    [Fact]
    public void ThermalVent_IsWallPlacedFlickableVent()
    {
        XElement ventDef = LoadThingDef("TunnelerLife_ThermalVent");

        Assert.Equal("TunnelerLife.Building_ThermalVent", (string?)ventDef.Element("thingClass"));
        Assert.Contains("thermal pipes on the other three sides", (string?)ventDef.Element("description") ?? string.Empty);
        Assert.Equal("TunnelerLife", (string?)ventDef.Element("designationCategory"));
        Assert.Equal("Rare", (string?)ventDef.Element("tickerType"));
        Assert.Equal("true", (string?)ventDef.Element("building")?.Element("canPlaceOverWall"));
        Assert.Contains(
            "TunnelerLife.PlaceWorker_ThermalVent",
            ventDef.Element("placeWorkers")?.Elements("li").Select(element => element.Value) ?? []);
        Assert.DoesNotContain(
            "PlaceWorker_Vent",
            ventDef.Element("placeWorkers")?.Elements("li").Select(element => element.Value) ?? []);
        Assert.Contains(
            ventDef.Descendants("li"),
            element => ((string?)element.Attribute("Class")) == "CompProperties_Flickable");
    }

    [Fact]
    public void ThermalValve_IsFlickableTemperatureTransferCutoff()
    {
        XElement switchDef = LoadThingDef("TunnelerLife_ThermalPipeSwitch");

        Assert.Equal("TunnelerLife_ThermalPipeSwitch", (string?)switchDef.Element("defName"));
        Assert.Equal("thermal valve", (string?)switchDef.Element("label"));
        Assert.Contains("temperature transfer", (string?)switchDef.Element("description") ?? string.Empty);
        Assert.DoesNotContain("power", ((string?)switchDef.Element("description") ?? string.Empty).ToLowerInvariant());
        Assert.Equal("TunnelerLife.Building_ThermalPipeSwitch", (string?)switchDef.Element("thingClass"));
        Assert.Equal("TunnelerLife", (string?)switchDef.Element("designationCategory"));
        Assert.Equal("RealtimeOnly", (string?)switchDef.Element("drawerType"));
        Assert.Equal("Things/Building/TunnelerLife/ThermalValve", (string?)switchDef.Element("graphicData")?.Element("texPath"));
        Assert.Equal("UI/Commands/ThermalValve", (string?)switchDef.Element("uiIconPath"));
        Assert.Null(switchDef.Element("graphicData")?.Element("linkType"));
        Assert.Contains(
            "Custom1",
            switchDef.Element("graphicData")?.Element("linkFlags")?.Elements("li").Select(element => element.Value) ?? []);
        Assert.True(File.Exists(Path.Combine(FindModRoot(), "Textures", "Things", "Building", "TunnelerLife", "ThermalValve.png")));
        Assert.True(File.Exists(Path.Combine(FindModRoot(), "Textures", "UI", "Commands", "ThermalValve.png")));
        Assert.Equal("30", (string?)switchDef.Element("costList")?.Element("Steel"));
        Assert.Equal("1", (string?)switchDef.Element("costList")?.Element("ComponentIndustrial"));
        Assert.Contains(
            "TunnelerLife.PlaceWorker_ThermalPipe",
            switchDef.Element("placeWorkers")?.Elements("li").Select(element => element.Value) ?? []);
        XElement flickableComp = switchDef.Descendants("li")
            .Single(element => ((string?)element.Attribute("Class")) == "CompProperties_Flickable");
        Assert.Equal("UI/Commands/ThermalValve", (string?)flickableComp.Element("commandTexture"));
        Assert.Equal("TunnelerLife_CommandDesignateOpenCloseThermalValveLabel", (string?)flickableComp.Element("commandLabelKey"));
        Assert.Equal("TunnelerLife_CommandDesignateOpenCloseThermalValveDesc", (string?)flickableComp.Element("commandDescKey"));
        Assert.DoesNotContain(
            switchDef.Descendants("li"),
            element => ((string?)element.Attribute("Class")) == "CompProperties_Power");
    }

    [Fact]
    public void ThermostaticValve_IsPoweredTemperatureControlledCutoff()
    {
        XElement valveDef = LoadThingDef("TunnelerLife_ThermostaticValve");

        Assert.Equal("thermostatic valve", (string?)valveDef.Element("label"));
        Assert.Contains("target temperature", (string?)valveDef.Element("description") ?? string.Empty);
        Assert.Equal("TunnelerLife.Building_ThermostaticValve", (string?)valveDef.Element("thingClass"));
        Assert.Equal("TunnelerLife", (string?)valveDef.Element("designationCategory"));
        Assert.Equal("Rare", (string?)valveDef.Element("tickerType"));
        Assert.Equal("RealtimeOnly", (string?)valveDef.Element("drawerType"));
        Assert.Equal("true", (string?)valveDef.Element("rotatable"));
        Assert.Equal("Things/Building/TunnelerLife/ThermostaticValve", (string?)valveDef.Element("graphicData")?.Element("texPath"));
        Assert.Equal("UI/Commands/ThermostaticValve", (string?)valveDef.Element("uiIconPath"));

        XElement powerComp = valveDef.Descendants("li")
            .Single(element => ((string?)element.Attribute("Class")) == "CompProperties_Power");
        Assert.Equal("CompPowerTrader", (string?)powerComp.Element("compClass"));
        Assert.Equal("20", (string?)powerComp.Element("basePowerConsumption"));

        XElement tempControlComp = valveDef.Descendants("li")
            .Single(element => ((string?)element.Attribute("Class")) == "CompProperties_TempControl");
        Assert.Equal("0", (string?)tempControlComp.Element("energyPerSecond"));
        Assert.DoesNotContain(
            valveDef.Descendants("li"),
            element => ((string?)element.Attribute("Class")) == "CompProperties_Flickable");

        Assert.True(File.Exists(Path.Combine(FindModRoot(), "Textures", "Things", "Building", "TunnelerLife", "ThermostaticValve.png")));
        Assert.True(File.Exists(Path.Combine(FindModRoot(), "Textures", "Things", "Building", "TunnelerLife", "ThermostaticValve_On.png")));
        Assert.True(File.Exists(Path.Combine(FindModRoot(), "Textures", "Things", "Building", "TunnelerLife", "ThermostaticValve_Off.png")));
        Assert.True(File.Exists(Path.Combine(FindModRoot(), "Textures", "UI", "Commands", "ThermostaticValve.png")));
    }

    [Fact]
    public void ThermalPipe_UsesLinkedGraphicThatPrintsConnectionsIntoAdjacentThermalValves()
    {
        string valveTexturePath = Path.Combine(
            FindModRoot(),
            "Textures",
            "Things",
            "Building",
            "TunnelerLife",
            "ThermalValve.png");

        Assert.Equal(
            typeof(Graphic_Linked),
            typeof(Graphic_LinkedThermalPipe).BaseType);
        Assert.Equal(
            typeof(Graphic_LinkedThermalPipe),
            typeof(Graphic_LinkedThermalPipe).GetMethod(nameof(Graphic_LinkedThermalPipe.Print))?.DeclaringType);
        Assert.Equal(
            typeof(Graphic_LinkedThermalPipe),
            typeof(Graphic_LinkedThermalPipe).GetMethod(nameof(Graphic_LinkedThermalPipe.ShouldLinkWith))?.DeclaringType);

        using Bitmap valveTexture = new(valveTexturePath);
        int valveCenterX = valveTexture.Width / 2;
        int valveCenterY = valveTexture.Height / 2;
        Assert.Equal(0, valveTexture.GetPixel(valveCenterX, 0).A);
        Assert.Equal(0, valveTexture.GetPixel(valveCenterX, valveTexture.Height - 1).A);
        Assert.Equal(0, valveTexture.GetPixel(0, valveCenterY).A);
        Assert.Equal(0, valveTexture.GetPixel(valveTexture.Width - 1, valveCenterY).A);
        Assert.True(valveTexture.GetPixel(valveCenterX, valveCenterY).A > 0);
        Assert.True(valveTexture.GetPixel(44, valveCenterY).A > 0);
        Assert.True(valveTexture.GetPixel(valveTexture.Width - 44, valveCenterY).A > 0);
        Assert.True(valveTexture.GetPixel(valveCenterX, 44).A > 0);
        Assert.True(valveTexture.GetPixel(valveCenterX, valveTexture.Height - 44).A > 0);
    }

    [Fact]
    public void TunnelerLifeCategory_DoesNotEnableVanillaPowerGridOverlay()
    {
        string modRoot = FindModRoot();
        string xmlPath = Path.Combine(modRoot, "Defs", "DesignationCategoryDefs", "TunnelerLife_Categories.xml");
        XDocument document = XDocument.Load(xmlPath);
        XElement categoryDef = document.Root?.Element("DesignationCategoryDef")
            ?? throw new InvalidOperationException("Tunneler Life category was not found.");

        Assert.Null(categoryDef.Element("showPowerGrid"));
    }

    [Fact]
    public void ThermalValveLanguage_UsesTemperatureTransferTextInsteadOfPowerText()
    {
        XElement languageData = LoadLanguageData();

        Assert.Equal(
            "Designate open/close thermal valve",
            (string?)languageData.Element("TunnelerLife_CommandDesignateOpenCloseThermalValveLabel"));
        string description = (string?)languageData.Element("TunnelerLife_CommandDesignateOpenCloseThermalValveDesc")
            ?? string.Empty;
        Assert.Contains("temperature transfer", description);
        Assert.DoesNotContain("power", description.ToLowerInvariant());
    }

    [Fact]
    public void ThermalNetworkDiagnosticsLanguage_ContainsPipeAndVentInspectLabels()
    {
        XElement languageData = LoadLanguageData();

        Assert.Equal(
            "Network temperature: {0}",
            (string?)languageData.Element("TunnelerLife_ThermalPipeNetworkTemperatureInspect"));
        Assert.Equal(
            "Connected rooms: {0}",
            (string?)languageData.Element("TunnelerLife_ThermalPipeConnectedRoomsInspect"));
        Assert.Equal(
            "Flow: {0}",
            (string?)languageData.Element("TunnelerLife_ThermalVentFlowModeInspect"));
        Assert.Equal(
            "Air side: {0} ({1})",
            (string?)languageData.Element("TunnelerLife_ThermalVentAirSideInspect"));
        Assert.Equal(
            "Pipe sides: {0} ({1} connected)",
            (string?)languageData.Element("TunnelerLife_ThermalVentPipeSidesInspect"));
        Assert.Equal("north", (string?)languageData.Element("TunnelerLife_DirectionNorth"));
        Assert.Equal("south", (string?)languageData.Element("TunnelerLife_DirectionSouth"));
    }

    [Fact]
    public void SettingsLanguage_ContainsFeatureToggleLabels()
    {
        XElement languageData = LoadLanguageData();

        Assert.NotNull(languageData.Element("TunnelerLife_SettingsIntro"));
        Assert.Equal(
            "Builders",
            (string?)languageData.Element("TunnelerLife_SettingsTabBuilders"));
        Assert.Equal(
            "Commands",
            (string?)languageData.Element("TunnelerLife_SettingsTabCommands"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsBuildersIntro"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsCommandsIntro"));
        Assert.Equal(
            "World rebuilding",
            (string?)languageData.Element("TunnelerLife_SettingsSectionWorldRebuilding"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsSectionWorldRebuildingDesc"));
        Assert.Equal(
            "Enable wall rebuilding",
            (string?)languageData.Element("TunnelerLife_SettingsEnableWallRebuilding"));
        Assert.Equal(
            "Heat pipe system",
            (string?)languageData.Element("TunnelerLife_SettingsSectionThermalSystem"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsSectionThermalSystemDesc"));
        Assert.Equal(
            "Heat pipe system",
            (string?)languageData.Element("TunnelerLife_SettingsEnableThermalSystem"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsThermostatTolerance"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsThermalTransferStrength"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsSectionPipeVariants"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsEnableHiddenThermalPipes"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsEnableWaterproofThermalPipes"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsSectionInterfaceDiagnostics"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsShowThermalOverlay"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsShowVentDirectionMarkers"));
        Assert.NotNull(languageData.Element("TunnelerLife_SettingsEnableThermalDebugInfo"));
        Assert.NotNull(languageData.Element("TunnelerLife_WallRebuildingDisabled"));
        Assert.NotNull(languageData.Element("TunnelerLife_ThermalSystemDisabled"));
        Assert.NotNull(languageData.Element("TunnelerLife_HiddenThermalPipesDisabled"));
        Assert.NotNull(languageData.Element("TunnelerLife_WaterproofThermalPipesDisabled"));
    }

    [Fact]
    public void TunnelerLife_PatchesVanillaPowerSwitchToDrawAboveCables()
    {
        string modRoot = FindModRoot();
        string xmlPath = Path.Combine(modRoot, "Patches", "TunnelerLife_VanillaPowerSwitch.xml");
        XDocument document = XDocument.Load(xmlPath);

        XElement operation = document.Root?.Element("Operation")
            ?? throw new InvalidOperationException("Vanilla power switch patch operation was not found.");

        Assert.Equal("PatchOperationAdd", (string?)operation.Attribute("Class"));
        Assert.Equal("Defs/ThingDef[defName=\"PowerSwitch\"]", (string?)operation.Element("xpath"));
        Assert.Equal("RealtimeOnly", (string?)operation.Element("value")?.Element("drawerType"));
    }

    private static XElement LoadThingDef(string defName)
    {
        string modRoot = FindModRoot();
        string xmlPath = Path.Combine(modRoot, "Defs", "ThingDefs", "TunnelerLife_ThermalNetwork.xml");
        XDocument document = XDocument.Load(xmlPath);

        return document.Root?.Elements("ThingDef")
            .SingleOrDefault(element => ((string?)element.Element("defName")) == defName)
            ?? throw new InvalidOperationException($"ThingDef '{defName}' was not found.");
    }

    private static string FindModRoot()
    {
        DirectoryInfo? directory = new(Directory.GetCurrentDirectory());
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "About", "About.xml"))
                && Directory.Exists(Path.Combine(directory.FullName, "Defs")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Tunneler Life mod root.");
    }

    private static XElement LoadLanguageData()
    {
        string modRoot = FindModRoot();
        string xmlPath = Path.Combine(modRoot, "Languages", "English", "Keyed", "TunnelerLife.xml");
        XDocument document = XDocument.Load(xmlPath);

        return document.Root
            ?? throw new InvalidOperationException("English keyed language data was not found.");
    }

}
