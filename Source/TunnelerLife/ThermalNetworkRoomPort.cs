using Verse;

namespace TunnelerLife;

/// <summary>
/// A room found next to a reachable thermal pipe network cell.
/// </summary>
internal readonly struct ThermalNetworkRoomPort
{
    public ThermalNetworkRoomPort(IntVec3 networkCell, IntVec3 roomCell, Room room, float temperature)
    {
        NetworkCell = networkCell;
        RoomCell = roomCell;
        Room = room;
        Temperature = temperature;
    }

    public IntVec3 NetworkCell { get; }

    public IntVec3 RoomCell { get; }

    public Room Room { get; }

    public float Temperature { get; }
}
