<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-HITFA3-INTEGER-PRECISION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs
diagnostic-source-path: artifacts/diagnostics/NTSD28-336B44-Q07-HITFA3-INTEGER-PRECISION-20261005/native_hitfa3_driver_witness.cpp
authority: formal 336B44 behavior3 double acceleration and four native ticks before wait3 next999
evidence: artifacts/diagnostics/NTSD28-336B44-Q07-HITFA3-INTEGER-PRECISION-20261005/REPORT.md
-->

# behavior3整数边界

脚本前PLANNED，准确scope/前置/风险/副作用/验收/恢复见[Task](../TASKS/NTSD28-336B44-Q07-HITFA3-INTEGER-PRECISION-001.md)。当前只是静态候选；先native实际完整core四tick与原Unity三项RED，不成立则不改生产。正式无生产帧的6/8/9/11/13保持条件门，旧限定证据复用。

Test-first 2026-10-05T14:37:13.994714+00:00: added exactly two declared test methods (3 cases) and new native full-Core diagnostic. Production RunHitFa3 unchanged; no RED/GREEN result yet. Tests use CreateNew JSONL and four natural frame ticks.

RED evidence 2026-10-05T14:40:44.344850+00:00: original Editor job8cdfad1d55d247afb27a418bc262ae3a terminal failed, 3 cases completed, all expected (±Z exact double and integer507/506). Earlier observation timeout retained, same job re-polled, not restarted. Native v2 full Core step4 exit0 frame54/54/54/0; first precise difference tick1, integer tick4. Proceed only four declared common literals and contract comment. Diagnostic wrong hp field build failure preserved/corrected current_hp.

2026-10-05T14:40:44.793780+00:00 CODE_WRITTEN: common RunHitFa3 only four float-to-double literals and one contract comment; actual two-file deltas recorded written-diff.json, outside production function bytes/text retained. No DAT or unrelated behavior change. GREEN pending.

2026-10-05T14:41:52.727585+00:00 generated RED 0errors/301warnings/12.57s; generated GREEN 0errors/334warnings/13.72s. Original MCP green refresh requested once; original focused GREEN not yet run.

2026-10-05T14:45:28.593184+00:00 RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS: original GREEN4/4,0 failed/skip43.9026606s; native complete Core4tick and subject65/65 strict equal, view residual <=1.14e-13px. Normal callback zero postconditions passed; original Battle clean/nonPlay/Console0errors. Shared constants restored, no DAT/nonbattle edits. Independent two-stage read-only review no blockers; GREEN terminal verified by root after reviewer response. Natural birth/706/full world/host/formalEXE/physical/GPU unverified; do not schedule matrix. REUSE50/TRIGGER14/P0=DEP=ONE=0,225 records/64open. Report/evidence and failure corrections retained.

2026-10-05T14:47:28.840741+00:00 Final governance: Validate-ChangeLedger exit0/1278 records/2 governed diff files covered; git diff --check exit0. scope v2 confirms protected4/DAT3 both sides/authority5/backups9/native inputs69 unchanged. Routing parser correction retained in new files;64 IDs REUSE50/TRIGGER14. No script changed after validator. Scope audit and runtime evidence limits remain as REPORT; parent goal active.
