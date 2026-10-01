<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F03-NATURAL-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07F03NaturalMarkerBattlePlayProbeEditor.cs
authority: selected336B44 formal playable relation hit writer and physics final-state tail; indexed Tayuya36/Naruto2
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F03-NATURAL-SCENE-001.md
-->

# NTSD28-336B44-Q07-F03-NATURAL-SCENE-001

2026-10-01 VERIFIED scoped: Editor-only v2 added native-clock scalar before/after capture and reset at initial fixture boundary, preserving v1. Original Editor compiled zero errors, `tay36-a243-x550-natural-scene-02.json` PASS/DONE and128 samples; offline source/Unity4608/4608 fields zero differences. Scene clean and Play exited; four protected SHA stable, Editor idle. Ledger validator1067 records/6 code files PASS; `git diff --check` PASS. No production/DAT/Scene/config change. The v1 first difference at relative tick7 was fixture phase, not production. Acceptance: `artifacts/diagnostics/NTSD28-336B44-Q07-F03-NATURAL-SCENE-001/ACCEPTANCE.md`.


Created before any Editor script change. Before: F03 local production correction has original Editor focused21/21 and complete-tick2/2; selected source/root natural Tayuya36 action243 vs Naruto2 case matches128 ticks/4480 fields/54 relation events, including tick66 state12→14 marker1→0. Missing original Unity Scene same-state evidence. After: a declared guarded Editor-only diagnostic can run the original Scene production Driver and record both entities/RNG128ticks for offline comparison, without production code or content changes. Anticipated side effects are temporary Play clone and generated diagnostic JSON/meta; saved Scenes/config assets must remain bit-identical. Acceptance, protected paths, no computer-use, limitations and review-only rollback are in the Task. Status stays PLANNED until compiled and executed; no Unity parity claimed yet.

2026-10-01 v1 actual: Editor compile0, original Scene128 ticks, exitedPlay/sceneCleanAfter true, Scene SHA unchanged. Offline source/Unity 4608 fields, first difference relative tick7 target HP Unity491/source500; 392 differences total. Native source/resource cadence debits 9 HP at tick12 whereas Unity Scene was already at global tick5 when fixture actor states were reset, so Unity debited on global tick12/relative tick7. V1 is invalid as a same-state certificate, retained as diagnostic evidence. Before v2 script change, scope remains exactly the declared Editor-only probe and generated meta: reset the three `NativeWorldClock` scalars at initial setup, capture before/after scalar values, use a unique `-02` result path, rerun128 tick and offline compare. Production, DAT, Scene, config and framework are unchanged. Status `IN_PROGRESS`; no F03 closure yet.

V2 code written in the declared `NTSD28Q07F03NaturalMarkerBattlePlayProbeEditor.cs`: `Report` now includes the three clock scalars before/after; `WaitForRoster` resets `Runtime.NativeWorldClock` alongside existing actor/RNG setup; `TryStart` accepts only new `tay36-a243-x550-natural-scene-02`. The generated `.meta` remains Unity-owned. Expected side effect is only a new probe JSON in the diagnostic directory. Verification pending original Editor compile, Scene Play, source/root offline comparison, four protected hashes, ledger and diff check. Review-only rollback of this diagnostic file; preserve v1 result. No production algorithm or content edited.
