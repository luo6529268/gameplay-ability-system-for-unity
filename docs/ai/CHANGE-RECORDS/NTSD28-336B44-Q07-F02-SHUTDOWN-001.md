<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F02-SHUTDOWN-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07F02Kind10BattlePlayProbeEditor.cs
authority: 336B44 formal F02 chain, original Battle Scene ordered shutdown contract and existing Unity shutdown report
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F02-SHUTDOWN-001.md
-->

# NTSD28-336B44-Q07-F02-SHUTDOWN-001

Created before the sole declared script edit. Current F02 run-06 proves combat tick and direct-event parity but only checks absence of a live Driver World after normal Play exit; it does not record ordered shutdown status or pool borrowers. This change only extends the existing Editor diagnostic to call the existing shutdown contract after its 45-tick capture and export the returned counters. The accepted state is all 11 stages complete, World detached, and zero remaining World objects, slots and pool borrowers. If the hard gate fails, the probe must record failure and stay in Play for diagnosis. Task defines scope, validation and forward-only rollback. No gameplay, DAT, Scene or nonbattle edits are authorized by this Record.

2026-10-03 code and build: only the declared F02 Editor probe changed. Its result now records shutdown status/stage/failure, World objects, runtime slots, pool borrowers, active renderer/sprite pool counts, quiescence and World detachment. After the prior 45-tick capture it calls the existing ordered shutdown and map cleanup; a hard-gate failure is saved as `CLEANUP_BLOCKED` without exiting Play. Generated `Assembly-CSharp-Editor.csproj` compiled with 0 errors/265 warnings (`Temp/NTSD28Q07F02ShutdownCompile.log`). Original Editor import, original Battle Scene Play, parity and shutdown postconditions remain pending; generated compilation alone is not the exit.

2026-10-03 bounded exit: original PID105896 Editor was clean/idle Menu, MCP `refresh_unity` imported the changed script and its `Assembly-CSharp-Editor.dll` timestamp advanced after source; one unique request `f02-kind10-scene-20261003-07` entered original Battle Scene Play. Before shutdown, initial+45 ticks again matched formal root 2346/2346 selected fields, seven kind10 applied events and both frame40→41 writes. Existing ordered shutdown returned `Completed / RuntimeMapCleared`, World detached, pool quiesced, and remaining World objects/runtime slots/pool borrowers/active renderer objects/active sprites all 0. Play then exited to clean Menu, no live Driver World and four protected hashes unchanged. [Paired shutdown and parity](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/paired-shutdown-20261003-07.json); [raw run](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-kind10-scene-20261003-07/00056.json). This verifies only the F02 controlled Scenario cleanup; physical keyboard, real Game View pixels and broader F02/Q07/Q12 remain open. No production logic, DAT, Scene, map, camera or nonbattle file changed under this ID.
