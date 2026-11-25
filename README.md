# LW Camera Plugin

🇷🇺 [Русская версия](README.ru.md)

**A dock camera for Space Engineers 2.** Sit in the cockpit, press **Alt+Home** and look out of your ship's
connector while you dock. The mouse wheel zooms. When you take control of a turret, the view moves to its barrel.

A client plugin for [Pulsar](https://github.com/SpaceGT/Pulsar). Client side only: nothing in the world changes.

![Version](https://img.shields.io/badge/version-0.1.0.0-blue) ![Game](https://img.shields.io/badge/Space%20Engineers%202-client%20plugin-orange) ![License](https://img.shields.io/badge/license-MIT-green)

> **First version (0.1.0.0)**, tested in single player. **New here? Read the [User Manual](Docs/UserManual.md)**
> ([на русском](Docs/UserManual.ru.md)).

---

## Features

| | |
|---|---|
| **Dock camera** | Every connector of the ship you fly is a camera. A fixed view of what is in front of the connector; no looking around. |
| **Controls follow the camera** | In the dock camera W flies where the camera looks and the screen's up is the ship's up, for a connector on any side. |
| **Mouse-wheel zoom** | 80° down to 6°, like the SE1 camera block. |
| **Turret camera** | Take control of a turret and your view is on its barrel: the mouse aims, the wheel zooms, the turret's angle limits apply. Free aiming exists only on turrets. |
| **Optional tag** | `[CAM]` in a connector's name names its camera; `[CAM 2]` moves it 2 m out. |
| **Turret ranges** | Auto-aim range, ammo range and spread of the game's weapons. See [Turret range and accuracy](Docs/UserManual.md#4-turret-range-and-accuracy). |
| **13 languages** | The settings follow the game's language. |
| **Safe** | Uses the game's own camera override. Leaving the cockpit, switching grid or a destroyed connector returns the normal view. |

Other blocks are not cameras in this version.

## Quick start

1. Install the plugin and start your world.
2. Sit in the cockpit of a ship with a connector and press **Alt+Home**. You now look out of the connector.
3. Mouse wheel zooms, **Alt+PageDown** / **Alt+PageUp** switch connectors, **Alt+Home** or **Esc** goes back.

Keys can be changed in the plugin settings (Pulsar → plugin → settings button).

## Installation

From Pulsar's plugin list once it's published on PluginHub-SE2. Until then: build locally (below) and
set `Pulsar` in `Directory.Build.props.user`, so the DLL is copied to `Pulsar/Modern/Local` after each build.

## Building

Requirements: .NET 10 SDK, Space Engineers 2, Pulsar.

1. Copy `Directory.Build.props.user.example` to `Directory.Build.props.user` and set `Game2`
   (the folder with `SpaceEngineers2.exe`) and optionally `Pulsar`.
2. `dotnet build` (or build `LwCameraPlugin.sln` in Rider / Visual Studio).
3. Tests for the game-independent logic: `dotnet run --project tests`.

## How it works

The game's render camera (`CameraComponent`) has a public transform override and a custom FOV, used by the
game itself for spectator look-around and aim-down-sights. The plugin sets the override every frame from the
connector's pivot (the centre of its connecting face) and removes it when you leave the dock camera. A Harmony
postfix on `Session.Update` drives everything. Small prefixes make the controls follow the dock camera, slow the
turret aim while zoomed, let Esc leave the camera and clear the plugin's state when a world closes; no game method
is replaced.

## License

MIT, see [LICENSE](LICENSE).
