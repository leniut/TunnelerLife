using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Draws only Tunneler Life thermal pipes while the Tunneler Life architect category is open.
/// </summary>
public sealed class ThermalPipeOverlayMapComponent : MapComponent
{
    private const int DiagnosticsIntervalTicks = 300;
    private const int MaxDiagnosticSamples = 8;
    private readonly HashSet<IntVec3> networkCells = [];
    private readonly HashSet<IntVec3> activeNetworkCells = [];
    private readonly List<string> diagnosticSamples = [];
    private Material? pipeAtlasMaterial;
    private Texture2D? pipeAtlasTexture;
    private int lastDiagnosticsTick = -999999;
    private string? lastDiagnosticsKey;
    private bool materialLoadFailureLogged;

    public ThermalPipeOverlayMapComponent(Map map)
        : base(map)
    {
        if (TunnelerLifeFeatureAvailability.ThermalDebugInfoEnabled)
        {
            Log.Message($"[TunnelerLife] Thermal pipe overlay map component attached to map '{map?.uniqueID}' ({map?.Size.x}x{map?.Size.z}).");
        }
    }

    public override void MapComponentDraw()
    {
        ThermalPipeOverlayDrawState drawState = GetDrawState();
        if (!drawState.ShouldDraw)
        {
            MaybeLogDiagnostics(drawState, builtPipeCount: 0, plannedPipeCount: 0, pipeCellCount: 0, overlayCellCount: 0, isolatedCount: 0);
            return;
        }

        networkCells.Clear();
        activeNetworkCells.Clear();
        diagnosticSamples.Clear();
        int builtPipeCount = 0;
        int plannedPipeCount = 0;

        foreach (Thing thing in map.listerThings.AllThings)
        {
            ThingDef? buildDef = thing.def.entityDefToBuild as ThingDef;
            bool isBuiltPipe = ThermalPipeUtility.IsThermalPipe(thing.def);
            bool isBuiltSwitch = ThermalPipeUtility.IsThermalValve(thing.def);
            bool isPlannedNetworkCell = ThermalPipeUtility.IsThermalNetworkBuildable(buildDef);
            bool isVisibleNetworkCell = isBuiltPipe || isBuiltSwitch || isPlannedNetworkCell;
            if (!isVisibleNetworkCell)
            {
                continue;
            }

            if (isBuiltPipe)
            {
                builtPipeCount++;
            }

            if (isPlannedNetworkCell)
            {
                plannedPipeCount++;
            }

            if (diagnosticSamples.Count < MaxDiagnosticSamples)
            {
                diagnosticSamples.Add($"{thing.def.defName}->{buildDef?.defName ?? "none"}@({thing.Position.x},{thing.Position.z})");
            }

            if (!networkCells.Contains(thing.Position))
            {
                networkCells.Add(thing.Position);
            }

            if (isPlannedNetworkCell || ThermalPipeUtility.IsActiveThermalNetworkThing(thing))
            {
                activeNetworkCells.Add(thing.Position);
            }
        }

        int linkedCellCount = 0;
        int isolatedCount = 0;
        foreach (ThermalPipeOverlayCell overlayCell in ThermalPipeOverlayGeometry.GetLinkedCells(networkCells, activeNetworkCells))
        {
            linkedCellCount++;
            if (overlayCell.LinkDirections == LinkDirections.None)
            {
                isolatedCount++;
            }

            DrawPipeCell(overlayCell);
        }

        MaybeLogDiagnostics(drawState, builtPipeCount, plannedPipeCount, networkCells.Count, linkedCellCount, isolatedCount);
    }

    private static ThermalPipeOverlayDrawState GetDrawState()
    {
        if (!TunnelerLifeFeatureAvailability.ShowThermalOverlay)
        {
            return new ThermalPipeOverlayDrawState(false, null, null, null, null);
        }

        MainTabWindow_Architect architectWindow = Find.WindowStack.WindowOfType<MainTabWindow_Architect>();
        string? selectedCategoryDefName = architectWindow?.selectedDesPanel?.def?.defName;
        Designator? selectedDesignator = Find.DesignatorManager.SelectedDesignator;
        string? selectedDesignatorType = selectedDesignator?.GetType().FullName;
        string? placingDefName = null;
        string? placingThingClassName = null;
        bool selectedThermalBuildable = false;

        if (selectedDesignator is Designator_Build designator)
        {
            placingDefName = designator.PlacingDef?.defName;
            if (designator.PlacingDef is ThingDef thingDef)
            {
                placingThingClassName = thingDef.thingClass?.FullName;
            }

            selectedThermalBuildable = ThermalPipeOverlayVisibility.ShouldDrawForPlacingDef(designator.PlacingDef);
        }

        bool selectedTunnelerCategory = ThermalPipeOverlayVisibility.ShouldDrawForSelectedCategoryDefName(selectedCategoryDefName);
        return new ThermalPipeOverlayDrawState(
            selectedTunnelerCategory || selectedThermalBuildable,
            selectedCategoryDefName,
            selectedDesignatorType,
            placingDefName,
            placingThingClassName);
    }

    private void DrawPipeCell(ThermalPipeOverlayCell overlayCell)
    {
        Material? atlasMaterial = GetPipeAtlasMaterial();
        if (atlasMaterial == null)
        {
            return;
        }

        Material material = MaterialAtlasPool.SubMaterialFromAtlas(atlasMaterial, overlayCell.LinkDirections);
        Vector3 position = overlayCell.Cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay);
        Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, material, 0);
    }

    private Material? GetPipeAtlasMaterial()
    {
        if (pipeAtlasMaterial != null)
        {
            return pipeAtlasMaterial;
        }

        try
        {
            pipeAtlasTexture = ThermalPipeOverlayAtlasFactory.CreateTexture();
            pipeAtlasMaterial = new Material(ThermalPipeOverlayMaterialSpec.Shader)
            {
                name = ThermalPipeOverlayMaterialSpec.AtlasSource,
                mainTexture = pipeAtlasTexture,
                color = Color.white
            };
            pipeAtlasMaterial.renderQueue = 3600;
            Log.Message("[TunnelerLife] Thermal pipe overlay material loaded on draw thread: generated-yellow-atlas-v1.");
        }
        catch (Exception exception)
        {
            if (!materialLoadFailureLogged)
            {
                materialLoadFailureLogged = true;
                Log.Error($"[TunnelerLife] Thermal pipe overlay material load failed: {exception}");
            }
        }

        return pipeAtlasMaterial;
    }

    private void MaybeLogDiagnostics(
        ThermalPipeOverlayDrawState drawState,
        int builtPipeCount,
        int plannedPipeCount,
        int pipeCellCount,
        int overlayCellCount,
        int isolatedCount)
    {
        if (!TunnelerLifeFeatureAvailability.ThermalDebugInfoEnabled)
        {
            return;
        }

        int ticksGame = GenTicks.TicksGame;
        string key = $"{drawState.ShouldDraw}|{drawState.SelectedCategoryDefName}|{drawState.SelectedDesignatorType}|{drawState.PlacingDefName}|{drawState.PlacingThingClassName}|{builtPipeCount}|{plannedPipeCount}|{pipeCellCount}|{overlayCellCount}|{isolatedCount}";
        if (key == lastDiagnosticsKey && ticksGame - lastDiagnosticsTick < DiagnosticsIntervalTicks)
        {
            return;
        }

        lastDiagnosticsKey = key;
        lastDiagnosticsTick = ticksGame;

        StringBuilder builder = new();
        builder.Append("[TunnelerLife] Thermal pipe overlay diagnostics: ");
        builder.Append("shouldDraw=").Append(drawState.ShouldDraw);
        builder.Append("; category=").Append(drawState.SelectedCategoryDefName ?? "null");
        builder.Append("; designator=").Append(drawState.SelectedDesignatorType ?? "null");
        builder.Append("; placingDef=").Append(drawState.PlacingDefName ?? "null");
        builder.Append("; placingThingClass=").Append(drawState.PlacingThingClassName ?? "null");
        builder.Append("; allThings=").Append(map.listerThings.AllThings.Count);
        builder.Append("; builtPipes=").Append(builtPipeCount);
        builder.Append("; plannedPipes=").Append(plannedPipeCount);
        builder.Append("; uniquePipeCells=").Append(pipeCellCount);
        builder.Append("; overlayCells=").Append(overlayCellCount);
        builder.Append("; isolated=").Append(isolatedCount);
        if (diagnosticSamples.Count > 0)
        {
            builder.Append("; samples=").Append(string.Join(", ", diagnosticSamples));
        }

        Log.Message(builder.ToString());
    }
}

internal readonly struct ThermalPipeOverlayDrawState(
    bool shouldDraw,
    string? selectedCategoryDefName,
    string? selectedDesignatorType,
    string? placingDefName,
    string? placingThingClassName)
{
    public bool ShouldDraw { get; } = shouldDraw;

    public string? SelectedCategoryDefName { get; } = selectedCategoryDefName;

    public string? SelectedDesignatorType { get; } = selectedDesignatorType;

    public string? PlacingDefName { get; } = placingDefName;

    public string? PlacingThingClassName { get; } = placingThingClassName;
}

internal static class ThermalPipeOverlayMaterialSpec
{
    public const string AtlasSource = "GeneratedYellowPipeAtlas";
    public const string ShaderName = "MetaOverlay";
    public const bool UsesVanillaPowerTransmitterAtlas = false;

    public static Shader Shader => ShaderDatabase.MetaOverlay;
}

internal static class ThermalPipeOverlayAtlasSpec
{
    public const int CellsPerSide = 4;
    public const int CellSizePixels = 32;
    public const int PaddingPixels = 4;
    public const int LineWidthPixels = 5;
    public const int SizePixels = CellsPerSide * CellSizePixels;

    public static readonly Color32 LineColor = new(255, 220, 0, 230);
}

internal static class ThermalPipeOverlayAtlasFactory
{
    public static Texture2D CreateTexture()
    {
        Color32[] pixels = new Color32[ThermalPipeOverlayAtlasSpec.SizePixels * ThermalPipeOverlayAtlasSpec.SizePixels];
        for (int index = 0; index < ThermalPipeOverlayAtlasSpec.CellsPerSide * ThermalPipeOverlayAtlasSpec.CellsPerSide; index++)
        {
            DrawCell(pixels, index, (LinkDirections)index);
        }

        Texture2D texture = new(
            ThermalPipeOverlayAtlasSpec.SizePixels,
            ThermalPipeOverlayAtlasSpec.SizePixels,
            TextureFormat.ARGB32,
            mipChain: false)
        {
            name = ThermalPipeOverlayMaterialSpec.AtlasSource,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        return texture;
    }

    private static void DrawCell(Color32[] pixels, int index, LinkDirections directions)
    {
        int originX = index % ThermalPipeOverlayAtlasSpec.CellsPerSide * ThermalPipeOverlayAtlasSpec.CellSizePixels;
        int originY = index / ThermalPipeOverlayAtlasSpec.CellsPerSide * ThermalPipeOverlayAtlasSpec.CellSizePixels;
        int minX = originX + ThermalPipeOverlayAtlasSpec.PaddingPixels;
        int minY = originY + ThermalPipeOverlayAtlasSpec.PaddingPixels;
        int maxX = originX + ThermalPipeOverlayAtlasSpec.CellSizePixels - ThermalPipeOverlayAtlasSpec.PaddingPixels - 1;
        int maxY = originY + ThermalPipeOverlayAtlasSpec.CellSizePixels - ThermalPipeOverlayAtlasSpec.PaddingPixels - 1;
        int centerX = originX + ThermalPipeOverlayAtlasSpec.CellSizePixels / 2;
        int centerY = originY + ThermalPipeOverlayAtlasSpec.CellSizePixels / 2;
        int halfWidth = ThermalPipeOverlayAtlasSpec.LineWidthPixels / 2;
        Color32 color = ThermalPipeOverlayAtlasSpec.LineColor;

        if (directions == LinkDirections.None)
        {
            DrawRect(pixels, centerX - halfWidth, centerY - halfWidth, centerX + halfWidth, centerY + halfWidth, color);
            return;
        }

        DrawRect(pixels, centerX - halfWidth, centerY - halfWidth, centerX + halfWidth, centerY + halfWidth, color);
        if ((directions & LinkDirections.Up) != 0)
        {
            DrawRect(pixels, centerX - halfWidth, centerY, centerX + halfWidth, maxY, color);
        }

        if ((directions & LinkDirections.Right) != 0)
        {
            DrawRect(pixels, centerX, centerY - halfWidth, maxX, centerY + halfWidth, color);
        }

        if ((directions & LinkDirections.Down) != 0)
        {
            DrawRect(pixels, centerX - halfWidth, minY, centerX + halfWidth, centerY, color);
        }

        if ((directions & LinkDirections.Left) != 0)
        {
            DrawRect(pixels, minX, centerY - halfWidth, centerX, centerY + halfWidth, color);
        }
    }

    private static void DrawRect(Color32[] pixels, int minX, int minY, int maxX, int maxY, Color32 color)
    {
        int size = ThermalPipeOverlayAtlasSpec.SizePixels;
        int clampedMinX = Math.Max(0, minX);
        int clampedMinY = Math.Max(0, minY);
        int clampedMaxX = Math.Min(size - 1, maxX);
        int clampedMaxY = Math.Min(size - 1, maxY);
        for (int y = clampedMinY; y <= clampedMaxY; y++)
        {
            for (int x = clampedMinX; x <= clampedMaxX; x++)
            {
                pixels[y * size + x] = color;
            }
        }
    }
}

internal static class ThermalPipeOverlayVisibility
{
    private const string TunnelerLifeCategoryDefName = "TunnelerLife";
    private const string ThermalVentDefName = "TunnelerLife_ThermalVent";

    public static bool ShouldDrawForSelectedCategoryDefName(string? selectedCategoryDefName)
    {
        return selectedCategoryDefName == TunnelerLifeCategoryDefName;
    }

    public static bool ShouldDrawForPlacingDef(BuildableDef? placingDef)
    {
        return placingDef is ThingDef thingDef
            && ShouldDrawForThingDef(thingDef.defName, thingDef.thingClass);
    }

    public static bool ShouldDrawForThingDef(string? defName, Type? thingClass)
    {
        return ThermalPipeUtility.IsThermalNetworkThingClass(thingClass) || defName == ThermalVentDefName;
    }
}
