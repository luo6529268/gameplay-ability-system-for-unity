P8-C is improved and compiles, but production acceptance is not complete.

Changed files:

- `BattleRenderingAcceptanceHarness.cs`
- `BattleRenderingAcceptanceEditorTests.cs`
- `BattleRenderingAcceptanceWindow.cs`
- `LF2ObjectPool.cs`
- `SimulationWorld.Registry.partial.cs`
- `BattlePresentationShadowBuild.cs`
- `BattleCentralRenderSystem.cs`
- `BattleCentralPresentationMountRegistry.cs`

No documentation or P8-D benchmark files were modified by this work.

Verification:

- Runtime and Editor assemblies reloaded with zero new C# errors.
- `git diff --check` passed.
- Deterministic synthetic matrix: `PASS` at `Temp/P8-C-PostFix-EditMode/P8-C-report.json`.
- Requested production outside Play Mode: correctly failed closed.
- Automatic Play Mode request executed, but failed production acceptance at `Temp/P8-C-Resume-Live/P8-C-report.json`.

The live failure is substantive:

- Pool expanded from 4 available objects to 5 checkouts.
- No newly checked-out logic entity obtained a valid runtime handle.
- No production weapon command existed in the published frame.
- Consequently, command resolution and visible production pixels were not proven.

The current live harness skips initialization performed by `LF2ObjectPointFactory`, including character module initialization/binding and weapon setup. Therefore this cannot honestly be reported as only a scene prerequisite or as accepted production behavior.

Architect verification was also not completed: the configured Codex model was unavailable, and the supported fallback timed out. Synthetic coverage and compile status are green; live P8-C production acceptance remains open.