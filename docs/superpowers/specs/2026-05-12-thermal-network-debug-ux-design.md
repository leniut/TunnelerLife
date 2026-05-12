# Thermal Network Debug UX Design

## Goal

Make thermal networks understandable in-game without adding recurring simulation cost.

## Scope

- Thermal pipes show network temperature and connected vent count in the inspect pane.
- Thermal vents expose one air side and three pipe sides in the inspect pane.
- Thermal vents connect to pipes on any side except the air side.
- Thermal vents can toggle between pulling air from the air side into pipes and pushing pipe air out through the air side.
- Existing thermostatic valve logic keeps selecting the needed source network automatically.

## Behavior

- Pipe network diagnostics are computed only when the pipe inspect string is requested.
- Pipe network temperature is derived from rooms connected through open vents in the same reachable network.
- Vent air side remains the existing outlet cell based on rotation.
- Vent pipe sides are the three remaining adjacent cells.
- Pull mode samples temperature from the air side cell and exposes it to the pipe network.
- Push mode receives network temperature and applies it through the air side cell.

## Out Of Scope

- On-map animated arrows.
- Proportional hot/cold mixing.
- New textures for vents.
