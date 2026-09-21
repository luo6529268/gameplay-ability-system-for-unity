<!-- CHANGE-RECORD
id: NTSD28-Q07-STAGED-FORMAL-CALLER-PLAY-001
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCallerPlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/NTSD28B11SourceCallerPlaySetup.cs
authority: D-023 formal Logan content and Q02 production caller/ordered shutdown contracts
evidence: real Play q07-formal-app-2 and q07-formal-menu-2 PASS; previous and current owner resources zero survivors
-->

# NTSD28-Q07-STAGED-FORMAL-CALLER-PLAY-001

Status: `VERIFIED` for the declared Editor Play caller/lifecycle scope; Q07 batch remains open.

Before: the Q02 probe only creates and expects its temporary three-object `E3_probe_*` source. Q07 staged bytes, identity and full Editor publication passed, but no actual Play caller/Unity lifecycle proof exists. Production GameConfig root is empty.

Planned exact changes: add an independent Q07 request path to the existing Editor poll and BeforeSceneLoad setup. For Q07, select the staged project-local root and fixed representative formal IDs 0/50/52; assert formal content fingerprint and real definition selection. Reuse existing App/menu execution, resource tracking, ordered shutdown/unload and report fields. Preserve the Q02 fixture path and assertions verbatim in its branch.

Expected side effects: one task-owned request JSON under Temp, diagnostic JSON reports, transient Play objects and content resources. No persistent GameConfig, Scene, original resources, production scripts, third-party or nonbattle edits. Acceptance and rollback follow the Task Contract. Exact Play/compile/Scene/ledger evidence to append after implementation; Player build and final Q07 exit stay pending.

Actual script change: the Editor poll gives a requested Q07 JSON priority over the existing Q02 JSON, holds the selected path while marking it consumed, and for Q07 selects the project-local staged root and representative 0/50/52 IDs. The Play setup reads the same request and still clones the existing GameConfig. The Run path preserves Q02 isolated-source assertions, while Q07 requires non-E3 formal definitions and the formal semantic fingerprint before the existing ordered shutdown/unload and resource-survivor checks. Play and compilation are pending.

First actual Play `q07-formal-app-1` entered the staged root and wrote a real report but failed the existing 1,800-render-frame `ProductionStressServicesReady` deadline while the full content was still loading; World remained Preparing with zero objects. Editor log showed `BattleTestBootstrap` starting but no completion/error before probe cancellation. Concurrent initial request-file write also produced a transient sharing violation in the poll callback. This RED does not show a content or shutdown failure. The Q07-only probe now waits up to 240 seconds of real time for full prewarm, while Q02 retains its 1,800-frame loop; transient request read `IOException` returns to the next Editor update. Re-run Q07 to distinguish slow load from actual initialization failure.

Fresh `q07-formal-app-2` PASS: staged formal identity selected, actual App caller reached Running with four World objects, three owners agreed on publication key, 29,400 owned resources were tracked, ordered shutdown reached RuntimeMapCleared, zero pool borrowers and zero tracked survivors after unload, and runtime stayed Stopped after two frames. Fresh second Play `q07-formal-menu-1` PASS: menu prewarm completed, rebuilt owner used candidate cache once, App battle reached four World objects and its final owner had zero tracked survivors. Scene SHA remained unchanged. The original menu report does not count resources from the previous prewarm owner destroyed during rehydration; add a Q07-only old-owner capture/zero assertion before claiming complete menu resource cleanup, then rerun the exact menu path.

Final exact menu replay `q07-formal-menu-2` PASS after adding the previous-owner resource capture: previous owner zero tracked survivors on real Play destruction, rebuilt owner menuReady=true/cache hit1, World4 and final owner zero tracked survivors after ordered shutdown/unload; borrowers0, two-frame Stopped. No production code was changed. The complete scoped result and initial RED are recorded in `STAGED-CALLER-PLAY-ACCEPTANCE.md`. Scene SHA remained protected; production GameConfig root remains empty. Fresh compile had no C# errors in recent Editor log; test script diff check passed. Player packaging and formal natural-skill behavior are unverified.
