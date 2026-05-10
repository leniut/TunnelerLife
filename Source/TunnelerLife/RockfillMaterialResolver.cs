using System.Collections.Generic;
using System.Linq;

namespace TunnelerLife;

/// <summary>
/// Resolves stone block def names to the rock def names used by rockfill.
/// </summary>
public static class RockfillMaterialResolver
{
    private static readonly RockfillMaterial[] Materials =
    [
        new("ChunkGranite", "Granite", "TunnelerLife_Rockfill_Granite"),
        new("ChunkLimestone", "Limestone", "TunnelerLife_Rockfill_Limestone"),
        new("ChunkMarble", "Marble", "TunnelerLife_Rockfill_Marble"),
        new("ChunkSandstone", "Sandstone", "TunnelerLife_Rockfill_Sandstone"),
        new("ChunkSlate", "Slate", "TunnelerLife_Rockfill_Slate")
    ];

    private static readonly string[] SupportedStoneChunkDefNames = Materials.Select(material => material.ChunkDefName).ToArray();

    /// <summary>
    /// Gets all supported rockfill material definitions.
    /// </summary>
    public static IReadOnlyList<RockfillMaterial> SupportedMaterials => Materials;

    /// <summary>
    /// Gets the stone chunk def names that rockfill can consume.
    /// </summary>
    public static IReadOnlyList<string> SupportedChunkDefNames => SupportedStoneChunkDefNames;

    /// <summary>
    /// Returns the rock def name represented by a stone chunk def name, or <see langword="null" /> when unsupported.
    /// </summary>
    /// <param name="chunkDefName">The RimWorld stone chunk def name, such as <c>ChunkGranite</c>.</param>
    /// <returns>The matching rock def name, or <see langword="null" /> for null, empty, or unsupported values.</returns>
    public static string? ResolveRockDefNameFromChunkDefName(string? chunkDefName)
    {
        foreach (RockfillMaterial material in Materials)
        {
            if (material.ChunkDefName == chunkDefName)
            {
                return material.RockDefName;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the rock def name represented by a concrete rockfill buildable def name.
    /// </summary>
    /// <param name="rockfillDefName">The rockfill buildable def name, such as <c>TunnelerLife_Rockfill_Granite</c>.</param>
    /// <returns>The matching rock def name, or <see langword="null" /> for null, empty, or unsupported values.</returns>
    public static string? ResolveRockDefNameFromRockfillDefName(string? rockfillDefName)
    {
        foreach (RockfillMaterial material in Materials)
        {
            if (material.RockfillDefName == rockfillDefName)
            {
                return material.RockDefName;
            }
        }

        return null;
    }
}

/// <summary>
/// Defines one supported rockfill construction material.
/// </summary>
public sealed class RockfillMaterial
{
    /// <summary>
    /// Creates a rockfill material mapping.
    /// </summary>
    public RockfillMaterial(string chunkDefName, string rockDefName, string rockfillDefName)
    {
        ChunkDefName = chunkDefName;
        RockDefName = rockDefName;
        RockfillDefName = rockfillDefName;
    }

    /// <summary>
    /// Gets the chunk item consumed by construction.
    /// </summary>
    public string ChunkDefName { get; }

    /// <summary>
    /// Gets the natural rough rock spawned after construction.
    /// </summary>
    public string RockDefName { get; }

    /// <summary>
    /// Gets the concrete rockfill buildable def used by RimWorld blueprints.
    /// </summary>
    public string RockfillDefName { get; }
}
