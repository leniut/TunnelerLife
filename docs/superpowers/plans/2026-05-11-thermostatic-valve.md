# Thermostatic Valve Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a powered thermostatic valve that uses vanilla-style temperature controls and opens only when the source side can move the controlled side toward the configured target.

**Architecture:** Keep the temperature decision in pure C# types so it is easy to test. Add a `Building_ThermostaticValve` that reads power and `CompTempControl`, scans the thermal pipe network on its controlled/source sides, and exposes itself as an open or closed thermal network cell. Reuse existing thermal traversal, valve rendering, and Tunneler Life XML patterns.

**Tech Stack:** RimWorld 1.6 XML defs, Verse/RimWorld C# on .NET Framework 4.7.2, xUnit tests, `System.Drawing` for generated PNG assets.

---

## File Structure

- Create `Source/TunnelerLife/ThermostaticValveMode.cs`: mode enum for heating/cooling.
- Create `Source/TunnelerLife/ThermostaticValveStatus.cs`: inspect/debug status enum.
- Create `Source/TunnelerLife/ThermostaticValveDecisionInput.cs`: pure input DTO.
- Create `Source/TunnelerLife/ThermostaticValveDecision.cs`: pure output DTO.
- Create `Source/TunnelerLife/ThermostaticValveController.cs`: pure decision logic.
- Create `Source/TunnelerLife/ThermalNetworkRoomPort.cs`: map scan result for one exposed room and exchange port count.
- Create `Source/TunnelerLife/ThermalNetworkRoomScanner.cs`: shared room discovery for thermal vent networks.
- Create `Source/TunnelerLife/ThermalNetworkSideTemperatures.cs`: controlled/source temperature summary.
- Create `Source/TunnelerLife/Building_ThermostaticValve.cs`: powered automatic valve building.
- Modify `Source/TunnelerLife/Building_ThermalValve.cs`: make `IsOpen` virtual and allow derived automatic valves.
- Modify `Source/TunnelerLife/ThermalPipeUtility.cs`: include thermostatic valves through existing `Building_ThermalValve` inheritance.
- Modify `Defs/ThingDefs/TunnelerLife_ThermalNetwork.xml`: add `ThingDef` for `TunnelerLife_ThermostaticValve`.
- Modify `Languages/English/Keyed/TunnelerLife.xml`: add mode/status strings.
- Create `Textures/Things/Building/TunnelerLife/ThermostaticValve.png`: map sprite.
- Create `Textures/UI/Commands/ThermostaticValve.png`: build icon.
- Create `Source/TunnelerLife.Tests/ThermostaticValveControllerTests.cs`: pure decision tests.
- Create `Source/TunnelerLife.Tests/ThermalNetworkSideSelectorTests.cs`: pure side start-cell tests.
- Modify `Source/TunnelerLife.Tests/ThermalNetworkXmlTests.cs`: XML and texture assertions.

---

### Task 1: Pure Thermostatic Decision Logic

**Files:**
- Create: `Source/TunnelerLife/ThermostaticValveMode.cs`
- Create: `Source/TunnelerLife/ThermostaticValveStatus.cs`
- Create: `Source/TunnelerLife/ThermostaticValveDecisionInput.cs`
- Create: `Source/TunnelerLife/ThermostaticValveDecision.cs`
- Create: `Source/TunnelerLife/ThermostaticValveController.cs`
- Test: `Source/TunnelerLife.Tests/ThermostaticValveControllerTests.cs`

- [ ] **Step 1: Write failing controller tests**

Create `Source/TunnelerLife.Tests/ThermostaticValveControllerTests.cs`:

```csharp
using TunnelerLife;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermostaticValveControllerTests
{
    [Fact]
    public void Evaluate_HeatingBelowTargetKeepsClosedWhenSourceIsCooler()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Evaluate(new ThermostaticValveDecisionInput(
            ThermostaticValveMode.Heating,
            targetTemperature: 21f,
            controlledTemperature: 19f,
            sourceTemperature: 15f,
            hasPower: true,
            wasOpen: false));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.SourceNotUseful, decision.Status);
    }

    [Fact]
    public void Evaluate_HeatingBelowTargetOpensWhenSourceIsWarmer()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Evaluate(new ThermostaticValveDecisionInput(
            ThermostaticValveMode.Heating,
            targetTemperature: 21f,
            controlledTemperature: 19f,
            sourceTemperature: 25f,
            hasPower: true,
            wasOpen: false));

        Assert.True(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.Open, decision.Status);
    }

    [Fact]
    public void Evaluate_CoolingAboveTargetOpensWhenSourceIsCooler()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Evaluate(new ThermostaticValveDecisionInput(
            ThermostaticValveMode.Cooling,
            targetTemperature: 21f,
            controlledTemperature: 24f,
            sourceTemperature: 15f,
            hasPower: true,
            wasOpen: false));

        Assert.True(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.Open, decision.Status);
    }

    [Fact]
    public void Evaluate_CoolingAboveTargetClosesWhenSourceIsWarmer()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Evaluate(new ThermostaticValveDecisionInput(
            ThermostaticValveMode.Cooling,
            targetTemperature: 21f,
            controlledTemperature: 24f,
            sourceTemperature: 30f,
            hasPower: true,
            wasOpen: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.SourceNotUseful, decision.Status);
    }

    [Fact]
    public void Evaluate_KeepsPreviousStateInsideHysteresisWhenSourceIsUseful()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Evaluate(new ThermostaticValveDecisionInput(
            ThermostaticValveMode.Heating,
            targetTemperature: 21f,
            controlledTemperature: 20.8f,
            sourceTemperature: 25f,
            hasPower: true,
            wasOpen: true));

        Assert.True(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.HysteresisHold, decision.Status);
    }

    [Fact]
    public void Evaluate_NoPowerForcesClosed()
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Evaluate(new ThermostaticValveDecisionInput(
            ThermostaticValveMode.Heating,
            targetTemperature: 21f,
            controlledTemperature: 19f,
            sourceTemperature: 25f,
            hasPower: false,
            wasOpen: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(ThermostaticValveStatus.NoPower, decision.Status);
    }

    [Theory]
    [InlineData(null, 25f, ThermostaticValveStatus.NoControlledRoom)]
    [InlineData(19f, null, ThermostaticValveStatus.NoSourceRoom)]
    public void Evaluate_MissingSideForcesClosed(float? controlledTemperature, float? sourceTemperature, ThermostaticValveStatus expectedStatus)
    {
        ThermostaticValveDecision decision = ThermostaticValveController.Evaluate(new ThermostaticValveDecisionInput(
            ThermostaticValveMode.Heating,
            targetTemperature: 21f,
            controlledTemperature,
            sourceTemperature,
            hasPower: true,
            wasOpen: true));

        Assert.False(decision.IsOpen);
        Assert.Equal(expectedStatus, decision.Status);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore --filter FullyQualifiedName~ThermostaticValveControllerTests
```

Expected: build fails because `ThermostaticValveController` and related types do not exist.

- [ ] **Step 3: Add pure decision types**

Create `Source/TunnelerLife/ThermostaticValveMode.cs`:

```csharp
namespace TunnelerLife;

public enum ThermostaticValveMode
{
    Heating,
    Cooling
}
```

Create `Source/TunnelerLife/ThermostaticValveStatus.cs`:

```csharp
namespace TunnelerLife;

public enum ThermostaticValveStatus
{
    Closed,
    Open,
    HysteresisHold,
    NoPower,
    NoControlledRoom,
    NoSourceRoom,
    SourceNotUseful
}
```

Create `Source/TunnelerLife/ThermostaticValveDecisionInput.cs`:

```csharp
namespace TunnelerLife;

public readonly struct ThermostaticValveDecisionInput(
    ThermostaticValveMode mode,
    float targetTemperature,
    float? controlledTemperature,
    float? sourceTemperature,
    bool hasPower,
    bool wasOpen)
{
    public ThermostaticValveMode Mode { get; } = mode;

    public float TargetTemperature { get; } = targetTemperature;

    public float? ControlledTemperature { get; } = controlledTemperature;

    public float? SourceTemperature { get; } = sourceTemperature;

    public bool HasPower { get; } = hasPower;

    public bool WasOpen { get; } = wasOpen;
}
```

Create `Source/TunnelerLife/ThermostaticValveDecision.cs`:

```csharp
namespace TunnelerLife;

public readonly struct ThermostaticValveDecision(bool isOpen, ThermostaticValveStatus status)
{
    public bool IsOpen { get; } = isOpen;

    public ThermostaticValveStatus Status { get; } = status;
}
```

Create `Source/TunnelerLife/ThermostaticValveController.cs`:

```csharp
namespace TunnelerLife;

public static class ThermostaticValveController
{
    private const float HysteresisBand = 1f;

    public static ThermostaticValveDecision Evaluate(ThermostaticValveDecisionInput input)
    {
        if (!input.HasPower)
        {
            return new ThermostaticValveDecision(false, ThermostaticValveStatus.NoPower);
        }

        if (!input.ControlledTemperature.HasValue)
        {
            return new ThermostaticValveDecision(false, ThermostaticValveStatus.NoControlledRoom);
        }

        if (!input.SourceTemperature.HasValue)
        {
            return new ThermostaticValveDecision(false, ThermostaticValveStatus.NoSourceRoom);
        }

        float controlledTemperature = input.ControlledTemperature.Value;
        float sourceTemperature = input.SourceTemperature.Value;
        float halfBand = HysteresisBand / 2f;

        bool sourceUseful = input.Mode == ThermostaticValveMode.Heating
            ? sourceTemperature > controlledTemperature
            : sourceTemperature < controlledTemperature;

        if (!sourceUseful)
        {
            return new ThermostaticValveDecision(false, ThermostaticValveStatus.SourceNotUseful);
        }

        if (input.Mode == ThermostaticValveMode.Heating)
        {
            if (controlledTemperature < input.TargetTemperature - halfBand)
            {
                return new ThermostaticValveDecision(true, ThermostaticValveStatus.Open);
            }

            if (controlledTemperature >= input.TargetTemperature + halfBand)
            {
                return new ThermostaticValveDecision(false, ThermostaticValveStatus.Closed);
            }
        }
        else
        {
            if (controlledTemperature > input.TargetTemperature + halfBand)
            {
                return new ThermostaticValveDecision(true, ThermostaticValveStatus.Open);
            }

            if (controlledTemperature <= input.TargetTemperature - halfBand)
            {
                return new ThermostaticValveDecision(false, ThermostaticValveStatus.Closed);
            }
        }

        return new ThermostaticValveDecision(input.WasOpen, ThermostaticValveStatus.HysteresisHold);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore --filter FullyQualifiedName~ThermostaticValveControllerTests
```

Expected: all `ThermostaticValveControllerTests` pass.

- [ ] **Step 5: Commit Task 1**

Run:

```powershell
git add Source\TunnelerLife Source\TunnelerLife.Tests\ThermostaticValveControllerTests.cs
git commit -m "test: add thermostatic valve decisions"
```

---

### Task 2: Side Selection and Room Temperature Summaries

**Files:**
- Create: `Source/TunnelerLife/ThermalNetworkRoomPort.cs`
- Create: `Source/TunnelerLife/ThermalNetworkSideTemperatures.cs`
- Create: `Source/TunnelerLife/ThermalNetworkRoomScanner.cs`
- Create: `Source/TunnelerLife.Tests/ThermalNetworkSideSelectorTests.cs`

- [ ] **Step 1: Write failing pure side-selection tests**

Create `Source/TunnelerLife.Tests/ThermalNetworkSideSelectorTests.cs`:

```csharp
using System.Linq;
using TunnelerLife;
using Verse;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class ThermalNetworkSideSelectorTests
{
    [Fact]
    public void GetControlledStartCell_UsesSouthRotatedByValveRotation()
    {
        IntVec3 position = new(10, 0, 10);

        Assert.Equal(new IntVec3(10, 0, 9), ThermalNetworkRoomScanner.GetControlledStartCell(position, Rot4.North));
        Assert.Equal(new IntVec3(9, 0, 10), ThermalNetworkRoomScanner.GetControlledStartCell(position, Rot4.East));
        Assert.Equal(new IntVec3(10, 0, 11), ThermalNetworkRoomScanner.GetControlledStartCell(position, Rot4.South));
        Assert.Equal(new IntVec3(11, 0, 10), ThermalNetworkRoomScanner.GetControlledStartCell(position, Rot4.West));
    }

    [Fact]
    public void GetSourceStartCells_ReturnsOtherThreeCardinalCells()
    {
        IntVec3 position = new(10, 0, 10);

        IntVec3[] sourceCells = ThermalNetworkRoomScanner.GetSourceStartCells(position, Rot4.North).ToArray();

        Assert.DoesNotContain(new IntVec3(10, 0, 9), sourceCells);
        Assert.Contains(new IntVec3(11, 0, 10), sourceCells);
        Assert.Contains(new IntVec3(10, 0, 11), sourceCells);
        Assert.Contains(new IntVec3(9, 0, 10), sourceCells);
    }

    [Fact]
    public void CalculateWeightedTemperature_UsesPortCounts()
    {
        ThermalNetworkRoomPort[] ports =
        [
            new(10f, exchangePortCount: 1),
            new(30f, exchangePortCount: 3)
        ];

        Assert.Equal(25f, ThermalNetworkRoomScanner.CalculateWeightedTemperature(ports), precision: 3);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore --filter FullyQualifiedName~ThermalNetworkSideSelectorTests
```

Expected: build fails because `ThermalNetworkRoomScanner` and `ThermalNetworkRoomPort` do not exist.

- [ ] **Step 3: Add room-port DTO and scanner helpers**

Create `Source/TunnelerLife/ThermalNetworkRoomPort.cs`:

```csharp
namespace TunnelerLife;

internal readonly struct ThermalNetworkRoomPort(float temperature, int exchangePortCount)
{
    public float Temperature { get; } = temperature;

    public int ExchangePortCount { get; } = exchangePortCount;
}
```

Create `Source/TunnelerLife/ThermalNetworkSideTemperatures.cs`:

```csharp
namespace TunnelerLife;

internal readonly struct ThermalNetworkSideTemperatures(float? controlledTemperature, float? sourceTemperature)
{
    public float? ControlledTemperature { get; } = controlledTemperature;

    public float? SourceTemperature { get; } = sourceTemperature;
}
```

Create `Source/TunnelerLife/ThermalNetworkRoomScanner.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TunnelerLife;

internal static class ThermalNetworkRoomScanner
{
    private static readonly IntVec3[] CardinalDirections =
    [
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West
    ];

    public static IntVec3 GetControlledStartCell(IntVec3 valvePosition, Rot4 rotation)
    {
        return valvePosition + IntVec3.South.RotatedBy(rotation);
    }

    public static IEnumerable<IntVec3> GetSourceStartCells(IntVec3 valvePosition, Rot4 rotation)
    {
        IntVec3 controlledStartCell = GetControlledStartCell(valvePosition, rotation);
        foreach (IntVec3 direction in CardinalDirections)
        {
            IntVec3 cell = valvePosition + direction;
            if (cell != controlledStartCell)
            {
                yield return cell;
            }
        }
    }

    public static float? CalculateWeightedTemperature(IReadOnlyList<ThermalNetworkRoomPort> roomPorts)
    {
        int totalPortCount = roomPorts.Sum(port => port.ExchangePortCount);
        if (totalPortCount == 0)
        {
            return null;
        }

        return roomPorts.Sum(port => port.Temperature * port.ExchangePortCount) / totalPortCount;
    }

    public static ThermalNetworkSideTemperatures GetSideTemperatures(Building_ThermostaticValve valve)
    {
        Map map = valve.Map;
        IntVec3 controlledStartCell = GetControlledStartCell(valve.Position, valve.Rotation);
        IEnumerable<IntVec3> sourceStartCells = GetSourceStartCells(valve.Position, valve.Rotation);

        IReadOnlyList<ThermalNetworkRoomPort> controlledPorts = FindRoomPorts(
            map,
            [controlledStartCell],
            cell => cell != valve.Position && ThermalPipeUtility.HasOpenThermalNetworkCellAt(cell, map));
        IReadOnlyList<ThermalNetworkRoomPort> sourcePorts = FindRoomPorts(
            map,
            sourceStartCells,
            cell => cell != valve.Position && ThermalPipeUtility.HasOpenThermalNetworkCellAt(cell, map));

        return new ThermalNetworkSideTemperatures(
            CalculateWeightedTemperature(controlledPorts),
            CalculateWeightedTemperature(sourcePorts));
    }

    public static IReadOnlyList<ThermalNetworkRoomPort> FindRoomPorts(
        Map map,
        IEnumerable<IntVec3> startingCells,
        System.Func<IntVec3, bool> isOpenNetworkCell)
    {
        Dictionary<Room, int> portCounts = [];
        foreach (IntVec3 pipeCell in ThermalPipeNetworkTraversal.FindConnectedCells(startingCells, isOpenNetworkCell))
        {
            AddConnectedVentRooms(pipeCell, map, portCounts);
        }

        return portCounts
            .Select(pair => new ThermalNetworkRoomPort(pair.Key.Temperature, pair.Value))
            .ToArray();
    }

    private static void AddConnectedVentRooms(IntVec3 pipeCell, Map map, Dictionary<Room, int> portCounts)
    {
        foreach (IntVec3 direction in CardinalDirections)
        {
            IntVec3 ventCell = pipeCell + direction;
            if (!ventCell.InBounds(map))
            {
                continue;
            }

            List<Thing> things = ventCell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is Building_ThermalVent vent
                    && vent.IsOpen
                    && vent.ConnectsToPipeCell(pipeCell)
                    && vent.ConnectedRoom is Room room)
                {
                    portCounts.TryGetValue(room, out int currentCount);
                    portCounts[room] = currentCount + 1;
                }
            }
        }
    }
}
```

- [ ] **Step 4: Keep existing vent equalization unchanged**

Do not modify `Source/TunnelerLife/ThermalVentNetworkService.cs` in this task. The new scanner is used by `Building_ThermostaticValve` only. This avoids changing working vent-to-vent equalization while adding the thermostat feature.

- [ ] **Step 5: Run tests**

Run:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore --filter FullyQualifiedName~ThermalNetworkSideSelectorTests
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore
```

Expected: side selector tests pass and the full suite remains green.

- [ ] **Step 6: Commit Task 2**

Run:

```powershell
git add Source\TunnelerLife Source\TunnelerLife.Tests\ThermalNetworkSideSelectorTests.cs
git commit -m "feat: scan thermal valve network sides"
```

---

### Task 3: Thermostatic Valve Building Behavior

**Files:**
- Modify: `Source/TunnelerLife/Building_ThermalValve.cs`
- Create: `Source/TunnelerLife/Building_ThermostaticValve.cs`
- Modify: `Languages/English/Keyed/TunnelerLife.xml`

- [ ] **Step 1: Modify base valve to allow automatic subclasses**

In `Source/TunnelerLife/Building_ThermalValve.cs`, change:

```csharp
public bool IsOpen => FlickUtility.WantsToBeOn(this);
```

to:

```csharp
public virtual bool IsOpen => FlickUtility.WantsToBeOn(this);
```

- [ ] **Step 2: Add thermostatic valve building**

Create `Source/TunnelerLife/Building_ThermostaticValve.cs`:

```csharp
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TunnelerLife;

/// <summary>
/// Powered valve that opens only when the source side moves the controlled side toward its target temperature.
/// </summary>
public sealed class Building_ThermostaticValve : Building_ThermalValve
{
    private static readonly Material PoweredLampMaterial = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 1f, 0.15f), false);
    private static readonly Material UnpoweredLampMaterial = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 0.05f, 0.05f), false);
    private const float LampSize = 0.16f;

    private CompPowerTrader? powerComp;
    private CompTempControl? tempControlComp;
    private ThermostaticValveMode mode = ThermostaticValveMode.Cooling;
    private bool automaticOpen;
    private ThermostaticValveStatus status = ThermostaticValveStatus.NoPower;
    private float? controlledTemperature;
    private float? sourceTemperature;

    public override bool IsOpen => automaticOpen;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        powerComp = GetComp<CompPowerTrader>();
        tempControlComp = GetComp<CompTempControl>();
        EvaluateAutomaticState();
    }

    public override void TickRare()
    {
        base.TickRare();
        EvaluateAutomaticState();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        yield return new Command_Action
        {
            defaultLabel = mode == ThermostaticValveMode.Cooling ? "TunnelerLife_ThermostaticValveModeCooling".Translate() : "TunnelerLife_ThermostaticValveModeHeating".Translate(),
            defaultDesc = "TunnelerLife_ThermostaticValveModeDesc".Translate(),
            icon = TexCommand.DesirePower,
            action = ToggleMode
        };
    }

    public override string GetInspectString()
    {
        string baseInspect = base.GetInspectString();
        string detail = "TunnelerLife_ThermostaticValveInspect".Translate(
            mode.ToString(),
            tempControlComp?.targetTemperature.ToStringTemperature() ?? "-",
            controlledTemperature?.ToStringTemperature() ?? "-",
            sourceTemperature?.ToStringTemperature() ?? "-",
            status.ToString());

        return string.IsNullOrWhiteSpace(baseInspect) ? detail : baseInspect + "\n" + detail;
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        DrawPowerLamp(drawLoc);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref mode, "thermostaticValveMode", ThermostaticValveMode.Cooling);
        Scribe_Values.Look(ref automaticOpen, "thermostaticValveOpen", defaultValue: false);
    }

    private void ToggleMode()
    {
        mode = mode == ThermostaticValveMode.Cooling ? ThermostaticValveMode.Heating : ThermostaticValveMode.Cooling;
        EvaluateAutomaticState();
    }

    private void EvaluateAutomaticState()
    {
        if (!Spawned)
        {
            automaticOpen = false;
            return;
        }

        bool previousOpen = automaticOpen;
        ThermalNetworkSideTemperatures sideTemperatures = ThermalNetworkRoomScanner.GetSideTemperatures(this);
        controlledTemperature = sideTemperatures.ControlledTemperature;
        sourceTemperature = sideTemperatures.SourceTemperature;

        ThermostaticValveDecision decision = ThermostaticValveController.Evaluate(new ThermostaticValveDecisionInput(
            mode,
            tempControlComp?.targetTemperature ?? 21f,
            controlledTemperature,
            sourceTemperature,
            powerComp?.PowerOn == true,
            automaticOpen));

        automaticOpen = decision.IsOpen;
        status = decision.Status;

        if (automaticOpen != previousOpen)
        {
            ThermalPipeMeshUtility.DirtyNetworkCellAndNeighbors(Map, Position);
        }
    }

    private void DrawPowerLamp(Vector3 drawLoc)
    {
        bool powered = powerComp?.PowerOn == true;
        if (!powered && GenTicks.TicksGame % 60 >= 30)
        {
            return;
        }

        Material material = powered ? PoweredLampMaterial : UnpoweredLampMaterial;
        Vector3 position = drawLoc;
        position.y = AltitudeLayer.BuildingOnTop.AltitudeFor();
        position.x += 0.26f;
        position.z += 0.26f;
        Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(LampSize, 1f, LampSize));
        Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
    }
}
```

- [ ] **Step 3: Add language keys**

In `Languages/English/Keyed/TunnelerLife.xml`, add these keys before `</LanguageData>`:

```xml
  <TunnelerLife_ThermostaticValveModeCooling>Cooling mode</TunnelerLife_ThermostaticValveModeCooling>
  <TunnelerLife_ThermostaticValveModeHeating>Heating mode</TunnelerLife_ThermostaticValveModeHeating>
  <TunnelerLife_ThermostaticValveModeDesc>Switch whether this thermostatic valve opens to cool or heat the controlled side.</TunnelerLife_ThermostaticValveModeDesc>
  <TunnelerLife_ThermostaticValveInspect>Mode: {0}\nTarget: {1}\nControlled side: {2}\nSource side: {3}\nStatus: {4}</TunnelerLife_ThermostaticValveInspect>
```

- [ ] **Step 4: Run build**

Run:

```powershell
dotnet build 'Source\TunnelerLife\TunnelerLife.csproj' --no-restore
```

Expected: build succeeds. If `TexCommand.DesirePower` is not available in RimWorld 1.6, replace it with `TexCommand.ForbidOff` and rebuild.

- [ ] **Step 5: Commit Task 3**

Run:

```powershell
git add Source\TunnelerLife\Building_ThermalValve.cs Source\TunnelerLife\Building_ThermostaticValve.cs Languages\English\Keyed\TunnelerLife.xml
git commit -m "feat: add powered thermostatic valve logic"
```

---

### Task 4: XML Definition and Generated Textures

**Files:**
- Modify: `Defs/ThingDefs/TunnelerLife_ThermalNetwork.xml`
- Create: `Textures/Things/Building/TunnelerLife/ThermostaticValve.png`
- Create: `Textures/UI/Commands/ThermostaticValve.png`
- Modify: `Source/TunnelerLife.Tests/ThermalNetworkXmlTests.cs`

- [ ] **Step 1: Add failing XML test**

In `Source/TunnelerLife.Tests/ThermalNetworkXmlTests.cs`, add this test after `ThermalValve_IsFlickableTemperatureTransferCutoff`:

```csharp
[Fact]
public void ThermostaticValve_IsPoweredTempControlledValve()
{
    XElement valveDef = LoadThingDef("TunnelerLife_ThermostaticValve");

    Assert.Equal("thermostatic valve", (string?)valveDef.Element("label"));
    Assert.Equal("TunnelerLife.Building_ThermostaticValve", (string?)valveDef.Element("thingClass"));
    Assert.Equal("TunnelerLife", (string?)valveDef.Element("designationCategory"));
    Assert.Equal("Rare", (string?)valveDef.Element("tickerType"));
    Assert.Equal("RealtimeOnly", (string?)valveDef.Element("drawerType"));
    Assert.Equal("Things/Building/TunnelerLife/ThermostaticValve", (string?)valveDef.Element("graphicData")?.Element("texPath"));
    Assert.Equal("UI/Commands/ThermostaticValve", (string?)valveDef.Element("uiIconPath"));
    Assert.True(File.Exists(Path.Combine(FindModRoot(), "Textures", "Things", "Building", "TunnelerLife", "ThermostaticValve.png")));
    Assert.True(File.Exists(Path.Combine(FindModRoot(), "Textures", "UI", "Commands", "ThermostaticValve.png")));
    Assert.Equal("50", (string?)valveDef.Element("costList")?.Element("Steel"));
    Assert.Equal("2", (string?)valveDef.Element("costList")?.Element("ComponentIndustrial"));
    Assert.Contains(
        valveDef.Descendants("li"),
        element => ((string?)element.Attribute("Class")) == "CompProperties_Power"
            && (string?)element.Element("basePowerConsumption") == "30");
    Assert.Contains(
        valveDef.Descendants("li"),
        element => ((string?)element.Attribute("Class")) == "CompProperties_TempControl"
            && (string?)element.Element("energyPerSecond") == "0");
    Assert.DoesNotContain(
        valveDef.Descendants("li"),
        element => ((string?)element.Attribute("Class")) == "CompProperties_Flickable");
}
```

- [ ] **Step 2: Run XML test to verify it fails**

Run:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore --filter FullyQualifiedName~ThermostaticValve_IsPoweredTempControlledValve
```

Expected: fails because `TunnelerLife_ThermostaticValve` does not exist.

- [ ] **Step 3: Add XML ThingDef**

In `Defs/ThingDefs/TunnelerLife_ThermalNetwork.xml`, add this `ThingDef` immediately after `TunnelerLife_ThermalPipeSwitch`:

```xml
  <ThingDef ParentName="BuildingBase">
    <defName>TunnelerLife_ThermostaticValve</defName>
    <label>thermostatic valve</label>
    <description>A powered thermal pipe valve that opens only when connected rooms can move its controlled side toward the configured target temperature.</description>
    <thingClass>TunnelerLife.Building_ThermostaticValve</thingClass>
    <category>Building</category>
    <designationCategory>TunnelerLife</designationCategory>
    <uiOrder>206</uiOrder>
    <tickerType>Rare</tickerType>
    <drawerType>RealtimeOnly</drawerType>
    <graphicData>
      <texPath>Things/Building/TunnelerLife/ThermostaticValve</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <linkFlags>
        <li>Custom1</li>
      </linkFlags>
      <shaderType>Transparent</shaderType>
      <damageData>
        <rect>(0.2,0.2,0.6,0.6)</rect>
      </damageData>
    </graphicData>
    <uiIconPath>UI/Commands/ThermostaticValve</uiIconPath>
    <building>
      <ai_chillDestination>false</ai_chillDestination>
      <isEdifice>false</isEdifice>
    </building>
    <altitudeLayer>Building</altitudeLayer>
    <passability>Standable</passability>
    <leaveResourcesWhenKilled>false</leaveResourcesWhenKilled>
    <statBases>
      <MaxHitPoints>120</MaxHitPoints>
      <WorkToBuild>260</WorkToBuild>
      <Flammability>0.5</Flammability>
      <Beauty>-2</Beauty>
    </statBases>
    <costList>
      <Steel>50</Steel>
      <ComponentIndustrial>2</ComponentIndustrial>
    </costList>
    <placeWorkers>
      <li>TunnelerLife.PlaceWorker_ThermalPipe</li>
    </placeWorkers>
    <comps>
      <li Class="CompProperties_Power">
        <compClass>CompPowerTrader</compClass>
        <basePowerConsumption>30</basePowerConsumption>
      </li>
      <li Class="CompProperties_TempControl">
        <energyPerSecond>0</energyPerSecond>
      </li>
      <li Class="CompProperties_Breakdownable"/>
    </comps>
    <rotatable>true</rotatable>
    <selectable>true</selectable>
    <neverMultiSelect>false</neverMultiSelect>
    <constructEffect>ConstructMetal</constructEffect>
    <designationHotKey>Misc5</designationHotKey>
    <terrainAffordanceNeeded>Medium</terrainAffordanceNeeded>
    <researchPrerequisites>
      <li>ComplexFurniture</li>
    </researchPrerequisites>
  </ThingDef>
```

- [ ] **Step 4: Generate textures**

Run this PowerShell script:

```powershell
@'
Add-Type -AssemblyName System.Drawing

$root = 'J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife'
$buildingDir = Join-Path $root 'Textures\Things\Building\TunnelerLife'
$uiDir = Join-Path $root 'Textures\UI\Commands'
New-Item -ItemType Directory -Force -Path $buildingDir, $uiDir | Out-Null

$edge = [System.Drawing.Color]::FromArgb(255, 20, 18, 8)
$dark = [System.Drawing.Color]::FromArgb(255, 58, 50, 12)
$fill = [System.Drawing.Color]::FromArgb(255, 151, 128, 13)
$hi = [System.Drawing.Color]::FromArgb(255, 220, 205, 72)
$sensor = [System.Drawing.Color]::FromArgb(255, 90, 130, 150)

function New-Pen($color, [float]$width) {
    $pen = New-Object System.Drawing.Pen($color, $width)
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    return $pen
}

function FillEllipse($g, $color, [float]$x, [float]$y, [float]$w, [float]$h) {
    $b = New-Object System.Drawing.SolidBrush($color)
    $g.FillEllipse($b, $x, $y, $w, $h)
    $b.Dispose()
}

$bitmap = New-Object System.Drawing.Bitmap(128, 128, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bitmap)
$g.Clear([System.Drawing.Color]::Transparent)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::Half

$cx = 64
$cy = 64
$edgePen = New-Pen $edge 17
$fillPen = New-Pen $fill 9
$hiPen = New-Pen $hi 3
$sensorPen = New-Pen $sensor 7

$g.DrawLine($edgePen, 34, $cy, 94, $cy)
$g.DrawLine($edgePen, $cx, 34, $cx, 94)
$g.DrawLine($fillPen, 34, $cy, 94, $cy)
$g.DrawLine($fillPen, $cx, 34, $cx, 94)
$g.DrawLine($hiPen, 34, 61, 94, 61)
$g.DrawLine($hiPen, 61, 34, 61, 94)
FillEllipse $g $edge 40 40 48 48
FillEllipse $g $dark 47 47 34 34
FillEllipse $g $fill 52 52 24 24
$g.DrawLine($hiPen, 50, 64, 78, 64)
$g.DrawLine($hiPen, 64, 50, 64, 78)
$g.DrawLine($sensorPen, 83, 44, 98, 30)
FillEllipse $g $sensor 92 24 14 14

$edgePen.Dispose(); $fillPen.Dispose(); $hiPen.Dispose(); $sensorPen.Dispose(); $g.Dispose()
$bitmap.Save((Join-Path $buildingDir 'ThermostaticValve.png'), [System.Drawing.Imaging.ImageFormat]::Png)

$icon = New-Object System.Drawing.Bitmap(64, 64, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($icon)
$g.Clear([System.Drawing.Color]::Transparent)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.DrawImage($bitmap, 0, 0, 64, 64)
$g.Dispose()
$icon.Save((Join-Path $uiDir 'ThermostaticValve.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$icon.Dispose()
$bitmap.Dispose()
'generated thermostatic valve textures'
'@ | powershell -NoProfile -ExecutionPolicy Bypass -Command -
```

- [ ] **Step 5: Run XML test**

Run:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore --filter FullyQualifiedName~ThermostaticValve_IsPoweredTempControlledValve
```

Expected: test passes.

- [ ] **Step 6: Commit Task 4**

Run:

```powershell
git add Defs\ThingDefs\TunnelerLife_ThermalNetwork.xml Source\TunnelerLife.Tests\ThermalNetworkXmlTests.cs Textures\Things\Building\TunnelerLife\ThermostaticValve.png Textures\UI\Commands\ThermostaticValve.png
git commit -m "feat: define thermostatic valve building"
```

---

### Task 5: Thermal Network Integration and Inspect Text

**Files:**
- Modify: `Source/TunnelerLife/ThermalPipeUtility.cs`
- Modify: `Source/TunnelerLife/ThermalPipeOverlayMapComponent.cs`
- Test: `Source/TunnelerLife.Tests/ThermalPipeUtilityTests.cs`

- [ ] **Step 1: Add utility tests for thermostatic valve classification**

In `Source/TunnelerLife.Tests/ThermalPipeUtilityTests.cs`, add:

```csharp
[Fact]
public void IsThermalNetworkThingClass_IncludesThermostaticValve()
{
    Assert.True(ThermalPipeUtility.IsThermalNetworkThingClass(typeof(Building_ThermostaticValve)));
}
```

- [ ] **Step 2: Run test to verify it fails if utility excludes the class**

Run:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore --filter FullyQualifiedName~IsThermalNetworkThingClass_IncludesThermostaticValve
```

Expected: passes if inheritance already covers it. If it fails, update `ThermalPipeUtility.IsThermalValve` and `IsThermalNetworkThingClass` to use `typeof(Building_ThermalValve).IsAssignableFrom(...)`.

- [ ] **Step 3: Verify overlay visibility includes thermostatic valves**

Review `Source/TunnelerLife/ThermalPipeOverlayMapComponent.cs`. Because it already uses `ThermalPipeUtility.IsThermalValve`, no code change is needed if Task 5 Step 2 passes. If Step 2 required a utility update, rerun overlay geometry tests:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore --filter FullyQualifiedName~ThermalPipeOverlayGeometryTests
```

Expected: overlay geometry tests pass.

- [ ] **Step 4: Run full tests**

Run:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore
```

Expected: all tests pass.

- [ ] **Step 5: Commit Task 5**

Run:

```powershell
git add Source\TunnelerLife Source\TunnelerLife.Tests
git commit -m "test: cover thermostatic valve network utility"
```

---

### Task 6: Final Verification and Manual RimWorld Checklist

**Files:**
- No new files required.

- [ ] **Step 1: Run full automated verification**

Run:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore
dotnet build 'Source\TunnelerLife\TunnelerLife.csproj' -c Release --no-restore
```

Expected:

- test output reports 0 failed tests,
- release build succeeds,
- `Assemblies\TunnelerLife.dll` is updated.

- [ ] **Step 2: Manual in-game smoke test**

Start RimWorld with Tunneler Life enabled and verify:

- Tunneler Life tab contains `thermostatic valve`.
- Build menu shows the new icon.
- The valve requires power.
- Temperature gizmos match cooler-style `-10C`, `-1C`, `Reset`, `+1C`, `+10C`.
- Heating mode with controlled `19C`, source `25C`, target `21C` opens.
- Heating mode with controlled `19C`, source `15C`, target `21C` closes.
- Cooling mode with controlled `24C`, source `15C`, target `21C` opens.
- Cooling mode with controlled `24C`, source `30C`, target `21C` closes.
- Unpowered valve closes and shows blinking red light.
- Powered valve shows steady green light.
- Existing manual thermal valve and thermal vent behavior still works.

- [ ] **Step 3: Commit any final fixes**

If manual testing reveals a fix, make the smallest focused change, rerun:

```powershell
dotnet test 'Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj' --no-restore
dotnet build 'Source\TunnelerLife\TunnelerLife.csproj' -c Release --no-restore
```

Then commit:

```powershell
git add Defs Languages Source Textures docs
git commit -m "fix: polish thermostatic valve behavior"
```

- [ ] **Step 4: Push**

Run:

```powershell
git push origin main
```

Expected: push succeeds to `https://github.com/leniut/TunnelerLife.git`.

---

## Self-Review

- Spec coverage: the plan covers powered valve, cooler-style temperature controls, heating/cooling mode, useful-source logic, hysteresis, no-power behavior, lamp rendering, distinct graphics, XML, tests, and in-game checks.
- Scope: automatic thermal vents are intentionally excluded from this plan.
- Type consistency: the plan uses `Building_ThermostaticValve`, `ThermostaticValveController`, `ThermostaticValveMode`, `ThermostaticValveStatus`, `ThermalNetworkRoomScanner`, and `ThermalNetworkSideTemperatures` consistently across tasks.
