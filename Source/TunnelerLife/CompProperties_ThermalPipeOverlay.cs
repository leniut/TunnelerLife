using Verse;

namespace TunnelerLife;

/// <summary>
/// Properties for drawing thermal pipes on the Architect overlay.
/// </summary>
public sealed class CompProperties_ThermalPipeOverlay : CompProperties
{
    public CompProperties_ThermalPipeOverlay()
    {
        compClass = typeof(CompThermalPipeOverlay);
    }
}
