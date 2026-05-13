using System;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Creates the command that opens detailed thermal network diagnostics.
/// </summary>
internal static class ThermalNetworkInspectorCommand
{
    public static Gizmo Create(string ownerLabel, Func<ThermalNetworkDiagnosticSnapshot> snapshotFactory)
    {
        return new Command_Action
        {
            defaultLabel = "TunnelerLife_CommandOpenThermalInspectorLabel".Translate(),
            defaultDesc = "TunnelerLife_CommandOpenThermalInspectorDesc".Translate(),
            icon = TexButton.Info,
            action = () => Find.WindowStack.Add(new Dialog_ThermalNetworkInspector(ownerLabel, snapshotFactory()))
        };
    }
}

/// <summary>
/// Modal details view for one thermal network diagnostic snapshot.
/// </summary>
internal sealed class Dialog_ThermalNetworkInspector : Window
{
    private const float TitleHeight = 36f;
    private const float ButtonHeight = 38f;
    private const float Gap = 12f;
    private readonly string title;
    private readonly ThermalNetworkDiagnosticSnapshot snapshot;
    private Vector2 scrollPosition;

    public Dialog_ThermalNetworkInspector(string ownerLabel, ThermalNetworkDiagnosticSnapshot snapshot)
    {
        title = "TunnelerLife_ThermalInspectorDialogTitle".Translate(ownerLabel);
        this.snapshot = snapshot;
        doCloseX = true;
        doCloseButton = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        forcePause = false;
    }

    public override Vector2 InitialSize => new(640f, 520f);

    public override void DoWindowContents(Rect inRect)
    {
        GameFont previousFont = Text.Font;
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, TitleHeight), title);
        Text.Font = GameFont.Small;

        string details = ThermalNetworkInspectorFormatter.FormatDetails(snapshot);
        Rect outRect = new(
            inRect.x,
            inRect.y + TitleHeight + Gap,
            inRect.width,
            inRect.height - TitleHeight - ButtonHeight - Gap * 2f);
        float viewHeight = Math.Max(outRect.height, Text.CalcHeight(details, outRect.width - 16f));
        Rect viewRect = new(0f, 0f, outRect.width - 16f, viewHeight);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        Widgets.Label(viewRect, details);
        Widgets.EndScrollView();

        Text.Font = previousFont;
    }
}
