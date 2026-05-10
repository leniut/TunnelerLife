using Xunit;
using TunnelerLife;

namespace TunnelerLife.Tests;

public sealed class RockfillMaterialResolverTests
{
    [Fact]
    public void SupportedChunkDefNames_ReturnsAllSupportedStoneChunks()
    {
        Assert.Equal(
            [
                "ChunkGranite",
                "ChunkLimestone",
                "ChunkMarble",
                "ChunkSandstone",
                "ChunkSlate"
            ],
            RockfillMaterialResolver.SupportedChunkDefNames);
    }

    [Theory]
    [InlineData("ChunkGranite", "Granite")]
    [InlineData("ChunkLimestone", "Limestone")]
    [InlineData("ChunkMarble", "Marble")]
    [InlineData("ChunkSandstone", "Sandstone")]
    [InlineData("ChunkSlate", "Slate")]
    public void ResolveRockDefNameFromChunkDefName_ReturnsRockDefNameForSupportedStoneChunks(
        string chunkDefName,
        string expectedRockDefName)
    {
        string? rockDefName = RockfillMaterialResolver.ResolveRockDefNameFromChunkDefName(chunkDefName);

        Assert.Equal(expectedRockDefName, rockDefName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("BlocksGranite")]
    [InlineData("Steel")]
    [InlineData("Granite")]
    public void ResolveRockDefNameFromChunkDefName_ReturnsNullForMissingOrUnsupportedChunks(string? chunkDefName)
    {
        string? rockDefName = RockfillMaterialResolver.ResolveRockDefNameFromChunkDefName(chunkDefName);

        Assert.Null(rockDefName);
    }

    [Theory]
    [InlineData("TunnelerLife_Rockfill_Granite", "Granite")]
    [InlineData("TunnelerLife_Rockfill_Limestone", "Limestone")]
    [InlineData("TunnelerLife_Rockfill_Marble", "Marble")]
    [InlineData("TunnelerLife_Rockfill_Sandstone", "Sandstone")]
    [InlineData("TunnelerLife_Rockfill_Slate", "Slate")]
    public void ResolveRockDefNameFromRockfillDefName_ReturnsRockDefNameForConcreteRockfill(
        string rockfillDefName,
        string expectedRockDefName)
    {
        string? rockDefName = RockfillMaterialResolver.ResolveRockDefNameFromRockfillDefName(rockfillDefName);

        Assert.Equal(expectedRockDefName, rockDefName);
    }
}
