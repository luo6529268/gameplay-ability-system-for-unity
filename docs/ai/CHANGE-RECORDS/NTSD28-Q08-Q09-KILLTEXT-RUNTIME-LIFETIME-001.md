<!-- CHANGE-RECORD
id: NTSD28-Q08-Q09-KILLTEXT-RUNTIME-LIFETIME-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleWorldCoreScalarSnapshotEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08NativeKnockoutEventStateEditorTests.cs
authority: formal Logan playable GameSession28 conditional knockout-feed prune and BattleWorld28 newest-tail expiry
evidence: artifacts/diagnostics/NTSD28-Q08-Q09-KILLTEXT-LIFETIME-HANDOFF-AUDIT-001/REPORT.md
-->

# NTSD28-Q08-Q09-KILLTEXT-RUNTIME-LIFETIME-001

Created before script modification. Status `PLANNED`, with no code or validation evidence yet. [Task Contract](../../../artifacts/diagnostics/NTSD28-Q08-Q09-KILLTEXT-RUNTIME-LIFETIME-001/TASK-CONTRACT.md) declares exact script ownership, formal authority, current unconditional-70 mismatch, target World publication/reset/snapshot/checksum/parity contract, focused acceptance and rollback.

Expected effects: published optional feed lifetime influences only Q08 post-core record expiry; no hit/KO producer, score, damage, camera, nonbattle UI, Scene, Prefab or content bytes change. The frozen Authority400 v3 trace must remain byte-stable. Versioned Unity snapshot/checksum/extended/lockstep parity must be advanced with exact expectations, not silently accept old payloads. Original Editor is alive with a previously consumed WORDS request but no result; do not issue concurrent NUnit or launch another project.

Pre-edit test-path amendment: existing `BattleStateSnapshotRestoreEditorTests.cs` owns the full aggregate capture/restore fixture; add this exact path before editing it to assert knockout-feed state and checksum restoration rather than trusting scalar capture alone.

2026-09-22 code written: new match-owned `NTSD28NativeKnockoutFeedRuntimeState` stores only `RecordPresent/LifetimeTicks` and resets to absent. During original host battle preparation, the already published and identity-matched Logan catalog's `ModeComboInput.KnockoutFeed` sets those scalars before seal/first tick; this separate idempotent application does not inherit the mode-combo one-world guard. `NTSDBattleTickSystem` now prunes after results only if the record is present, using the published lifetime regardless of bound/mode; negative lifetime remains the existing prune no-op. The scalar is captured/restored by core snapshot (12→13; aggregate 27→28), included in lockstep checksum (30→31), and added to Unity extended/lockstep parity v3. Frozen Authority400 v3 schema and its domain were not edited. SelfCheck's literal extended schema expectation was advanced to v3. Focused existing fixtures now assert immutable scalar capture, aggregate restore/checksum and reset/checksum distinction. No Scene, Prefab, content, nonbattle, renderer, audio, KO producer, score or damage code changed by this package.

Validation: original-project Temp-only absolute-targets `dotnet msbuild Assembly-CSharp-Editor.csproj -t:Build -clp:ErrorsOnly -nologo` returned 0; the targets include new Q08/Q09 Editor test sources omitted by Unity's stale generated csproj. Twelve-path `git diff --check` returned 0. Targeted search found no remaining old extended/lockstep v2 schema literal in `Assets/NTSD/Scripts` or `Tools/NTSD28Parity`. This is **offline compile/static evidence only**; original Editor compilation, focused NUnit, SelfCheck, same-seed full-tick trace, Battle Scene Play and exit/re-entry are pending. Q09 icon identity, complete row projection and Q10 sound are separate open work; R15 versioned trace fixtures must be revisited with the new Unity v3 schemas. Status `CODE_WRITTEN`, not runtime verified.

`Tools/Validate-ChangeLedger.ps1` returned 0 after code and Record updates. It reported only existing unrelated historical declared-but-not-current-diff warnings.
