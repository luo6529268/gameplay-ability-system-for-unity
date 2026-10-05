<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-HITFA1-3-DEAD-MOTION-GATE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs
diagnostic-source-path: artifacts/diagnostics/NTSD28-336B44-Q07-HITFA1-3-DEAD-MOTION-GATE-20261005/native_dead_tracking_witness.cpp
authority: formal 336B44 paired native_ai common target then current_hp<=0 guard
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-HITFA1-3-DEAD-MOTION-GATE-001.md
-->

# 共用1/3非正HP运动门

脚本前PLANNED。完整Task预置scope/风险/验收/恢复；先两端实测，不改DAT/非战斗/其它共用分支。

2026-10-05T14:55:29.691985+00:00 Test-first exactly two methods (3 cases) and new native two-mode single full-Core step witness written; production untouched. Native/RED pending.


2026-10-05T15:02:12.605191+00:00 Native build exit0, current declared 28 Core CPP closure matched, 74 inputs unchanged. Actual full Driver one step for behavior1 and3 exit0: sourceX500/Y-100/Z600/V0/HP0 preserved; behavior1 serial frame4/counter1, behavior3 frame54/counter1. Generated RED build exit0/301 warnings/0 errors. Original Editor MCP refreshed, both corresponding DLLs fresh, clean Battle/nonPlay/idle. RED exact3 started job77c232993d844af493fc38a014a5c81b; no production change. Evidence files native-compile-result.json/native-mode{1,3}-run-result.json/editor-red-launch.json.


2026-10-05T15:03:21.481291+00:00 Actual RED terminal failed expected3/3: Fa1 Vx0.85 vs0; Fa3 Vx2 vs0; full Fa3 sourceIntegerX502 vs500. Same job77c232993d844af493fc38a014a5c81b repolled after observation timeout; never restarted. Production minimal edit now written: Fa1 own HP null/nonpositive return after target resolution/validity; Fa3 HP branch only removes old drift. Outside those two functions exact before bytes preserved (production-delta.patch). No lifecycle/physics/frame/input/DAT changes; GREEN pending. Independent read-only review confirms test entrypoints and notes normal callback only closure counters. Task source chain correction: formal Genma901 opoint902/action40, then40->41->42->43->44->999->0; controlled902/0 is not direct birth.


2026-10-05T15:07:35.159555+00:00 RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS. GREEN4/4 PASS/0fail/0skip 43.6676229s at original Editor, native complete206 Driver declared fields26/26 equal, source500/V0/HP0/counter1 and unified view ratio preserved. Independent read-only final diff review no blocker. GeneratedGREEN0errors, finalEditor clean/nonPlay/idle/Console0errors; normal callback closure checks actual. Evidence artifacts/diagnostics/NTSD28-336B44-Q07-HITFA1-3-DEAD-MOTION-GATE-20261005/REPORT.md. Natural HP crosszero/Play/formal root EXE/full World/Host/GPU not proved, only triggered revisit; parent Q/goal open. Necessary ONE complete, queue REUSE51/TRIGGER14/ONE0 with65 open. Validator pending final.


2026-10-05T15:09:44.388271+00:00 VERIFIED file operation: final-scope-check.json proves protected4/authority5/content5 both ends raw same/backups9/native74 stable, production outsidetwo methods unchanged, test onlynew200lines/two methods; actual Validator exit0 and gitdiffcheck exit0. Operation has no deletion/move/Git discard. ProductionRecord remains RUNTIME_PENDING scoped4/4/26fields; Q/goal open.
