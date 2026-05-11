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
- Thermal vents cost the same as vanilla vents, can be opened or closed, and use rotation to mark the room outlet.
- A thermal vent can be placed by normal building rules and connects to any adjacent thermal pipe.
- The network only works when at least two open thermal vents are connected to the same pipe network and expose at least two rooms.

## Behavior

- A thermal vent has a directional room outlet.
- The outlet is the cell south of the vent after rotation.
- A thermal pipe may touch the vent from any cardinal side.
- If no adjacent cell has a thermal pipe, the vent is inactive.
- If the outlet cell has no room, the vent is inactive.
- Multiple vents exposing the same room count as multiple exchange ports, so three vents exchange temperature about three times as strongly as one vent.
- Once per rare tick, one vent per connected network performs the exchange to avoid duplicate temperature transfer.
- Outdoor rooms can influence the target temperature but are not directly modified.
- Opening the Tunneler Life architect category highlights only thermal pipes with a yellow pipe overlay; it does not enable RimWorld's power-grid overlay.

## Balance

- `Thermal pipe`: 2 steel per cell.
- `Hidden thermal pipe`: 4 steel per cell.
- `Waterproof thermal pipe`: 20 steel per cell.
- `Thermal vent`: 30 steel.
- No power requirement in V1.
- Transfer rate starts close to vanilla vent behavior, then can be tuned after in-game testing.

## Non-Goals

- No powered heat pump.
- No stored pipe temperature.
- No one-way flow.
- No thermostat controls.
- No compatibility patch for third-party pipe systems in V1.
