using System;
using System.Collections.Generic;
using System.Linq;

namespace TunnelerLife;

/// <summary>
/// Calculates passive temperature deltas for rooms connected by a thermal pipe network.
/// </summary>
public static class ThermalExchangeCalculator
{
    /// <summary>
    /// Calculates per-room temperature deltas that move indoor rooms toward the network average.
    /// </summary>
    public static float[] CalculateTemperatureDeltas(
        IReadOnlyList<ThermalRoomState> rooms,
        float rate,
        bool inVacuum)
    {
        if (rooms.Count < 2)
        {
            return rooms.Select(_ => 0f).ToArray();
        }

        float averageTemperature = rooms.Average(room => room.Temperature);
        float[] deltas = new float[rooms.Count];

        for (int i = 0; i < rooms.Count; i++)
        {
            ThermalRoomState room = rooms[i];
            if (room.UsesOutdoorTemperature)
            {
                continue;
            }

            float delta = (averageTemperature - room.Temperature) * rate / room.CellCount;
            if (inVacuum && delta < 0f)
            {
                delta *= 0.1f;
            }

            deltas[i] = ClampToAverage(delta, room.Temperature, averageTemperature);
        }

        return deltas;
    }

    private static float ClampToAverage(float delta, float currentTemperature, float averageTemperature)
    {
        if (delta > 0f)
        {
            return Math.Min(delta, averageTemperature - currentTemperature);
        }

        if (delta < 0f)
        {
            return Math.Max(delta, averageTemperature - currentTemperature);
        }

        return 0f;
    }
}
