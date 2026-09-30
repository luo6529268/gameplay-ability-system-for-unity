<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-DIRECT-BATTLE-BIRTH-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
authority: user D-024 unified battle spatial projection and selected 336B44 release; Unity direct Battle Scene bootstrap adapter
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-DIRECT-BATTLE-BIRTH-001.md
-->

# NTSD28-336B44-Q07-D024-DIRECT-BATTLE-BIRTH-001

Created before script edits. The [Task](../TASKS/NTSD28-336B44-Q07-D024-DIRECT-BATTLE-BIRTH-001.md) fixes one direct-Scene physical-birth adapter call. Its evidence is the original Editor's natural physical Attack reaching Naruto frame 513 with zero candidates while visually adjacent participants have inconsistent source X/Z. Expected after state is the same registered physical spawn point mapped through the World projection at birth, as the existing menu route already does. No claim of formal-release native scene equivalence is made; this is the user's Unity spatial exception.

Invariants: no DAT, Scene, spawn configuration, camera, AppManager, collision writer, result writer, nonbattle flow or GAS framework change. The change is one reversible call substitution; test the exact physical Battle scenario and preserve original failed XMLs. Validation and any new first difference will be appended here immediately after the edit.

Code written: `BattleTestBootstrap.SetupTestCharacters` now passes its existing physical spawn X/Z to `AppManager.SyncParticipantPhysicalBirthPosition`, which performs the World projection before setting source-rule X/Z. The old direct call interpreted those physical numbers as source values. No other script or runtime rule was changed under this ID. Original Editor compile and natural Play are pending; status `CODE_WRITTEN`.

Original Editor refreshed/recompiled the changed script and ran the exact existing `NTSD28Q08DirectBattleNaturalKoTwoCyclePlayModeTests.PhysicalPunchKoDrivesOneRematchThenSameWorldSelection` physical-keyboard natural Play test. Terminal Unity XML `artifacts/diagnostics/NTSD28-336B44-Q08-C008-DEFERRED-RESULT-EXIT-001/fourth-natural-play-passed.xml` SHA-256 `5D6351A90D3BD2E88C1089E453483F8F337E261CC29F999D266D094C1ED5C431` reports 1/1 PASS, duration 319.542966 s. This test requires a real attack knockout, first direct rematch into a different World, a second real knockout, held result input at timer144 leaving 350/transition0, following host pass transition2 without World frame advancement, then upper selection transition1 without a second rematch. The same test failed three times at zero attack candidates before the one-line birth correction. The original Editor ended non-Play/idle; Battle Scene `isDirty=false`. Four protected Battle/Menu/GameConfig/ProjectBattleModeConfig file SHA-256 values are unchanged and recorded in the Q08 report. `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` passed after the one-line edit (1032 records, 73 governed diff code files), and scoped `git diff --check` passed. This verifies the direct-Scene birth adapter at this natural Play gate only; Q07 as a whole and formal-root same-state result parity remain open.
