# NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001

<!-- CHANGE-RECORD
id: NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B3LegacyLifecycleChildRetirementEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::resolve_pending_lifecycles_range 1100..1299 self-only reset; Direction-B Itachi action167 next1250; Unity BattleLateEntityLifecycleModule KillCount child scan; EXE B1E13AE1, closure 39DDDA15.
evidence: Final RED was 0/4: unmatched child's own late update produced HitStun40 while KillCount-matched child was overwritten to 0/-99/-149/-198 for codes 1100/1200/1250/1299. Production removed only the owner-slot world traversal and child write. Self 1100-code/frame0 is preserved. Focused 4/4 and targeted NTSD_Battle Play pass, including current Itachi next1250 value and eight absent child-owner writes; builds 0 error, Console 0, Scene unchanged. Full SelfCheck remains blocked earlier by unrelated CPoint mode0 victim-Vz assertion.
-->

> 状态：`VERIFIED / RED_0_OF_4 / PRODUCTION_CHILD_SCAN_REMOVED / FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELF_RESET_PRESERVED / CURRENT_ITACHI_1250_COVERED / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`

RED 0/4精确观察matched child被写成0/-99/-149/-198而自身正常结果为40。production仅删除world/KillCount
child traversal；self encoded reset保持。focused4/4、targeted Play、builds0 error、Console0和Scene不变；full
SelfCheck仍被更早独立CPoint阻塞。current Itachi next1250已覆盖；下一严格route为B4 revival participant gate。
