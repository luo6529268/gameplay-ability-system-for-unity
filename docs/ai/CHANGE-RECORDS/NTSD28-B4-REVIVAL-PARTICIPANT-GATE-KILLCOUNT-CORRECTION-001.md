# NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalParticipantGateCorrectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalPlacementPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan BattleWorld28::advance_reaction_timers_slot and advance_native_revivals plus SimulationTickDriver28 C07; EXE B1E13AE1, closure 39DDDA15; owner audit NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001.
evidence: Focused RED was 10/16 and isolated both legacy HitStun30 writes, C07 KillCount/team rejection, lives2 nextHP80 wrongly selecting queued, and primary slot19 wrongly freed. Production removed only those render-phase writes while retaining AttackingCounter reset, removed the C07 legacy gate, and made branch ownership lives-first with transient-only terminal free. Focused 16/16; combined new+C25+C07 regressions 34/34; targeted NTSD_Battle Play passed 20 logical cases; builds 0 errors (47/104 warnings), Console 0 and Scene unchanged. Full SelfCheck remains blocked earlier by unrelated CPoint mode0 victim-Vz. Queued controller/group/visual, normal floor/RNG and exit trace remain downstream.
-->

> 状态：`VERIFIED / RED_10_OF_16 / FOCUSED_16_OF_16 / RELATED_34_OF_34 / TARGETED_PLAY_20_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / QUEUED_ROUTE_NEXT`

RED=`10/16`后只删除两个legacy HitStun30写入、C07额外gate，并恢复lives-first四结果。focused16/16、
相关34/34、Play20 cases与builds通过，Scene/Console不变；SelfCheck仍由更早CPoint阻塞。下一route为queued
continuation，不把normal floor/RNG或exit parity冒充完成。
