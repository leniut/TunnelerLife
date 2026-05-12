using TunnelerLife;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class TunnelerLifeSettingsTests
{
    [Fact]
    public void Settings_DefaultsEnableAllFeatures()
    {
        TunnelerLifeSettings settings = new();

        Assert.True(settings.EnableWallRebuilding);
        Assert.True(settings.EnableThermalSystem);
        Assert.True(settings.ShowThermalOverlay);
        Assert.True(settings.ShowVentDirectionMarkers);
        Assert.True(settings.AllowHiddenThermalPipes);
        Assert.True(settings.AllowWaterproofThermalPipes);
        Assert.False(settings.EnableThermalDebugInfo);
        Assert.Equal(TunnelerLifeSettings.DefaultThermostatTolerance, settings.ThermostatTolerance);
        Assert.Equal(TunnelerLifeSettings.DefaultThermalTransferStrength, settings.ThermalTransferStrength);
    }

    [Theory]
    [InlineData("TunnelerLife_Rockfill_Granite", true)]
    [InlineData("TunnelerLife_Rockfill_Slate", true)]
    [InlineData("TunnelerLife_ThermalPipe", false)]
    public void IsWallRebuildingDefName_IdentifiesRockfillDefs(string defName, bool expected)
    {
        Assert.Equal(expected, TunnelerLifeFeatureAvailability.IsWallRebuildingDefName(defName));
    }

    [Theory]
    [InlineData("TunnelerLife_ThermalPipe", true)]
    [InlineData("TunnelerLife_HiddenThermalPipe", true)]
    [InlineData("TunnelerLife_WaterproofThermalPipe", true)]
    [InlineData("TunnelerLife_ThermalPipeSwitch", true)]
    [InlineData("TunnelerLife_ThermalVent", true)]
    [InlineData("TunnelerLife_ThermostaticValve", true)]
    [InlineData("TunnelerLife_Rockfill_Granite", false)]
    public void IsThermalSystemDefName_IdentifiesThermalDefs(string defName, bool expected)
    {
        Assert.Equal(expected, TunnelerLifeFeatureAvailability.IsThermalSystemDefName(defName));
    }

    [Fact]
    public void IsBuildableEnabled_RespectsSettings()
    {
        TunnelerLifeSettings settings = new()
        {
            EnableWallRebuilding = false,
            EnableThermalSystem = true
        };

        Assert.False(TunnelerLifeFeatureAvailability.IsBuildableDefNameEnabled(
            "TunnelerLife_Rockfill_Granite",
            settings));
        Assert.True(TunnelerLifeFeatureAvailability.IsBuildableDefNameEnabled(
            "TunnelerLife_ThermalPipe",
            settings));

        settings.EnableWallRebuilding = true;
        settings.EnableThermalSystem = false;

        Assert.True(TunnelerLifeFeatureAvailability.IsBuildableDefNameEnabled(
            "TunnelerLife_Rockfill_Granite",
            settings));
        Assert.False(TunnelerLifeFeatureAvailability.IsBuildableDefNameEnabled(
            "TunnelerLife_ThermalPipe",
            settings));
    }

    [Fact]
    public void IsBuildableEnabled_RespectsThermalPipeVariantSettings()
    {
        TunnelerLifeSettings settings = new()
        {
            AllowHiddenThermalPipes = false,
            AllowWaterproofThermalPipes = false
        };

        Assert.True(TunnelerLifeFeatureAvailability.IsBuildableDefNameEnabled(
            "TunnelerLife_ThermalPipe",
            settings));
        Assert.False(TunnelerLifeFeatureAvailability.IsBuildableDefNameEnabled(
            "TunnelerLife_HiddenThermalPipe",
            settings));
        Assert.False(TunnelerLifeFeatureAvailability.IsBuildableDefNameEnabled(
            "TunnelerLife_WaterproofThermalPipe",
            settings));
    }

    [Fact]
    public void NormalizeValues_ClampsSliderSettings()
    {
        TunnelerLifeSettings settings = new()
        {
            ThermostatTolerance = 99f,
            ThermalTransferStrength = -1f
        };

        settings.NormalizeValues();

        Assert.Equal(TunnelerLifeSettings.MaxThermostatTolerance, settings.ThermostatTolerance);
        Assert.Equal(TunnelerLifeSettings.MinThermalTransferStrength, settings.ThermalTransferStrength);
    }
}
