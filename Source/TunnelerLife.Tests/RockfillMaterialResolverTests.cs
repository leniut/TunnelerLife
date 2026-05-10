using Xunit;
using TunnelerLife;

namespace TunnelerLife.Tests;

public sealed class RockfillMaterialResolverTests
{
    [Fact]
    public void SupportedStuffDefNames_ReturnsAllSupportedStoneBlocks()
    {
        Assert.Equal(
            [
                "BlocksGranite",
                "BlocksLimestone",
                "BlocksMarble",
                "BlocksSandstone",
                "BlocksSlate"
            ],
            RockfillMaterialResolver.SupportedStuffDefNames);
    }

    [Theory]
    [InlineData("BlocksGranite", "Granite")]
    [InlineData("BlocksLimestone", "Limestone")]
    [InlineData("BlocksMarble", "Marble")]
    [InlineData("BlocksSandstone", "Sandstone")]
    [InlineData("BlocksSlate", "Slate")]
    public void ResolveRockDefName_ReturnsRockDefNameForSupportedStoneBlocks(
        string stuffDefName,
        string expectedRockDefName)
    {
        string? rockDefName = RockfillMaterialResolver.ResolveRockDefName(stuffDefName);

        Assert.Equal(expectedRockDefName, rockDefName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("BlocksWood")]
    [InlineData("Granite")]
    public void ResolveRockDefName_ReturnsNullForMissingOrUnsupportedBlocks(string? stuffDefName)
    {
        string? rockDefName = RockfillMaterialResolver.ResolveRockDefName(stuffDefName);

        Assert.Null(rockDefName);
    }
}
