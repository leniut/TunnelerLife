# Thermal Vents V1 Design

## Goal

Add a passive remote ventilation system for underground colonies. Thermal pipes do not create heat or cold; they connect thermal vents so rooms can exchange temperature over distance.

## Player Model

The intended layout is:

`room A | thermal vent | thermal pipes | thermal vent | room B`

When both vents are open and connected by one pipe network, the connected rooms slowly equalize temperature. Heat moves both ways depending on which room is hotter. A freezer, heater room, hot workshop, or outdoor room can become the source only because a vent exposes that room to the network.

## V1 Scope

- Add `Thermal pipe`, `Hidden thermal pipe`, and `Waterproof thermal pipe` in the Tunneler Life architect category.
- Add `Thermal vent` in the Tunneler Life architect category.
- Thermal pipes are built like conduits, cost one more steel than the matching vanilla conduit, can be dragged, and can share a tile with power conduits.
- Thermal vents are placed in walls like vanilla vents, cost the same as vanilla vents, and can be opened or closed.
- A thermal vent connects only the room on its room-facing side to the pipe on its pipe-facing side.
- The network only works when at least two open thermal vents are connected to the same pipe network and expose at least two rooms.

## Behavior

- A thermal vent is directional.
- The pipe side is the cell north of the vent after rotation.
- The room side is the cell south of the vent after rotation.
- If the pipe-side cell has no thermal pipe, the vent is inactive.
- If the room-side cell has no room, the vent is inactive.
- Once per rare tick, one vent per connected network performs the exchange to avoid duplicate temperature transfer.
- Outdoor rooms can influence the target temperature but are not directly modified.

## Balance

- `Thermal pipe`: 2 steel per cell.
- `Hidden thermal pipe`: 3 steel per cell.
- `Waterproof thermal pipe`: 11 steel per cell.
- `Thermal vent`: 30 steel.
- No power requirement in V1.
- Transfer rate starts close to vanilla vent behavior, then can be tuned after in-game testing.

## Non-Goals

- No powered heat pump.
- No stored pipe temperature.
- No one-way flow.
- No thermostat controls.
- No compatibility patch for third-party pipe systems in V1.
