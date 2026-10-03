<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-F02-BLACK-CELL-ATTRIBUTION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07F02Kind10BattlePlayProbeEditor.cs
authority: 336B44 formal battle visual path and original Unity F02 Game View black-cell observation
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-F02-BLACK-CELL-ATTRIBUTION-001.md
-->

# Q09/F02 black-cell BattleControls attribution

Created before diagnostic code edit. Original state: F02 Editor probe captured two Game View images and matching rule/event traces, but had no way to separate BattleControls UI pixels from battle-rendered pixels at the black cell. Planned sole code path adds opt-in same-tick temporary UI deactivation and a third PNG, with restoration on success/failure. Expected side effects are Play-only UI visibility during a paused simulation tick and one new unique result directory. Invariants: original 45 production ticks and current authority state equal prior run, no persistent Scene/UI mutation, zero-residue shutdown and protected hashes. Rollback is a reviewed forward correction of the diagnostic script. Actual changed symbols, compile/runtime evidence and limitations will be appended after implementation.

2026-10-03 actual code: the sole Editor probe now accepts `captureNoControlsView` only with `captureGameView`; after its ordinary relative-tick39 PNG has been written, it stores Naruto's current catalog source/rect and central pixel mode, temporarily deactivates the original Scene's `BattleControls`, captures a separate same-tick PNG, restores the controls before resuming simulation, and also attempts restoration on failure. The original no-flag and two-image routes retain their prior behavior. No production, Scene, DAT, PNG, shader, camera, UI layout or non-battle code changed. Generated compile, original Editor import/Play, exact state/event comparison and shutdown are still pending.

2026-10-03 compile: `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` succeeded with 0 errors/272 warnings. Original Editor `refresh_unity` completed and domain reloaded; subsequent Editor state was idle, Menu active, non-Play and non-compiling. `read_console` reported no C# error (only an MCP client disconnect notice). Original Battle Play and visual attribution remain pending.

2026-10-03 runtime: original Editor run `f02-black-cell-20261003-01` completed `CAPTURED / DONE` with ordinary tick30/39 plus UI-hidden tick39 PNGs, valid 1920×1080/hash and no error. `CentralOnly` effective pixel mode; `actor.Sprite.CurrentEntry` blank, so exact central command/catalog/slice remains unknown. Removing BattleControls left the near-black rectangle in place: maximum component at `(216,806)` grew from 124×101/9,420 pixels to 127×123/15,146 pixels because the joystick no longer occluded its lower area. `before`, `after`, all 46 samples and 47 events equal prior no-screenshot run-07. UI restored before the remaining ticks; ordered shutdown has zero World/slots/pool borrower/active object/Sprite, pool quiesced and World detached. Play returned to clean Menu, protected SHA stable; live Editor idle/nonPlay. [Evidence](../../../artifacts/diagnostics/NTSD28-336B44-Q09-F02-BLACK-CELL-ATTRIBUTION-001/REPORT.md). `VERIFIED` closes only the UI-exclusion diagnostic, not Q09 appearance or formal EXE pixel parity. Rollback remains a reviewed forward correction of this Editor-only opt-in script.
