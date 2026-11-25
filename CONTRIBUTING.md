# Contributing

Thanks for helping with LW Camera Plugin. Ideas, bug reports and pull requests are all welcome.

## Ideas and bug reports

- **Ideas:** open an issue with the *Suggestion* form. Say what you want to achieve in the game, not only how.
- **Bugs:** open an issue with the *Bug report* form. The plugin version and the lines with `[LwCameraPlugin]`
  from the game log are the most useful part (`%AppData%\SpaceEngineers2\Temp\Logs`).
- Multiplayer is untested until the game releases it.

## What fits this plugin

**One plugin, one job.** This one is a camera: a fixed dock view out of a connector, and wheel zoom while
you control a turret. An idea outside that (cargo, build planning, autopilot ...) belongs in a separate plugin,
so it will be declined here even when it is a good idea. Cameras on other blocks are out of scope for now.

## Pull requests

Please open an issue first for anything bigger than a small fix, so we can agree on the approach.

Rules the code follows:

- **Decisions live in `ClientPlugin/Policy/`**, which never references game, Harmony or Avalonia types.
  Every decision gets a test.
- **The camera is driven only through the game's own public camera API** (transform override and custom FOV).
  No game method is replaced.
- **Every path that turns the camera view on has a path that gives the view back** (`CameraEngine.Exit`).
- **Client side only:** never change the world (block names, settings, ownership).
- **Never commit decompiled game code.**
- **No network access.** Files: only the plugin's settings and the weapon report.

Before you open the pull request:

1. Run the tests: `dotnet run --project tests`. All must pass.
2. Test in the game through Pulsar from your local build: load a world, go back to the menu, load again,
   and check the log for errors.
3. Do not change the version number or `LwCameraPlugin.xml`; the maintainer does that at release.
4. Update `Docs/UserManual.md` if players will see the change.

## Building

See *Building* in the [README](README.md#building). Copy `Directory.Build.props.user.example` to
`Directory.Build.props.user` and set your game folder.

## Licence

By contributing you agree that your work is released under the project's [MIT licence](LICENSE).
