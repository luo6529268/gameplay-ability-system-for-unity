<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-MAP-PHYSICAL-PARTICIPANT-BIRTH-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AppParticipantSourceBirthEditorTests.cs
authority: user D-024 physical distance ratio and project map exception; formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033
evidence: docs/ai/TASKS/NTSD28-Q07-D024-MAP-PHYSICAL-PARTICIPANT-BIRTH-001.md; artifacts/diagnostics/NTSD28-Q07-D024-COLLISION-GEOMETRY-DOMAIN-AUDIT-001/MAP-ORIGIN-AND-BIRTH-DOMAIN-CONTRACT-20260929.md
-->

# NTSD28-Q07-D024-MAP-PHYSICAL-PARTICIPANT-BIRTH-001

Status: `FOCUSED_TEST_PASS / MENU_TO_BATTLE_PLAY_PENDING`.

Original state: production menu initialization selects a project-stage physical spawn point but passes the same X/Z into `SourceRuleX/Z`; `SyncParticipantBirthPosition` is also used by existing diagnostic fixtures whose raw source=physical setup must stay stable. `BattleSpatialProjection` and the World read surface already exist, but no birth caller uses its inverse mapping.

Planned change: add one explicitly physical-map sync path in AppManager and use it only for production menu initialization; retain the existing raw diagnostic path. Add focused identity/configured positive/negative physical-origin tests to the existing App participant test class. No other code or asset ownership.

Expected side effects: only menu-born participants' source carrier changes under an enlarged view; physical placement, velocity, roster, RNG, project walkable region and existing raw diagnostic fixtures remain unchanged. Fractional inverse values are expected. Stage-formal-source birth and collision are not covered by this package. Reversal is a narrow inverse patch to the declared two scripts, preserving unrelated dirty work.

Actual change (2026-09-29): `AppManager.InitializeBattleParticipants` now calls `SyncParticipantPhysicalBirthPosition` after setting physical `PS`. The new path reads `RegisteredWorldForSimulation.SpatialProjection`, inverse-projects physical X/Z around shared origin zero and delegates to the pre-existing source sync. The raw-source `SyncParticipantBirthPosition` keeps its behavior; its Z parameter widens from `int` to `double` to retain fractional inverse Z. The existing Editor test class gains two configured/identity cases with positive and negative coordinates, overwritten birth history, physical/runtime/source fields, velocity and both RNG streams. No DAT, map, camera, mode, Scene, or nonbattle script was edited by this package.

Validation so far: `dotnet build Assembly-CSharp.csproj --no-restore -nologo -v:q` exited 0 (53 existing warnings, 0 errors); `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -clp:ErrorsOnly` exited 0 (214 warnings, 0 errors). The original project Editor PID 11944 accepted one `refresh_unity(force/all/compile=request)` via its prior port 6401; `Editor.log` then recorded `*** Tundra build success`, both `Library/ScriptAssemblies/Assembly-CSharp*.dll` advanced to 16:02, and no CS errors were reported. `Tools/Validate-ChangeLedger.ps1` exited 0, `Change ledger validation PASSED` (1015 records, 45 governed code files in current shared dirty diff); targeted `git diff --check` exited 0. Original Editor port 6401 is not yet listening after domain reload; port 6402 belongs to another Unity process/project and was not used for tests. This is compile evidence, not a focused runtime pass.

Protection check after compile: Battle Scene SHA-256 `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`, unchanged this turn; Menu Scene `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`, changed concurrently before this compile and left untouched; SunagakureMap `F7B5E4A44CAC05480D1CA6F67ABF623531264C1C96725D7FDD23DA50C8E60C08`, GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82` unchanged. No DAT write was made. Pending: the four targeted original-Editor EditMode cases (two old raw + two new physical), then menu-to-battle Play only after original Editor reconnect and live Scene state verification; stage birth and shared collision geometry remain separate Q07 tasks.

Focused validation (2026-09-29): original Editor PID 11944 recovered its own port 6401, idle/non-Play with `NTSD_Battle.unity` active and newest script assemblies. First exact `groupNames=[NTSD.Test.NTSD28AppParticipantSourceBirthEditorTests]` job `0df004f0ef8d43ad8096efbbf03cb00f` encountered an MCP TestJobManager initialization timeout during Editor recovery (`completed=0`, `result=null`); Editor.log later showed a Test Runner start/finish, but no attributable count, so this attempt is not counted. Once Editor was stable, the same single-class job `22a3685ad5b14032987761b7a7ea69ba` finished `succeeded / Passed`: total 4, passed 4, failed 0, skipped 0 (old raw two + new physical two). No broad suite was run. Afterward original Editor remained idle/non-Play in Battle Scene; `git diff --check` passed and the five protected Scene/map/config hashes listed above remained identical. This closes the focused unit behavior only. Direct menu→battle random birth and proportional collision Play remain pending; Q07 stays open.
