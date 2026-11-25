## What this changes

<!-- One or two sentences. Link the issue: "Fixes #12". -->

## How it was tested

- [ ] `dotnet run --project tests` passes
- [ ] Loaded in the game through Pulsar from a local build
- [ ] World load, back to menu, load again: no plugin errors in the log

## Checklist

- [ ] The change is about the dock camera or turret zoom (one plugin, one job)
- [ ] New decisions are in `ClientPlugin/Policy/` and have a test
- [ ] Every way into the camera view has a way back to the normal view
- [ ] Nothing in the world is changed (client side only)
- [ ] No network access was added, and no decompiled game code
- [ ] Version number and `LwCameraPlugin.xml` are unchanged
- [ ] `Docs/UserManual.md` is updated if players see the change
