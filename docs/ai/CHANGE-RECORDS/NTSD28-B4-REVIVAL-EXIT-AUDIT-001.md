# NTSD28-B4-REVIVAL-EXIT-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B4-REVIVAL-EXIT-AUDIT-001
status: VERIFIED
change-kind: DIAGNOSTIC_TOOL_AND_EDITOR_TEST
code-path: Tools/NTSD28AuthorityTrace/revival_exit_capture_main.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalExitAuditEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28 step_frames C25 revival arm and advance_native_revivals C07 lives-first queued/terminal/normal branches, direct participant defaults and slot lifecycle; EXE B1E13AE1, closure 39DDDA15.
evidence: Authority and Unity revival traces are each byte-stable across two runs and compare equal across 13 records/416 field occurrences with an empty first difference. Related Unity tests pass 54/54, both builds have 0 errors, and targeted NTSD_Battle Play passes with Console 0 and unchanged Scene SHA/dirty/root. Full SelfCheck was run but remains blocked earlier by the unrelated existing CPoint mode0 victim-Vz assertion. Production behavior did not change; B4 revival exit is ready while B7/H queued producers and public schema remain pending.
-->

> 状态：`VERIFIED / REVIVAL_TRACE_EQUAL_13_RECORDS_416_FIELDS / DOUBLE_RUN_BYTE_STABLE / RELATED_54_OF_54 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_REVIVAL_EXIT_READY / PRODUCTION_UNCHANGED / B7_H_PRODUCER_PENDING`

完整范围、Authority、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B4-REVIVAL-EXIT-AUDIT-001.md`。

## 当前实施

- 已恢复当前 Authority identity 与 B4 四个前置包的 `VERIFIED` 状态。
- 已冻结本包只新增专项 Authority runner 与 Unity focused/Play trace test；不改生产runtime、content、Scene或
  Authority目录。
- 已实现13条规范化record与逐字段comparison；diagnostic-only边界与生产不变保持。

## 最终证据

- runner/binary SHA：`AAD627BC...47E8` / `38D6C949...21C`；Authority capture两次SHA均
  `9240BCD7...C298`，Unity capture两次SHA均`58C9A740...3C8`。
- comparison为`equal-revival-trace`，13 records / 416 fields，first difference空；direct、C25、queued、
  terminal、normal、peer与slot reuse全部进入同一严格顺序schema。
- Unity focused双跑`1/1`、联合回归`54/54`、两套build 0 error；目标Play 13/416通过、Console0、Scene
  SHA/dirty/root不变。
- full SelfCheck仍提前失败于既有CPoint mode0 victim-Vz断言；B7/H queued producer与公开schema仍后置。
- production runtime/content/Scene/Authority目录均未改。
- Change Ledger validator：`PASS / 409 records / 341 governed code files`。
