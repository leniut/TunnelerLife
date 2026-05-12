using RimWorld;
using System.Globalization;
using System.Text;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Marker building for cells that belong to a thermal pipe network.
/// </summary>
public sealed class Building_ThermalPipe : Building
{
    public override string GetInspectString()
    {
        StringBuilder builder = new();
        builder.Append(base.GetInspectString());

        ThermalNetworkDiagnosticSnapshot diagnostics = ThermalNetworkDiagnostics.InspectPipe(this);
        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermalPipeNetworkTemperatureInspect".Translate(
            FormatTemperature(diagnostics.NetworkTemperature)));
        AppendLineIfNeeded(builder);
        builder.Append("TunnelerLife_ThermalPipeConnectedRoomsInspect".Translate(
            diagnostics.ConnectedRoomCount.ToString(CultureInfo.InvariantCulture)));

        return builder.ToString();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        ThermalPipeMeshUtility.DirtyNetworkCellAndNeighbors(map, Position);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        Map map = Map;
        IntVec3 position = Position;
        base.DeSpawn(mode);
        ThermalPipeMeshUtility.DirtyNetworkCellAndNeighbors(map, position);
    }

    private static string FormatTemperature(float? temperature)
    {
        return temperature.HasValue
            ? temperature.Value.ToString("0.#", CultureInfo.InvariantCulture) + " C"
            : "TunnelerLife_ThermalPipeNetworkTemperatureUnavailable".Translate();
    }

    private static void AppendLineIfNeeded(StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
        }
    }
}

internal static class ThermalPipeMeshUtility
{
    public static void DirtyNetworkCellAndNeighbors(Map map, IntVec3 position)
    {
        if (map == null)
        {
            return;
        }

        map.mapDrawer.MapMeshDirty(position, MapMeshFlagDefOf.Things, regenAdjacentCells: true, regenAdjacentSections: false);
    }
}
