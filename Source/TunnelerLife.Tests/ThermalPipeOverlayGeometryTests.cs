using System.Linq;
using TunnelerLife;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalPipeOverlayGeometryTests
{
    [Fact]
    public void ThermalPipeOverlayMapComponent_DoesNotUseStaticInitializer()
    {
        Assert.Null(typeof(ThermalPipeOverlayMapComponent).TypeInitializer);
    }

    [Fact]
    public void OverlayVisibility_ReturnsTrueForTunnelerLifeCategoryOrSelectedThermalBuildable()
    {
        Assert.True(ThermalPipeOverlayVisibility.ShouldDrawForSelectedCategoryDefName("TunnelerLife"));
        Assert.True(ThermalPipeOverlayVisibility.ShouldDrawForThingDef(null, typeof(Building_ThermalPipe)));
        Assert.True(ThermalPipeOverlayVisibility.ShouldDrawForThingDef("TunnelerLife_ThermalVent", null));
        Assert.False(ThermalPipeOverlayVisibility.ShouldDrawForSelectedCategoryDefName("Power"));
        Assert.False(ThermalPipeOverlayVisibility.ShouldDrawForThingDef("PowerConduit", typeof(Building)));
    }

    [Fact]
    public void ThermalPipeOverlayMaterial_UsesGeneratedYellowAtlasWithoutVanillaTextureTint()
    {
        Assert.Equal("GeneratedYellowPipeAtlas", ThermalPipeOverlayMaterialSpec.AtlasSource);
        Assert.Equal("MetaOverlay", ThermalPipeOverlayMaterialSpec.ShaderName);
        Assert.Equal(4, ThermalPipeOverlayAtlasSpec.CellsPerSide);
        Assert.Equal(32, ThermalPipeOverlayAtlasSpec.CellSizePixels);
        Assert.Equal(5, ThermalPipeOverlayAtlasSpec.LineWidthPixels);
        Assert.False(ThermalPipeOverlayMaterialSpec.UsesVanillaPowerTransmitterAtlas);
    }

    [Fact]
    public void GetLinkedCells_ReturnsPowerOverlayDirectionsForThermalPipeCells()
    {
        IntVec3[] pipeCells =
        [
            new(0, 0, 0),
            new(1, 0, 0),
            new(1, 0, 1),
            new(3, 0, 0)
        ];

        ThermalPipeOverlayCell[] linkedCells = ThermalPipeOverlayGeometry
            .GetLinkedCells(pipeCells)
            .ToArray();

        Assert.Equal(4, linkedCells.Length);
        Assert.Contains(new ThermalPipeOverlayCell(new IntVec3(0, 0, 0), LinkDirections.Right), linkedCells);
        Assert.Contains(new ThermalPipeOverlayCell(new IntVec3(1, 0, 0), LinkDirections.Up | LinkDirections.Left), linkedCells);
        Assert.Contains(new ThermalPipeOverlayCell(new IntVec3(1, 0, 1), LinkDirections.Down), linkedCells);
        Assert.Contains(new ThermalPipeOverlayCell(new IntVec3(3, 0, 0), LinkDirections.None), linkedCells);
    }

    [Fact]
    public void GetLinkedCells_DoesNotDrawConnectionsThroughInactiveNetworkCells()
    {
        IntVec3[] visibleCells =
        [
            new(0, 0, 0),
            new(1, 0, 0),
            new(2, 0, 0)
        ];
        IntVec3[] activeCells =
        [
            new(0, 0, 0),
            new(2, 0, 0)
        ];

        ThermalPipeOverlayCell[] linkedCells = ThermalPipeOverlayGeometry
            .GetLinkedCells(visibleCells, activeCells)
            .ToArray();

        Assert.Equal(3, linkedCells.Length);
        Assert.Contains(new ThermalPipeOverlayCell(new IntVec3(0, 0, 0), LinkDirections.None), linkedCells);
        Assert.Contains(new ThermalPipeOverlayCell(new IntVec3(1, 0, 0), LinkDirections.None), linkedCells);
        Assert.Contains(new ThermalPipeOverlayCell(new IntVec3(2, 0, 0), LinkDirections.None), linkedCells);
    }

    [Fact]
    public void GetConnectedSegments_ReturnsUniqueCardinalPipeConnections()
    {
        IntVec3[] pipeCells =
        [
            new(0, 0, 0),
            new(1, 0, 0),
            new(1, 0, 1),
            new(3, 0, 0)
        ];

        ThermalPipeOverlaySegment[] segments = ThermalPipeOverlayGeometry
            .GetConnectedSegments(pipeCells)
            .ToArray();

        Assert.Equal(2, segments.Length);
        Assert.Contains(new ThermalPipeOverlaySegment(new IntVec3(0, 0, 0), new IntVec3(1, 0, 0)), segments);
        Assert.Contains(new ThermalPipeOverlaySegment(new IntVec3(1, 0, 0), new IntVec3(1, 0, 1)), segments);
    }

    [Fact]
    public void GetIsolatedCells_ReturnsPipeCellsWithoutCardinalPipeNeighbors()
    {
        IntVec3[] pipeCells =
        [
            new(0, 0, 0),
            new(1, 0, 0),
            new(3, 0, 0)
        ];

        IntVec3[] isolatedCells = ThermalPipeOverlayGeometry
            .GetIsolatedCells(pipeCells)
            .ToArray();

        Assert.Equal([new IntVec3(3, 0, 0)], isolatedCells);
    }
}
