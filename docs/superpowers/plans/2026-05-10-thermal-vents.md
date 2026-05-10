# Thermal Vents V1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a passive vent-to-vent thermal pipe network for Tunneler Life.

**Architecture:** Add marker buildings for thermal pipes and directional thermal vents. A vent performs one flood fill across adjacent thermal pipe cells, finds connected open vents, and one deterministic vent per network equalizes the exposed rooms.

**Tech Stack:** RimWorld XML defs, C# net472, Verse/RimWorld APIs, xUnit tests.

---

### Task 1: Thermal Exchange Math

**Files:**
- Create: `Source/TunnelerLife/ThermalRoomState.cs`
- Create: `Source/TunnelerLife/ThermalExchangeCalculator.cs`
- Test: `Source/TunnelerLife.Tests/ThermalExchangeCalculatorTests.cs`

- [ ] **Step 1: Write failing tests for passive equalization**

Add tests proving hot and cold rooms move toward the average, outdoor rooms are not modified, and fewer than two rooms gives no deltas.

- [ ] **Step 2: Run tests and verify they fail**

Run `dotnet test Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj --no-restore -v minimal`.

- [ ] **Step 3: Implement minimal pure calculator**

Implement `ThermalRoomState` and `ThermalExchangeCalculator.CalculateTemperatureDeltas`.

- [ ] **Step 4: Run tests and commit**

Run tests, then commit `feat: add thermal exchange calculator`.

### Task 2: Runtime Network Logic

**Files:**
- Create: `Source/TunnelerLife/Building_ThermalPipe.cs`
- Create: `Source/TunnelerLife/Building_ThermalVent.cs`
- Create: `Source/TunnelerLife/ThermalVentNetworkService.cs`
- Create: `Source/TunnelerLife/PlaceWorker_ThermalPipe.cs`

- [ ] **Step 1: Implement marker pipe and directional vent**

`Building_ThermalVent` exposes `PipeCell`, `RoomCell`, `IsOpen`, and `ConnectedRoom`.

- [ ] **Step 2: Implement flood fill**

`ThermalVentNetworkService` starts from the vent pipe cell, walks adjacent thermal pipes, finds open vents whose pipe side touches the network, and applies the calculator once per network.

- [ ] **Step 3: Implement pipe place worker**

Block another thermal pipe or thermal pipe blueprint on the same cell, but do not block power conduits.

- [ ] **Step 4: Build and commit**

Run `dotnet build Source\TunnelerLife\TunnelerLife.csproj -c Release -v minimal`, then commit `feat: add thermal vent network logic`.

### Task 3: Defs and XML Guards

**Files:**
- Create: `Defs/ThingDefs/TunnelerLife_ThermalNetwork.xml`
- Create: `Source/TunnelerLife.Tests/ThermalNetworkXmlTests.cs`
- Modify: `README.md`

- [ ] **Step 1: Write failing XML tests**

Assert regular, hidden, and waterproof pipes have no power comp, cost one more steel than matching vanilla conduits, use conduit-style dragging, allow overlap with power, and the vent uses `Building_ThermalVent` with `CompProperties_Flickable`.

- [ ] **Step 2: Add XML defs**

Add `Thermal pipe`, `Hidden thermal pipe`, `Waterproof thermal pipe`, and `Thermal vent` to the Tunneler Life category with English labels/descriptions.

- [ ] **Step 3: Update README**

Document rockfill and thermal vents in one short feature list.

- [ ] **Step 4: Run tests/build and commit**

Run tests and Release build, then commit `feat: add thermal vent defs`.

### Task 4: Final Verification

**Files:**
- Verify all touched files.

- [ ] **Step 1: Run full tests**

Run `dotnet test Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj -v minimal --no-restore`.

- [ ] **Step 2: Run Release build**

Run `dotnet build Source\TunnelerLife\TunnelerLife.csproj -c Release -v minimal`.

- [ ] **Step 3: Check git and push**

Run `git status --short`, commit remaining docs if needed, and push `main`.
