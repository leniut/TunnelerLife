using System;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class RockfillXmlTests
{
    [Fact]
    public void RockfillBase_IsBuildableSoRimWorldGeneratesFrames()
    {
        XElement baseDef = LoadRockfillBaseDef();

        Assert.Equal("TunnelerLife", (string?)baseDef.Element("designationCategory"));
        Assert.Equal("false", (string?)baseDef.Element("canGenerateDefaultDesignator"));
    }

    [Fact]
    public void RockfillBase_UsesLinkedWallGraphics()
    {
        XElement baseDef = LoadRockfillBaseDef();
        XElement? graphicData = baseDef.Element("graphicData");
        XElement? blueprintGraphicData = baseDef.Element("building")?.Element("blueprintGraphicData");

        Assert.Equal("Graphic_Appearances", (string?)graphicData?.Element("graphicClass"));
        Assert.Equal("Things/Building/Linked/Wall_Blueprint_Atlas", (string?)blueprintGraphicData?.Element("texPath"));
    }

    private static XElement LoadRockfillBaseDef()
    {
        string modRoot = FindModRoot();
        string xmlPath = Path.Combine(modRoot, "Defs", "ThingDefs", "TunnelerLife_Rockfill.xml");
        XDocument document = XDocument.Load(xmlPath);

        return document.Root?.Element("ThingDef")
            ?? throw new InvalidOperationException("Rockfill base ThingDef was not found.");
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
