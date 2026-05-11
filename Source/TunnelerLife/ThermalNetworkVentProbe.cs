using Verse;

namespace TunnelerLife;

/// <summary>
/// Direct adjacency check between a thermal network cell and a possible vent cell.
/// </summary>
internal readonly struct ThermalNetworkVentProbe
{
    public ThermalNetworkVentProbe(IntVec3 networkCell, IntVec3 ventCell)
    {
        NetworkCell = networkCell;
        VentCell = ventCell;
    }

    public IntVec3 NetworkCell { get; }

    public IntVec3 VentCell { get; }
}
