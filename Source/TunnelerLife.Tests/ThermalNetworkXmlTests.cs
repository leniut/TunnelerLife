using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalNetworkXmlTests
{
    [Fact]
    public void ThermalPipe_IsDragBuildableAndDoesNotUsePowerNet()
    {
        XElement pipeDef = LoadThingDef("TunnelerLife_ThermalPipe");

        Assert.Equal("TunnelerLife.Building_ThermalPipe", (string?)pipeDef.Element("thingClass"));
        Assert.Equal("TunnelerLife", (string?)pipeDef.Element("designationCategory"));
        Assert.Equal("Conduits", (string?)pipeDef.Element("drawStyleCategory"));
        Assert.Equal("false", (string?)pipeDef.Element("clearBuildingArea"));
        Assert.Contains(
            "TunnelerLife.PlaceWorker_ThermalPipe",
            pipeDef.Element("placeWorkers")?.Elements("li").Select(element => element.Value) ?? []);
        Assert.DoesNotContain(
            pipeDef.Descendants("li"),
            element => ((string?)element.Attribute("Class")) == "CompProperties_Power");
    }

    [Fact]
    public void ThermalVent_IsWallPlacedFlickableVent()
    {
        XElement ventDef = LoadThingDef("TunnelerLife_ThermalVent");

        Assert.Equal("TunnelerLife.Building_ThermalVent", (string?)ventDef.Element("thingClass"));
        Assert.Equal("TunnelerLife", (string?)ventDef.Element("designationCategory"));
        Assert.Equal("Rare", (string?)ventDef.Element("tickerType"));
        Assert.Equal("true", (string?)ventDef.Element("building")?.Element("canPlaceOverWall"));
        Assert.Contains(
            "PlaceWorker_Vent",
            ventDef.Element("placeWorkers")?.Elements("li").Select(element => element.Value) ?? []);
        Assert.Contains(
            ventDef.Descendants("li"),
            element => ((string?)element.Attribute("Class")) == "CompProperties_Flickable");
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
}
