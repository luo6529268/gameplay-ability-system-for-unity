<!-- CHANGE-RECORD
id: NTSD28-Q10-NATIVE-BATTLE-VOLUME-PHYSICAL-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10NativeBattleVolumePhysicalPlayProbeEditor.cs
authority: formal NTSD2.8-Logan playable main.cpp successful-tick F11/F12 priority and paused F2, existing Q10 SFX host implementation
evidence: artifacts/diagnostics/NTSD28-Q10-NATIVE-BATTLE-VOLUME-PHYSICAL-PLAY-001/ACCEPTANCE.md
-->

# NTSD28-Q10-NATIVE-BATTLE-VOLUME-PHYSICAL-PLAY-001

Status `PLANNED`. Task contract: `docs/ai/TASKS/NTSD28-Q10-NATIVE-BATTLE-VOLUME-PHYSICAL-PLAY-001.md`. Before this Change, the original Editor sound percent and host tests pass, while the saved Battle physical-key effect is unverified. Only one new Editor-only test probe and its `.meta` are declared. The probe must not change production logic, DAT values/resources, saved Scenes, the Menu implementation, camera, or nonbattle behavior. It will reuse the production Menu-to-Battle path with an isolated test match and use physical Input System key states. Validation and rollback are defined in the Task; results and exact hashes will be appended here after execution.

2026-09-27 implementation: new probe and `.meta` written at the declared paths. It prewarms Naruto/Sasuke from the original Menu, loads the existing additive Battle with a two-player test match, observes the actual production Driver and AppManager battle SoundPlayer, and injects physical Input System F11/F12/F2 state events across six bounded phases. It checks paused no-tick, successful single-step down/up, F12 priority, and both releases; it releases keys, restores pause/stress flag, and unloads the Battle it loaded. No production or Scene file was edited. Status `CODE_WRITTEN`; Unity import/compile, Play result, Scene hashes and validator are pending.

2026-09-27 original Editor Play acceptance: `refresh_unity` imported/domain-reloaded the new menu probe without observed new script compilation errors. The original PID11944 entered Play from saved Menu, invoked only the Q10 physical volume probe, selected formal `LoganRuntime`, and returned `PASS`: tick2/100 baseline; paused F11 no tick/no change; physical F11+F2 tick3/99; release hold; simultaneous F11+F12 physical priority Up without tick; F11+F12+F2 tick4/100; final release hold. The probe unloaded the additive Battle; MCP stopped Play and observed non-Play Menu. Result SHA, Scene hashes, exact commands and scope are in `artifacts/diagnostics/NTSD28-Q10-NATIVE-BATTLE-VOLUME-PHYSICAL-PLAY-001/ACCEPTANCE.md`. `git diff --check` and the Change Ledger validator exited 0. This Change is `VERIFIED` only for the physical SFX percent path; worker, batched per-cue gain, BGM/WMA and audible/formal comparison remain open under Q10/O-03.
