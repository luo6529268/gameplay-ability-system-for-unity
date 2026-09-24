<!-- CHANGE-RECORD
id: NTSD28-USER-SOURCE-COORDINATE-PARTICIPANT-BIRTH-001
status: RUNTIME_PENDING
change-kind: D024_PARTICIPANT_ABSOLUTE_SOURCE_BIRTH
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AppParticipantSourceBirthEditorTests.cs
authority: user D-024 ratio decision; paired playable GameSession28 participant SpawnRequest28 and BattleWorld28 spawn_at; active Unity AppManager.SetupBattleCharacters
evidence: docs/ai/TASKS/NTSD28-USER-SOURCE-COORDINATE-PARTICIPANT-BIRTH-001.md
-->

# NTSD28-USER-SOURCE-COORDINATE-PARTICIPANT-BIRTH-001

2026-09-24 implementation and focused evidence: extracted only the existing `SyncIntegerPosition -> RefreshRuntimeSnapshot` pair into internal `AppManager.SyncParticipantBirthPosition`; the real `SetupBattleCharacters` call site retains preceding PS writes, velocities and HitStun in place. With only that behavior-preserving extraction, original Editor RED job `8748c0dd646245b8bae7dec60792ed9f` failed both factor1/configured-view cases because source initialization remained false. The final seam adds `SetSourceRulePosition(spawnX, spawnZ)` and `SyncSourceRuleIntegerPosition` between existing battle integer sync and Refresh. Original Editor compile/domain reload and focused GREEN `416518993b5742c997d9e360185130da` passed2/2 in0.673s. Each case tests initial raw absolute placement301.75/220 followed by -42.75/290 overwriting old source history, independent of configured2048/1152 view. Battle precise/int, velocities0.1/0/0.1, HitStun75, deterministic RNG state and native synchronized call count remain unchanged by the seam.

Exact authored scripts: `AppManager.cs` and new `NTSD28AppParticipantSourceBirthEditorTests.cs`; Unity generated test meta GUID `445b69ac457121444aacb2bfdb69e642`, unique among Assets meta files. No production menu decision, random draw, resource or scene changed. Paired formal `game_session.cpp:2169+` passes combatant absolute X/Y/Z into SpawnRequest28; this bounded check does not certify matching stage selection. `Tools/Validate-ChangeLedger.ps1` PASS747, scoped diff whitespace PASS, Scene SHA `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0` unchanged, Content/Config/Scene/ProjectSettings scoped status clear. Log: `Temp/NTSD28-USER-SOURCE-COORDINATE-PARTICIPANT-BIRTH-001-ledger.log`.

Status `RUNTIME_PENDING / FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE`: tests execute the exact seam called by the production setup, but real menu-to-Battle Play has not run. No full SelfCheck or Play test was claimed. Rollback removes only the seam's source initialization or restores the original sync/refresh call site and this new test/meta, preserving unrelated work. Remaining source history categories and consumers remain gated. Following pre-script paragraph is retained as historical plan.

Pre-script status PLANNED. Unity currently selects a raw absolute participant spawn X/Z, writes physical `PS` X/Z and integer mirrors but leaves independent source-rule position absent. This package adds source precise and integer initialization at that existing battle-host placement point without changing selected values, RNG, velocities, DAT, Scene, camera or nonbattle flow. Expected side effect is versioned/checksummed source-rule state for spawned participants. A focused test must first fail and then pass on the real placement-sync method; true AppManager menu-to-battle Play acceptance remains separate. Task lists exact validation and rollback. No source-rule gameplay reader may activate under this ID.
