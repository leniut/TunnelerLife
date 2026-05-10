namespace TunnelerLife;

/// <summary>
/// Resolves stone block def names to the rock material names used by rockfill.
/// </summary>
public static class RockfillMaterialResolver
{
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
    /// Returns the rock material name represented by a stone block def name, or <see langword="null" /> when unsupported.
    /// </summary>
    /// <param name="blockDefName">The RimWorld stone block def name, such as <c>BlocksGranite</c>.</param>
    /// <returns>The matching rock material name, or <see langword="null" /> for null, empty, or unsupported values.</returns>
    public static string? ResolveRockMaterialName(string? blockDefName)
    {
        return blockDefName switch
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
