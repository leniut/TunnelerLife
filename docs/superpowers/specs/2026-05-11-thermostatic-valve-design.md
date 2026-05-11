# Thermostatic Valve V1 Design

## Goal

Add a powered thermostatic valve for thermal pipe networks. The valve should open only when connected rooms can move the controlled side toward the configured target temperature. It must avoid opening a colder source into a room that needs heat, or a hotter source into a room that needs cooling.

## Player Model

The player places a `thermostatic valve` on a thermal pipe network and rotates it. The arrow/sensor side marks the controlled side: the room or connected thermal network segment that the player wants to regulate.

The target temperature is adjusted with the same style of commands used by vanilla coolers and heaters: `-10C`, `-1C`, `Reset`, `+1C`, `+10C`. The valve also has a `Heating / Cooling` mode toggle.

## V1 Scope

- Add a new `thermostatic valve` buildable in the Tunneler Life architect category.
- It is separate from the manual `thermal valve`.
- It requires power through `CompPowerTrader`.
- It uses `CompTempControl` for vanilla-style target temperature gizmos.
- It has a distinct texture and command icon.
- It displays a small power lamp:
  - steady green when powered,
  - blinking red when unpowered.
- It defaults to closed when unpowered or when it cannot determine a useful source.
- Automatic thermal vents are out of scope for V1.

## Temperature Model

Thermal pipes do not store temperature. The valve estimates source temperature from connected open thermal vents and their exposed rooms.

Each thermostatic valve divides the network into two sides:

- `controlled side`: the side pointed to by valve rotation.
- `source side`: all other pipe cells reachable from the valve through open network cells.

For each side, the valve finds connected open thermal vents and groups their exposed rooms. The side temperature is the weighted average of those rooms, using each room's exchange port count. Outdoor rooms may be included as source temperature but are never directly modified by the valve.

## Open/Close Logic

The valve has a target temperature from `CompTempControl` and a hysteresis band to avoid flickering. V1 uses a fixed 1C hysteresis band.

Inside the hysteresis band, the valve keeps its previous open/closed state. If there is no previous state yet, it starts closed.

### Heating Mode

The valve opens when:

- controlled temperature is below `target - 0.5C`, and
- source temperature is warmer than controlled temperature.

The valve closes when:

- controlled temperature is at or above `target + 0.5C`, or
- source temperature is not warmer than controlled temperature, or
- power is off, or
- either side has no measurable room.

Examples with target `21C`:

- controlled `19C`, source `15C`: closed, because source would cool the room.
- controlled `19C`, source `25C`: open, because source would warm the room.
- controlled `20.8C`, source `25C`: keep previous state because the controlled side is inside the hysteresis band.

### Cooling Mode

The valve opens when:

- controlled temperature is above `target + 0.5C`, and
- source temperature is cooler than controlled temperature.

The valve closes when:

- controlled temperature is at or below `target - 0.5C`, or
- source temperature is not cooler than controlled temperature, or
- power is off, or
- either side has no measurable room.

Examples with target `21C`:

- controlled `24C`, source `15C`: open, because source would cool the room.
- controlled `24C`, source `30C`: closed, because source would heat the room.
- controlled `20C`, source `15C`: closed, because the room is already below target.

## Network Behavior

The existing thermal exchange remains bidirectional once the valve is open. The valve does not create one-way airflow. Instead, it decides whether opening a bidirectional connection is useful before allowing that connection to participate in network traversal.

Closed thermostatic valves behave like closed manual thermal valves: they block thermal network traversal through their cell.

Open thermostatic valves behave like thermal pipes: they allow traversal through their cell.

## UI and Inspection

The inspect pane should show:

- mode: heating or cooling,
- target temperature,
- controlled side temperature,
- source side temperature,
- current state: open, closed, no power, no controlled room, no useful source.

Gizmos:

- vanilla-style target temperature commands from `CompTempControl`,
- `Heating / Cooling` mode toggle,
- build copy and reinstall support as normal.

## Graphics

The thermostatic valve should look related to the manual thermal valve but visually distinct. It should include:

- a central valve body,
- a small thermostat/sensor detail,
- a small power lamp overlay.

The lamp is drawn by code so it can change at runtime:

- green steady when powered,
- red blinking when unpowered.

The sprite should not draw full pipe segments to the tile edges. Linked thermal pipe graphics are responsible for full pipe connections under the valve.

## Testing

Tests should cover:

- XML definition includes power, temp control, category, costs, texture paths, and no power-production behavior.
- Target temperature is read from `CompTempControl`.
- Heating logic opens only when source is warmer and controlled side is below target.
- Cooling logic opens only when source is cooler and controlled side is above target.
- Hysteresis prevents rapid open/close toggling around the target.
- No power forces the valve closed.
- Missing controlled or source side forces the valve closed.
- Thermal traversal treats open thermostatic valves as passable and closed thermostatic valves as blockers.
