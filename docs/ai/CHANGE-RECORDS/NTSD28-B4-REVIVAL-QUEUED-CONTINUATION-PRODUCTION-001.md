# NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalQueuedContinuationProductionEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::advance_native_revivals queued branch and SimulationTickDriver28 continuation OID998 tail; battle_flow_tests locked fallback/visual witnesses; EXE B1E13AE1, closure 39DDDA15.
evidence: Focused RED was 4/11. Production added +0x184/+0x180 runtime carriers with copy/reset and changed only queued continuation: explicit controller must be active before mutation, -1 projects physical slot one and inactive backing group zero, controller group replaces fixed one, and explicit/default visual writes +0x318/+0x180 before action219/counter0/hold10 and OID998 call. Focused 13/13, combined queued+gate+C07+C25+snapshot 47/47, targeted NTSD_Battle Play 13 cases, builds 0 errors (47/104 warnings), Console 0 and Scene unchanged. Full SelfCheck remains blocked earlier by unrelated CPoint mode0 victim-Vz. Normal floor/RNG, B7/H queued-field producers and public schema remain downstream.
-->

> 状态：`VERIFIED / RED_4_OF_11 / FOCUSED_13_OF_13 / RELATED_47_OF_47 / TARGETED_PLAY_13_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / NORMAL_ROUTE_NEXT / PRODUCER_B7_H_PENDING / SCHEMA_DEFERRED`

RED4/11后已补两个visual carrier及精确controller/defer/group/visual事务；focused13/13、相关47/47、
Play13、builds/Console/Scene均通过。SelfCheck仍由更早CPoint阻塞；normal、B7/H producer和schema保持后继。
