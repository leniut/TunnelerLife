using Xunit;
using TunnelerLife;

namespace TunnelerLife.Tests;

public sealed class RockfillMaterialResolverTests
{
    [Theory]
    [InlineData("BlocksGranite", "Granite")]
    [InlineData("BlocksLimestone", "Limestone")]
    [InlineData("BlocksMarble", "Marble")]
    [InlineData("BlocksSandstone", "Sandstone")]
    [InlineData("BlocksSlate", "Slate")]
    public void ResolveRockMaterialName_ReturnsMaterialNameForSupportedStoneBlocks(
        string blockDefName,
        string expectedMaterialName)
    {
        string? materialName = RockfillMaterialResolver.ResolveRockMaterialName(blockDefName);

        Assert.Equal(expectedMaterialName, materialName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("BlocksWood")]
    [InlineData("Granite")]
    public void ResolveRockMaterialName_ReturnsNullForMissingOrUnsupportedBlocks(string? blockDefName)
    {
        string? materialName = RockfillMaterialResolver.ResolveRockMaterialName(blockDefName);

        Assert.Null(materialName);
    }
}
