# Tunneler Life Rockfill Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first Tunneler Life helper: a rockfill construction order that lets mountain colonies rebuild mined-out tunnel cells into natural stone.

**Architecture:** The mod uses normal RimWorld XML for metadata, Architect placement, and build costs. A small C# assembly handles the non-XML part: after colonists finish a temporary rockfill building, it replaces itself with the matching natural stone wall based on the selected stone block stuff.

**Tech Stack:** RimWorld 1.6.4633, XML defs, C# `net472`, Verse/RimWorld assemblies from the local game install, xUnit for pure resolver tests.

---

## File Structure

- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Defs\DesignationCategoryDefs\TunnelerLife_Categories.xml`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Defs\ThingDefs\TunnelerLife_Rockfill.xml`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Languages\English\Keyed\TunnelerLife.xml`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife\TunnelerLife.csproj`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife\RockfillMaterialResolver.cs`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife\Building_Rockfill.cs`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife\PlaceWorker_Rockfill.cs`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife.Tests\RockfillMaterialResolverTests.cs`

## Design Decisions

- The player gets one Architect buildable: `Rockfill`.
- The native RimWorld material selector is used through `stuffCategories: Stony`.
- Construction consumes stone blocks, not chunks, because blocks are native construction stuff and give the material picker the player asked for.
- Finished rockfill becomes natural rough stone: `Granite`, `Limestone`, `Marble`, `Sandstone`, or `Slate`.
- Version 1 does not create overhead mountain roof on open-air cells. If a tunnel still has overhead mountain, the rebuilt rock sits under the existing roof.

### Task 1: Add XML Defs

**Files:**
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Defs\DesignationCategoryDefs\TunnelerLife_Categories.xml`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Defs\ThingDefs\TunnelerLife_Rockfill.xml`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Languages\English\Keyed\TunnelerLife.xml`

- [ ] **Step 1: Create the Architect category**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <DesignationCategoryDef>
    <defName>TunnelerLife</defName>
    <label>Tunneler Life</label>
    <order>245</order>
    <specialDesignatorClasses>
      <li>Designator_Cancel</li>
      <li>Designator_Deconstruct</li>
    </specialDesignatorClasses>
  </DesignationCategoryDef>
</Defs>
```

- [ ] **Step 2: Create the rockfill buildable**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Defs>
  <ThingDef ParentName="BuildingBase">
    <defName>TunnelerLife_Rockfill</defName>
    <label>rockfill</label>
    <description>Rebuilds a mined-out tunnel cell into natural rough stone. Select the stone block material before placing it.</description>
    <thingClass>TunnelerLife.Building_Rockfill</thingClass>
    <category>Building</category>
    <designationCategory>TunnelerLife</designationCategory>
    <altitudeLayer>Building</altitudeLayer>
    <passability>Impassable</passability>
    <fillPercent>1</fillPercent>
    <blockWind>true</blockWind>
    <castEdgeShadows>true</castEdgeShadows>
    <selectable>true</selectable>
    <useHitPoints>true</useHitPoints>
    <holdsRoof>true</holdsRoof>
    <statBases>
      <MaxHitPoints>300</MaxHitPoints>
      <WorkToBuild>800</WorkToBuild>
      <Flammability>0</Flammability>
    </statBases>
    <costStuffCount>25</costStuffCount>
    <stuffCategories>
      <li>Stony</li>
    </stuffCategories>
    <graphicData>
      <texPath>Things/Building/Linked/Wall</texPath>
      <graphicClass>Graphic_Single</graphicClass>
      <drawSize>(1,1)</drawSize>
    </graphicData>
    <constructEffect>ConstructMetal</constructEffect>
    <designationHotKey>Misc1</designationHotKey>
    <placeWorkers>
      <li>TunnelerLife.PlaceWorker_Rockfill</li>
    </placeWorkers>
    <building>
      <isInert>true</isInert>
      <isEdifice>true</isEdifice>
      <claimable>false</claimable>
      <ai_chillDestination>false</ai_chillDestination>
      <deconstructible>true</deconstructible>
    </building>
  </ThingDef>
</Defs>
```

- [ ] **Step 3: Create English keyed text**

```xml
<?xml version="1.0" encoding="utf-8"?>
<LanguageData>
  <TunnelerLife_RockfillCannotPlaceOnBuilding>Rockfill requires an empty cell.</TunnelerLife_RockfillCannotPlaceOnBuilding>
  <TunnelerLife_RockfillCannotPlaceOnWater>Rockfill cannot be placed on deep water.</TunnelerLife_RockfillCannotPlaceOnWater>
  <TunnelerLife_RockfillUnsupportedMaterial>Rockfill supports granite, limestone, marble, sandstone, and slate blocks.</TunnelerLife_RockfillUnsupportedMaterial>
</LanguageData>
```

- [ ] **Step 4: Validate XML loads**

Run:

```powershell
cd "J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife"
[xml](Get-Content ".\Defs\DesignationCategoryDefs\TunnelerLife_Categories.xml")
[xml](Get-Content ".\Defs\ThingDefs\TunnelerLife_Rockfill.xml")
[xml](Get-Content ".\Languages\English\Keyed\TunnelerLife.xml")
```

Expected: no parser errors.

### Task 2: Add Source Projects and Resolver Tests

**Files:**
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife\TunnelerLife.csproj`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife\RockfillMaterialResolver.cs`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife.Tests\RockfillMaterialResolverTests.cs`

- [ ] **Step 1: Write the failing resolver tests**

```csharp
using TunnelerLife;
using Xunit;

namespace TunnelerLife.Tests;

public sealed class RockfillMaterialResolverTests
{
    [Theory]
    [InlineData("BlocksGranite", "Granite")]
    [InlineData("BlocksLimestone", "Limestone")]
    [InlineData("BlocksMarble", "Marble")]
    [InlineData("BlocksSandstone", "Sandstone")]
    [InlineData("BlocksSlate", "Slate")]
    public void ResolveRockDefName_ReturnsNaturalStoneForSupportedBlocks(string stuffDefName, string expectedRockDefName)
    {
        Assert.Equal(expectedRockDefName, RockfillMaterialResolver.ResolveRockDefName(stuffDefName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Steel")]
    public void ResolveRockDefName_ReturnsNullForUnsupportedStuff(string? stuffDefName)
    {
        Assert.Null(RockfillMaterialResolver.ResolveRockDefName(stuffDefName));
    }
}
```

- [ ] **Step 2: Add the test project**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\TunnelerLife\TunnelerLife.csproj" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Add the mod project**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
    <OutputPath>..\..\Assemblies\</OutputPath>
    <RimWorldDir>J:\SteamLibrary\steamapps\common\RimWorld</RimWorldDir>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies.net472" Version="1.0.3" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>$(RimWorldDir)\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>$(RimWorldDir)\RimWorldWin64_Data\Managed\UnityEngine.CoreModule.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
```

- [ ] **Step 4: Run tests to verify failure**

Run:

```powershell
cd "J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife"
dotnet test ".\Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj"
```

Expected: build fails because `RockfillMaterialResolver` does not exist.

- [ ] **Step 5: Add the resolver**

```csharp
namespace TunnelerLife;

public static class RockfillMaterialResolver
{
    public static string? ResolveRockDefName(string? stuffDefName)
    {
        return stuffDefName switch
        {
            "BlocksGranite" => "Granite",
            "BlocksLimestone" => "Limestone",
            "BlocksMarble" => "Marble",
            "BlocksSandstone" => "Sandstone",
            "BlocksSlate" => "Slate",
            _ => null
        };
    }
}
```

- [ ] **Step 6: Run tests to verify pass**

Run:

```powershell
cd "J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife"
dotnet test ".\Source\TunnelerLife.Tests\TunnelerLife.Tests.csproj"
```

Expected: all resolver tests pass.

### Task 3: Add Rockfill Placement and Completion Code

**Files:**
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife\Building_Rockfill.cs`
- Create: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Source\TunnelerLife\PlaceWorker_Rockfill.cs`

- [ ] **Step 1: Add placement validation**

```csharp
using Verse;

namespace TunnelerLife;

public sealed class PlaceWorker_Rockfill : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing? thingToIgnore = null, Thing? thing = null)
    {
        if (!loc.InBounds(map))
        {
            return false;
        }

        if (loc.GetEdifice(map) is not null)
        {
            return "TunnelerLife_RockfillCannotPlaceOnBuilding".Translate();
        }

        TerrainDef terrain = loc.GetTerrain(map);
        if (terrain is not null && terrain.passability == Traversability.Impassable)
        {
            return "TunnelerLife_RockfillCannotPlaceOnWater".Translate();
        }

        return true;
    }
}
```

- [ ] **Step 2: Add completion replacement**

```csharp
using Verse;

namespace TunnelerLife;

public sealed class Building_Rockfill : Building
{
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        if (respawningAfterLoad)
        {
            return;
        }

        string? rockDefName = RockfillMaterialResolver.ResolveRockDefName(Stuff?.defName);
        ThingDef? rockDef = rockDefName is null ? null : DefDatabase<ThingDef>.GetNamedSilentFail(rockDefName);
        if (rockDef is null)
        {
            Messages.Message("TunnelerLife_RockfillUnsupportedMaterial".Translate(), MessageTypeDefOf.RejectInput, historical: false);
            Destroy(DestroyMode.Vanish);
            return;
        }

        IntVec3 cell = Position;
        Destroy(DestroyMode.Vanish);
        GenSpawn.Spawn(rockDef, cell, map, WipeMode.Vanish);
    }
}
```

- [ ] **Step 3: Build the assembly**

Run:

```powershell
cd "J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife"
dotnet build ".\Source\TunnelerLife\TunnelerLife.csproj" -c Release
```

Expected: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Assemblies\TunnelerLife.dll` exists.

### Task 4: Manual Game Verification

**Files:**
- Verify: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\About\About.xml`
- Verify: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Assemblies\TunnelerLife.dll`
- Verify: `J:\SteamLibrary\steamapps\common\RimWorld\Mods\TunnelerLife\Defs\ThingDefs\TunnelerLife_Rockfill.xml`

- [ ] **Step 1: Enable the mod**

Run RimWorld 1.6, enable `Tunneler Life`, restart when RimWorld asks.

Expected: the mod list shows `Tunneler Life` with no red load errors.

- [ ] **Step 2: Place rockfill**

Open any colony map, mine one stone wall cell, open Architect, choose `Tunneler Life`, select `rockfill`, choose granite/limestone/marble/sandstone/slate blocks, and place it on the mined cell.

Expected: the blueprint accepts placement on an empty mined tunnel cell.

- [ ] **Step 3: Finish construction**

Spawn or haul the selected stone blocks, assign construction work, and let a colonist finish the job.

Expected: the temporary rockfill building disappears and the cell becomes rough natural stone matching the selected block material.

- [ ] **Step 4: Mine the rebuilt cell**

Use the vanilla Mine order on the rebuilt cell.

Expected: RimWorld accepts the mine designation, the cell behaves like natural rock, and mining yields the normal stone chunk for that rock type.

## Self-Review

- Spec coverage: The plan covers English metadata, local mod structure, a build-menu rockfill helper, material selection through stone blocks, safe placement, natural rock replacement, and manual game verification.
- Placeholder scan: The plan contains no deferred implementation markers.
- Type consistency: XML class names match C# namespaces: `TunnelerLife.Building_Rockfill` and `TunnelerLife.PlaceWorker_Rockfill`.
