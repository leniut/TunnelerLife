using TunnelerLife;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalPipeDiagnosticsTests
{
    [Fact]
    public void AverageNetworkTemperature_ReturnsMeanTemperature()
    {
        Assert.Equal(20f, ThermalNetworkDiagnostics.AverageNetworkTemperature([10f, 20f, 30f]));
    }

    [Fact]
    public void AverageNetworkTemperature_ReturnsNullWhenNoTemperatureIsAvailable()
    {
        Assert.Null(ThermalNetworkDiagnostics.AverageNetworkTemperature([]));
    }

    [Fact]
    public void ThermalPipe_OverridesInspectStringForNetworkDiagnostics()
    {
        Assert.Equal(
            typeof(Building_ThermalPipe),
            typeof(Building_ThermalPipe).GetMethod(nameof(Building_ThermalPipe.GetInspectString))?.DeclaringType);
    }

    [Fact]
    public void ThermalNetworkInspectorFormatter_SummaryKeepsInspectPaneShort()
    {
        ThermalNetworkDiagnosticSnapshot snapshot = CreateDiagnosticSnapshot();

        string inspectText = ThermalNetworkInspectorFormatter.FormatSummary(snapshot, TestTranslate);

        Assert.Contains("Thermal network: ready", inspectText);
        Assert.Contains("Vents: 1 input, 2 output", inspectText);
        Assert.Contains("Rooms: 2", inspectText);
        Assert.Contains("Blockers: 1", inspectText);
        Assert.DoesNotContain("Connected rooms:", inspectText);
        Assert.DoesNotContain("- freezer", inspectText);
    }

    [Fact]
    public void ThermalNetworkInspectorFormatter_DetailsIncludeVentsRoomsAndBlockers()
    {
        ThermalNetworkDiagnosticSnapshot snapshot = CreateDiagnosticSnapshot();

        string inspectText = ThermalNetworkInspectorFormatter.FormatDetails(snapshot, TestTranslate);

        Assert.Contains("Thermal network", inspectText);
        Assert.Contains("Source temperature: 12.5 C", inspectText);
        Assert.Contains("Vents: 3 (1 input, 2 output)", inspectText);
        Assert.Contains("- freezer: -10 C, input x1", inspectText);
        Assert.Contains("- workshop: 21.5 C, output x2", inspectText);
        Assert.Contains("- thermal valve: closed", inspectText);
    }

    [Fact]
    public void ThermalNetworkDiagnosticSnapshot_ReportsMissingTransferPath()
    {
        ThermalNetworkDiagnosticSnapshot snapshot = new(
            networkTemperature: null,
            networkCellCount: 2,
            rooms: [],
            ventCount: 1,
            inputVentCount: 1,
            outputVentCount: 0,
            blockers: []);

        Assert.False(snapshot.HasInputAndOutput);
    }

    [Fact]
    public void ThermalNetworkBuildings_OverrideInspectStringForNetworkDiagnostics()
    {
        Assert.Equal(
            typeof(Building_ThermalVent),
            typeof(Building_ThermalVent).GetMethod(nameof(Building_ThermalVent.GetInspectString))?.DeclaringType);
        Assert.Equal(
            typeof(Building_ThermalValve),
            typeof(Building_ThermalValve).GetMethod(nameof(Building_ThermalValve.GetInspectString))?.DeclaringType);
        Assert.Equal(
            typeof(Building_ThermalPipe),
            typeof(Building_ThermalPipe).GetMethod(nameof(Building.GetGizmos))?.DeclaringType);
        Assert.Equal(
            typeof(Building_ThermalValve),
            typeof(Building_ThermalValve).GetMethod(nameof(Building.GetGizmos))?.DeclaringType);
    }

    [Fact]
    public void ThermalNetworkInspectorDialog_IsWindow()
    {
        Assert.Equal(typeof(Window), typeof(Dialog_ThermalNetworkInspector).BaseType);
    }

    private static ThermalNetworkDiagnosticSnapshot CreateDiagnosticSnapshot()
    {
        return new ThermalNetworkDiagnosticSnapshot(
            networkTemperature: 12.5f,
            networkCellCount: 9,
            rooms:
            [
                new ThermalNetworkRoomDiagnostic("freezer", -10f, inputVentCount: 1, outputVentCount: 0),
                new ThermalNetworkRoomDiagnostic("workshop", 21.5f, inputVentCount: 0, outputVentCount: 2)
            ],
            ventCount: 3,
            inputVentCount: 1,
            outputVentCount: 2,
            blockers:
            [
                new ThermalNetworkBlockerDiagnostic("thermal valve", "closed")
            ]);
    }

    private static string TestTranslate(string key, params string[] args)
    {
        string text = key switch
        {
            "TunnelerLife_ThermalInspectorHeader" => "Thermal network",
            "TunnelerLife_ThermalInspectorSummaryReady" => "Thermal network: ready",
            "TunnelerLife_ThermalInspectorSummaryMissingInput" => "Thermal network: missing input",
            "TunnelerLife_ThermalInspectorSummaryMissingOutput" => "Thermal network: missing output",
            "TunnelerLife_ThermalInspectorSummaryNoConnectedNetwork" => "Thermal network: no connected pipe network",
            "TunnelerLife_ThermalInspectorSummaryVents" => "Vents: {0} input, {1} output",
            "TunnelerLife_ThermalInspectorSummaryRooms" => "Rooms: {0}",
            "TunnelerLife_ThermalInspectorSummaryBlockers" => "Blockers: {0}",
            "TunnelerLife_ThermalInspectorNoConnectedNetwork" => "No connected pipe network.",
            "TunnelerLife_ThermalInspectorNetworkCells" => "Network cells: {0}",
            "TunnelerLife_ThermalInspectorSourceTemperature" => "Source temperature: {0}",
            "TunnelerLife_ThermalInspectorVents" => "Vents: {0} ({1} input, {2} output)",
            "TunnelerLife_ThermalInspectorFlowPathReady" => "Flow path: input and output available",
            "TunnelerLife_ThermalInspectorFlowPathMissing" => "Flow path: missing input or output",
            "TunnelerLife_ThermalInspectorConnectedRooms" => "Connected rooms:",
            "TunnelerLife_ThermalInspectorNoConnectedRooms" => "- none",
            "TunnelerLife_ThermalInspectorRoomLine" => "{0}: {1}, {2}",
            "TunnelerLife_ThermalInspectorRoomFlowInput" => "input x{0}",
            "TunnelerLife_ThermalInspectorRoomFlowOutput" => "output x{0}",
            "TunnelerLife_ThermalInspectorRoomFlowBoth" => "input x{0}, output x{1}",
            "TunnelerLife_ThermalInspectorFlowBlockers" => "Flow blockers:",
            "TunnelerLife_ThermalInspectorMoreRows" => "- and {0} more",
            "TunnelerLife_ThermalPipeNetworkTemperatureUnavailable" => "unavailable",
            _ => key
        };

        return args.Length == 0 ? text : string.Format(text, args);
    }
}
