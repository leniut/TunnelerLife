# Rockfill Orders Designator Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move rockfill into the vanilla Orders workflow with a single drag-capable command and a compact material menu, while keeping the Tunneler Life Architect category for future helpers.

**Architecture:** Keep `TunnelerLife_Rockfill` as the buildable/frame backend. Add a custom `Designator_Rockfill` dropdown to the vanilla `Orders` category; each dropdown option delegates to a `Designator_Build` configured with the selected stone block stuff so RimWorld keeps native blueprints, hauling, and construction.

**Tech Stack:** RimWorld 1.6.4633 XML patches, C# `net472`, Verse/RimWorld designator APIs, xUnit resolver tests.

---

### Task 1: Centralize Supported Materials

- [ ] Add `SupportedStuffDefNames` to `RockfillMaterialResolver`.
- [ ] Add a unit test proving all five supported block defs are exposed.

### Task 2: Add Orders Designator

- [ ] Add `Designator_Rockfill` as a `Designator_Dropdown`.
- [ ] Add one hidden `Designator_Build` child per supported stone block material.
- [ ] Configure each child by setting the build designator stuff to the selected block def.
- [ ] Use a shared rockfill icon.

### Task 3: Patch Vanilla Orders and Add Icon

- [ ] Add a `Patches` XML file that appends `TunnelerLife.Designator_Rockfill` to the vanilla `Orders` category.
- [ ] Add `uiIconPath` to the backend buildable.
- [ ] Create `Textures/UI/Designators/Rockfill.png`.

### Task 4: Verify

- [ ] Parse XML files.
- [ ] Run `dotnet test Source/TunnelerLife.Tests/TunnelerLife.Tests.csproj -v minimal`.
- [ ] Run `dotnet build Source/TunnelerLife/TunnelerLife.csproj -c Release -v minimal`.
- [ ] Confirm `Assemblies/TunnelerLife.dll` exists and Git is clean after commit.
