using System.Collections.Generic;

namespace TunnelerLife;

/// <summary>
/// Resolves stone block def names to the rock def names used by rockfill.
/// </summary>
public static class RockfillMaterialResolver
{
    private static readonly string[] SupportedStoneBlockDefNames =
    [
        BlocksGranite,
        BlocksLimestone,
        BlocksMarble,
        BlocksSandstone,
        BlocksSlate
    ];

    private const string BlocksGranite = "BlocksGranite";
    private const string BlocksLimestone = "BlocksLimestone";
    private const string BlocksMarble = "BlocksMarble";
    private const string BlocksSandstone = "BlocksSandstone";
    private const string BlocksSlate = "BlocksSlate";

    private const string Granite = "Granite";
    private const string Limestone = "Limestone";
    private const string Marble = "Marble";
    private const string Sandstone = "Sandstone";
    private const string Slate = "Slate";

    /// <summary>
    /// Gets the stone block stuff def names that rockfill can convert into rough natural stone.
    /// </summary>
    public static IReadOnlyList<string> SupportedStuffDefNames => SupportedStoneBlockDefNames;

    /// <summary>
    /// Returns the rock def name represented by a stone block stuff def name, or <see langword="null" /> when unsupported.
    /// </summary>
    /// <param name="stuffDefName">The RimWorld stone block stuff def name, such as <c>BlocksGranite</c>.</param>
    /// <returns>The matching rock def name, or <see langword="null" /> for null, empty, or unsupported values.</returns>
    public static string? ResolveRockDefName(string? stuffDefName)
    {
        return stuffDefName switch
        {
            BlocksGranite => Granite,
            BlocksLimestone => Limestone,
            BlocksMarble => Marble,
            BlocksSandstone => Sandstone,
            BlocksSlate => Slate,
            _ => null
        };
    }
}
