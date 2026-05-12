using Verse;

namespace TunnelerLife;

/// <summary>
/// Applies persisted settings after RimWorld finishes loading definitions.
/// </summary>
[StaticConstructorOnStartup]
public static class TunnelerLifeStartup
{
    static TunnelerLifeStartup()
    {
        TunnelerLifeFeatureAvailability.ApplySettings(TunnelerLifeMod.Settings);
    }
}
