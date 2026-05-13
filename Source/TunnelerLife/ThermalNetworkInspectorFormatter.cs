using System.Globalization;
using System.Text;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Formats thermal network diagnostics for RimWorld inspect panes.
/// </summary>
internal static class ThermalNetworkInspectorFormatter
{
    private const int MaxRows = 6;

    public delegate string Translator(string key, params string[] args);

    public static string FormatSummary(ThermalNetworkDiagnosticSnapshot snapshot, Translator? translator = null)
    {
        StringBuilder builder = new();
        AppendSummaryTo(builder, snapshot, translator ?? Translate);
        return builder.ToString();
    }

    public static string FormatDetails(ThermalNetworkDiagnosticSnapshot snapshot, Translator? translator = null)
    {
        StringBuilder builder = new();
        AppendDetailsTo(builder, snapshot, translator ?? Translate);
        return builder.ToString();
    }

    public static void AppendSummaryTo(StringBuilder builder, ThermalNetworkDiagnosticSnapshot snapshot)
    {
        AppendSummaryTo(builder, snapshot, Translate);
    }

    public static void AppendDetailsTo(StringBuilder builder, ThermalNetworkDiagnosticSnapshot snapshot)
    {
        AppendDetailsTo(builder, snapshot, Translate);
    }

    private static void AppendSummaryTo(
        StringBuilder builder,
        ThermalNetworkDiagnosticSnapshot snapshot,
        Translator translator)
    {
        AppendLineIfNeeded(builder);
        builder.Append(SummaryStatus(snapshot, translator));

        if (snapshot.NetworkCellCount == 0 && snapshot.VentCount == 0 && snapshot.Blockers.Count == 0)
        {
            return;
        }

        AppendLineIfNeeded(builder);
        builder.Append(translator(
            "TunnelerLife_ThermalInspectorSummaryVents",
            snapshot.InputVentCount.ToString(CultureInfo.InvariantCulture),
            snapshot.OutputVentCount.ToString(CultureInfo.InvariantCulture)));
        AppendLineIfNeeded(builder);
        builder.Append(translator(
            "TunnelerLife_ThermalInspectorSummaryRooms",
            snapshot.Rooms.Count.ToString(CultureInfo.InvariantCulture)));
        AppendLineIfNeeded(builder);
        builder.Append(translator(
            "TunnelerLife_ThermalInspectorSummaryBlockers",
            snapshot.Blockers.Count.ToString(CultureInfo.InvariantCulture)));
    }

    private static void AppendDetailsTo(
        StringBuilder builder,
        ThermalNetworkDiagnosticSnapshot snapshot,
        Translator translator)
    {
        AppendLineIfNeeded(builder);
        builder.Append(translator("TunnelerLife_ThermalInspectorHeader"));

        if (snapshot.NetworkCellCount == 0 && snapshot.VentCount == 0 && snapshot.Blockers.Count == 0)
        {
            AppendLineIfNeeded(builder);
            builder.Append(translator("TunnelerLife_ThermalInspectorNoConnectedNetwork"));
            return;
        }

        if (TunnelerLifeFeatureAvailability.ThermalDebugInfoEnabled)
        {
            AppendLineIfNeeded(builder);
            builder.Append(translator(
                "TunnelerLife_ThermalInspectorNetworkCells",
                snapshot.NetworkCellCount.ToString(CultureInfo.InvariantCulture)));
        }

        AppendLineIfNeeded(builder);
        builder.Append(translator(
            "TunnelerLife_ThermalInspectorSourceTemperature",
            FormatTemperature(snapshot.NetworkTemperature, translator)));
        AppendLineIfNeeded(builder);
        builder.Append(translator(
            "TunnelerLife_ThermalInspectorVents",
            snapshot.VentCount.ToString(CultureInfo.InvariantCulture),
            snapshot.InputVentCount.ToString(CultureInfo.InvariantCulture),
            snapshot.OutputVentCount.ToString(CultureInfo.InvariantCulture)));
        AppendLineIfNeeded(builder);
        builder.Append(snapshot.HasInputAndOutput
            ? translator("TunnelerLife_ThermalInspectorFlowPathReady")
            : translator("TunnelerLife_ThermalInspectorFlowPathMissing"));

        AppendRooms(builder, snapshot, translator);
        AppendBlockers(builder, snapshot, translator);
    }

    private static string SummaryStatus(ThermalNetworkDiagnosticSnapshot snapshot, Translator translator)
    {
        if (snapshot.NetworkCellCount == 0 && snapshot.VentCount == 0 && snapshot.Blockers.Count == 0)
        {
            return translator("TunnelerLife_ThermalInspectorSummaryNoConnectedNetwork");
        }

        if (snapshot.InputVentCount == 0)
        {
            return translator("TunnelerLife_ThermalInspectorSummaryMissingInput");
        }

        if (snapshot.OutputVentCount == 0)
        {
            return translator("TunnelerLife_ThermalInspectorSummaryMissingOutput");
        }

        return translator("TunnelerLife_ThermalInspectorSummaryReady");
    }

    private static void AppendRooms(
        StringBuilder builder,
        ThermalNetworkDiagnosticSnapshot snapshot,
        Translator translator)
    {
        AppendLineIfNeeded(builder);
        builder.Append(translator("TunnelerLife_ThermalInspectorConnectedRooms"));
        if (snapshot.Rooms.Count == 0)
        {
            AppendLineIfNeeded(builder);
            builder.Append(translator("TunnelerLife_ThermalInspectorNoConnectedRooms"));
            return;
        }

        int displayedCount = 0;
        for (int index = 0; index < snapshot.Rooms.Count && displayedCount < MaxRows; index++)
        {
            displayedCount++;
            AppendLineIfNeeded(builder);
            builder.Append("- ").Append(FormatRoom(snapshot.Rooms[index], translator));
        }

        AppendMoreLine(builder, snapshot.Rooms.Count - displayedCount, translator);
    }

    private static void AppendBlockers(
        StringBuilder builder,
        ThermalNetworkDiagnosticSnapshot snapshot,
        Translator translator)
    {
        if (snapshot.Blockers.Count == 0)
        {
            return;
        }

        AppendLineIfNeeded(builder);
        builder.Append(translator("TunnelerLife_ThermalInspectorFlowBlockers"));

        int displayedCount = 0;
        for (int index = 0; index < snapshot.Blockers.Count && displayedCount < MaxRows; index++)
        {
            displayedCount++;
            ThermalNetworkBlockerDiagnostic blocker = snapshot.Blockers[index];
            AppendLineIfNeeded(builder);
            builder.Append("- ")
                .Append(blocker.Label)
                .Append(": ")
                .Append(blocker.Status);
        }

        AppendMoreLine(builder, snapshot.Blockers.Count - displayedCount, translator);
    }

    private static string FormatRoom(ThermalNetworkRoomDiagnostic room, Translator translator)
    {
        return translator(
            "TunnelerLife_ThermalInspectorRoomLine",
            room.Label,
            FormatTemperature(room.Temperature),
            FormatRoomFlow(room, translator));
    }

    private static string FormatRoomFlow(ThermalNetworkRoomDiagnostic room, Translator translator)
    {
        if (room.InputVentCount > 0 && room.OutputVentCount > 0)
        {
            return translator(
                "TunnelerLife_ThermalInspectorRoomFlowBoth",
                room.InputVentCount.ToString(CultureInfo.InvariantCulture),
                room.OutputVentCount.ToString(CultureInfo.InvariantCulture));
        }

        if (room.InputVentCount > 0)
        {
            return translator(
                "TunnelerLife_ThermalInspectorRoomFlowInput",
                room.InputVentCount.ToString(CultureInfo.InvariantCulture));
        }

        return translator(
            "TunnelerLife_ThermalInspectorRoomFlowOutput",
            room.OutputVentCount.ToString(CultureInfo.InvariantCulture));
    }

    private static void AppendMoreLine(StringBuilder builder, int moreCount, Translator translator)
    {
        if (moreCount <= 0)
        {
            return;
        }

        AppendLineIfNeeded(builder);
        builder.Append(translator(
            "TunnelerLife_ThermalInspectorMoreRows",
            moreCount.ToString(CultureInfo.InvariantCulture)));
    }

    private static string FormatTemperature(float? temperature, Translator translator)
    {
        return temperature.HasValue
            ? FormatTemperature(temperature.Value)
            : translator("TunnelerLife_ThermalPipeNetworkTemperatureUnavailable");
    }

    private static string FormatTemperature(float temperature)
    {
        return temperature.ToString("0.#", CultureInfo.InvariantCulture) + " C";
    }

    private static void AppendLineIfNeeded(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }
    }

    private static string Translate(string key, params string[] args)
    {
        return args.Length switch
        {
            0 => key.Translate().ToString(),
            1 => key.Translate(args[0]).ToString(),
            2 => key.Translate(args[0], args[1]).ToString(),
            3 => key.Translate(args[0], args[1], args[2]).ToString(),
            _ => key
        };
    }
}
