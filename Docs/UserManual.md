# LW Camera Plugin: User Manual

🇷🇺 [Русская версия](UserManual.ru.md)

## 1. What it does

Two things, nothing else:

- **Dock camera:** from the cockpit you look out of a connector of your ship. The view is fixed: it shows what is
  in front of the connector, you cannot look around. You fly with W A S D C Space and the mouse, in the
  camera's directions.
  The mouse wheel zooms.
- **Turret camera:** when you take control of a turret, your view moves to the turret's barrel. The mouse aims
  the turret, the wheel zooms. Free aiming exists only here.

Other blocks are not cameras.

## 2. Dock camera

Every connector of the ship you fly is a dock camera. No setup needed. The camera sits 0.1 m in front of the
connector's face and looks straight out of it.

The top of the picture follows your cockpit, like your own view: up on the screen is the ship's up, so the mouse
and W A S D C Space match what you see. A camera looking straight down shows the ship's nose at the top, one
looking straight up shows the tail at the top. How the connector was rotated when you placed it does not matter.

**Controls follow the camera.** While the dock camera is on, the keys and the mouse work in the camera's
directions, whichever side of the ship the connector is on:

| Input | Moves the ship |
|---|---|
| **W / S** | Toward / away from what the camera sees |
| **A / D** | Left / right on the screen |
| **Space / C** | Up / down on the screen |
| **Mouse** | Turns the ship around the camera, as if the camera were the cockpit |
| **Q / E** | Roll around the camera's view axis |

So you dock with any connector the same way: aim with the mouse, close in with W. Switch off "Controls follow dock
camera" in the settings to keep the cockpit's directions. A key you hold while switching the camera on or off keeps
its old direction until you release it.

Sit in a cockpit of the ship:

| Key | Does |
|---|---|
| **Alt+Home** | Dock camera on / off |
| **Esc** | Leave the dock camera (a second Esc opens the game menu) |
| **Alt+PageDown** | Next connector |
| **Alt+PageUp** | Previous connector |
| **Mouse wheel** | Zoom in / out |

A short game message shows which camera you look through, for example **Cam: Back** · 2/6, when the dock camera
turns on and each time you switch. Name your connectors (`Back`, `Top`, `Dock port [CAM]`) so the message and the
list tell them apart.

Connectors are listed A–Z by name. The plugin remembers the last one used per ship.

The dock camera ends by itself when you leave the cockpit, take control of something else or the connector is
destroyed.
While a screen with a mouse cursor is open (terminal, inventory, menus), the plugin's keys and the wheel zoom are off.

**Optional tag.** Put `[CAM]` in a connector's name to adjust its camera. The text before or after the tag is the
camera's name.

| Name | Camera |
|---|---|
| `Dock port [CAM]` | Default view, named "Dock port" |
| `Dock port [CAM 2]` | 2 m in front of the face (−50 to 50) |

The camera always looks straight out of the connector.

Tags are not case-sensitive and work next to other tags, for example LW Cargo Manager's: `Dock [Unload] [CAM]`.
A tag on a block that is not a connector does nothing.

On a ship with many connectors, switch off "All connectors are cameras": then only connectors with `[CAM]`
are in the list. A changed name is picked up the next time you switch the dock camera on.

## 3. Turret camera

Take control of a turret the game's way, for example from a slot on the cockpit shortcut panel. Your view moves to
the turret's barrel and looks where it aims:

- **Mouse** aims the turret. The game turns it at the turret's own speed, so on a fast swing the view follows
  a moment later. The crosshair is always where the shots go.
- **Limits:** the turret turns all the way round, but up and down only within its limits (Gatling turret −40° to
  +90°). The view follows the barrel, so it stops at the same limits.
- **Mouse wheel** zooms. Aiming gets finer as you zoom in.
- **Releasing control** (the game's release key, F) brings your normal view back. **Esc** releases the turret too,
  instead of opening the game menu; a second Esc opens the menu.
- **Gatling turret:** the view sits 0.3 m left of and above the game's own camera point, so the barrel does not
  fill the screen. Shots still go to the crosshair (the shift is under 0.2° at 100 m).

**Shortcut panel:** put a turret's control action on the cockpit panel to jump into it with one key. Name it in
the terminal so you find it quickly, for example `Top [CAM]`; the name before or after the tag is shown in the log.

**Only some turrets:** switch off "All turrets": then only turrets with `[CAM]` in the name get the barrel view,
the others keep the normal view.

**Game view with zoom:** switch off "Barrel view" to keep the game's own turret view. The mouse wheel still zooms
and aiming gets finer as you zoom in. The same applies to turrets without the barrel view because of "All turrets".

## Settings

Pulsar → LW Camera Plugin → settings button. The settings are shown in the game's language (English, Czech,
German, Spanish, French, Italian, Japanese, Korean, Polish, Brazilian Portuguese, Russian, Turkish, Simplified and
Traditional Chinese).

| Setting | Default | |
|---|---|---|
| Camera view on/off, Next, Previous | Alt+Home, Alt+PageDown, Alt+PageUp | Keys |
| Mouse wheel zoom | on | Zoom in the dock camera and the turret camera |
| Wide view (FOV °) | 80 | Field of view when the dock camera is switched on |
| Barrel view | on | Off: the game's own turret view; the wheel still zooms |
| All turrets | on | Off: only turrets with `[CAM]` |
| All connectors are cameras | on | Off: only connectors with `[CAM]` |
| Camera offset (m) | 0.1 | Distance in front of the connector's face |
| Controls follow dock camera | on | W A S D C Space and the mouse work in the camera's directions |
| Verbose log | off | Writes the camera list and camera switching to the game log |

## 4. Turret range and accuracy

**Two different ranges.** A turret has an *auto-aim range*: how far it aims and fires by itself
(Gatling turret: 800 m). The ammo has its own *ammo range*: how far a shot can hit at all. When you control
the turret yourself, only the ammo range counts. In game version 4.0.0.3169 both are 800 m for the Gatling and
rocket weapons, so **nothing hits beyond 800 m**, also under your own control.

- **Gatling ammo is hitscan:** the shot is an instant straight line, no flight time and no bullet drop.
  It hits the first thing on the line, or nothing beyond the ammo range.
- **Rockets fly:** they travel at their speed and explode when they reach their range.

**Spread.** Each shot is turned by a random amount sideways and up/down. The **first shot of a burst is exact**;
the spread grows while you hold fire and is back to zero after a short pause (about 0.3 s).

| Weapon | Auto-aim range | Ammo range | Shot | Full spread | Full after | Max miss at 100 m | at 400 m | at 800 m |
|---|---|---|---|---|---|---|---|---|
| Gatling turret | 800 m | 800 m | hitscan | ±0.60° | 14 shots (~1 s) | ±1.0 m | ±4.2 m | ±8.4 m |
| Gatling cannon (fixed) | – | 800 m | hitscan | ±0.50° | 14 shots (~1 s) | ±0.9 m | ±3.5 m | ±7.0 m |
| Interior turret | 300 m | 300 m | hitscan | ±1.60° | 7 shots (~1 s) | ±2.8 m | – | – |
| Rocket turret | 800 m | 800 m | 200 m/s, 4 s to 800 m | 0° | – | 0 | 0 | 0 |
| Rocket launcher (fixed) | – | 800 m | 200 m/s, 4 s to 800 m | 0° | – | 0 | 0 | 0 |

Rockets have no spread and no drop, but they fly 4 s to 800 m: aim ahead of a moving target.
Fire rates: Gatling 800 rounds/min, interior turret 400, rocket turret 60, rocket launcher 90.

"Max miss" is the worst case at full spread, in each direction. A large-grid ship is a big target at 800 m even at
full spread; a small grid at 800 m needs short bursts.

**Sniping tips**
- Fire **short bursts** (2–4 shots) and pause about a third of a second: every burst starts exact.
- **Zoom in** with the mouse wheel while you control the turret; mouse sensitivity drops with the zoom.
- Past 800 m, fly closer: the shots end at the ammo range.

The table above was read from the game's weapon data (game version 4.0.0.3169). Keen changes weapon balance between
versions, so the numbers can change with game updates.

## 5. Troubleshooting

- **Nothing happens on Alt+Home:** you must sit in a cockpit, and the ship needs a connector (with `[CAM]` in its name
  if "All connectors are cameras" is off). The game log says "No connectors on this grid" otherwise.
- **I see part of the connector:** raise the offset: `[CAM 1]`, or the "Camera offset" setting.
- **Something went wrong:** search the game log for `[LwCameraPlugin]`. After repeated errors the plugin stops
  itself; it starts again when you load a world.
