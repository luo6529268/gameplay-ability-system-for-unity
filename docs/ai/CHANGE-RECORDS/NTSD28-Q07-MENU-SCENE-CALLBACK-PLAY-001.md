<!-- CHANGE-RECORD
id: NTSD28-Q07-MENU-SCENE-CALLBACK-PLAY-001
status: BLOCKED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07MenuSceneCallbackPlayProbeEditor.cs
authority: formal Logan content selected by serialized GameConfig; existing Menu Scene and AppManager callback chain
evidence: q07-menu-scene-callback-1/2 actual Play FAIL; empty EditorBuildSettings; negative preflight; RESULT.md
-->

# NTSD28-Q07-MENU-SCENE-CALLBACK-PLAY-001

Before: prior serialized-menu caller probe only creates a prewarm controller and directly invokes `InitializeBattleAsync` on an active probe Scene. It cannot prove actual `NTSD_Menu.unity` callback stages, additive load or Menu return. Formal root and both Scene SHA values are captured in the Task before change.

Planned exact behavior: add one guarded Editor-only test probe with a unique JSON request/result. It refuses dirty/wrong starting Scene or existing result; opens the existing Menu Scene in-memory, enters Play, calls actual Scene component callbacks through the current Menu chain, observes formal publication and battle, unloads via AppManager, exits Play and restores Battle Scene. No production menu/battle script, serialized Scene, prefab, GameConfig or resource edit. Callback-driven evidence is narrower than physical input/visual UX, and Q07 remains open until all required exits.

Rollback: remove/revert only the new test script and `.meta` after preserving its report if this probe is superseded; no Scene/resource rollback is expected. If automatic Scene restoration fails, stop and inspect the current Editor state without destructive filesystem commands. Validation: fresh Unity compile, actual single Menu Play report, Editor state, Scene hashes, Change Ledger and diff check. Append exact results and status after running.

Written: added only the declared `NTSD28Q07MenuSceneCallbackPlayProbeEditor.cs` and matching `.meta`. Editor request preflight checks unique result, saved Battle Scene and formal content; it opens the existing Menu Scene in memory, invokes real GameStart pointer callback, observes native-content prewarm, invokes existing mode/role/CMC/settings callbacks, verifies additive battle/formal publication, unloads via AppManager and restores the Battle Scene after Play. It marks request false instead of deleting files. The probe does not synthesize physical keyboard events, so its acceptance remains callback-chain scope. Unity compile and Play pending.

First actual Play `q07-menu-scene-callback-1.json`: FAIL at `mode`, after formal prewarm returned true and before VS panel appeared within the probe's original 10-second wait. This is a first observed timeout, not proof of a production Menu defect: `LoadingPrewarmController.Start` waits for its progress-text queue to drain after `IsPrewarmed`. Request became false; Editor returned idle/outside Play on the original Battle Scene; both Scene file hashes remained unchanged. The original failure report remains intact. Only the declared test script was refined to record the pending progress-text count and set a bounded timeout from that queue length; no UI/App production behavior changed. Fresh compile and same-path rerun pending.

Actual rerun `q07-menu-scene-callback-2.json`: formal prewarm completed with 749 pending text entries, then VS mode, Naruto OID2 role/team, CMC and Fight component callbacks passed. At `battle`, Unity logged `Scene 'NTSD_Battle' couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded`, originating at `AppManager.LoadBattleAdditive`; `ProjectSettings/EditorBuildSettings.asset` indeed has `m_Scenes: []`. The test timed out before `BattleRunning`; no additive Battle/World/ordered-exit proof exists. New read-only enabled-Scene preflight in the same declared test script returns that exact reason without entering Play; `q07-menu-scene-preflight-1.json` confirms it. Fresh Editor import/compile returned idle with no new `error CS` in the current log tail. Both actual requests false, Editor idle on original Battle Scene and both Scene SHA values unchanged. `RESULT.md` holds the scope and evidence. This test-only package is blocked on production Scene closure; Q07 and the master goal continue through other work. No production UI/App, ProjectSettings or resource change was made.
