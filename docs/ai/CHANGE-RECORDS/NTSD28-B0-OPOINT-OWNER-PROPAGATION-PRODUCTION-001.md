# NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_RUNTIME_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicObjectPointRuntime.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B0OpointOwnerPropagationProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan playable ObjectSpawnPlanner28::plan_frame intent.owner_slot=parent.owner_slot and BattleWorld28::spawn_from_opoint_intents request.owner_slot=intent.owner_slot; EXE B1E13AE1, closure 39DDDA15.
evidence: pre-change audit proves both world-owned Unity late OPoint materializers omitted task.ownerEntityIndex; route1 target deconfliction, route2 pre-registration initializers and route3 F8 owner99 focused gates passed. Test fixture corrections isolated no-op parent frame tick, non-destruction and second-hop structural segment from unrelated lifecycle gates. Corrected pre-production v1 at 2026-09-08 23:59:37 local produced exact RED 2 pass/5 fail: sentinel and built-in OID999 owner -1 passed; self, nonself, kind2 holder separation, multi and two-hop all failed expected literal owner versus actual -1. Logic and presentation ProcessOneLateOpoint producers now each write only task.ownerEntityIndex=spawner.OwnerEntityIndex. Runtime and Editor builds passed with 0 errors/47 and 0 errors/104 warnings; Unity runtime assembly refreshed 2026-09-09 00:09:30 and v1 passed 7/7 at 00:09:55. Full SelfCheck at 00:11:22 stopped at the earlier unrelated CPoint throw-Vz assertion before the added presentation-owner matrix. Play and joint trace remain pending.
-->

> 状态：`FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_7_OF_7 / SELFCHECK_BLOCKED_BEFORE_PRESENTATION_ASSERT_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_4 / LOGIC_AND_PRESENTATION_PRODUCERS_WRITTEN / FRAGMENT_AND_LEGACY_BRANCHES_EXCLUDED`

完整范围、Authority、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B0-OPOINT-OWNER-PROPAGATION-PRODUCTION-001.md`。

## 实施与验证

- full asset refresh将新focused源纳入Unity程序集；测试夹具先后排除了真实`LF2OtherObject` frame tick、
  non-character cleanup和第二轮character lifecycle gate，最终保持第一跳full late pass、第二跳exact structural
  segment，不改变production。
- corrected pre-production v1于2026-09-08 23:59:37本地实际
  `2 passed / 5 failed / 0 skipped / 0 inconclusive`：sentinel与built-in OID999 fragment owner`-1`通过；
  self/nonself、kind2 holder分离、multi与two-hop全部精确失败为expected literal owner/actual`-1`。
- 后续完整调用链复核确认同一`BattleStructuralWriter.ProcessLateOpointSegment()`按world mode解析为logic-only
  `BattleLogicObjectPointRuntime`或presentation `LF2ObjectPointFactory`；后者也有同构
  `ProcessOneLateOpoint()` task producer且同样缺owner。它已在修改前补入精确code-path，不授权修改通用factory。
- logic-only `BattleLogicObjectPointRuntime.ProcessOneLateOpoint()`与presentation
  `LF2ObjectPointFactory.ProcessOneLateOpoint()`现均只在ordinary task构造中写
  `ownerEntityIndex=spawner.OwnerEntityIndex`；通用create/post-init/factory inference均未改。
- `RunAudit7LateOpointPrecisionCase()`为presentation镜像补parent literal owner与claimed active-slot/raw backing
  断言；既有state9996 owner`-1`断言保留。
- 进程外编译：runtime `0 errors / 47 warnings`；Editor `0 errors / 104 warnings`。
- Unity runtime程序集于2026-09-09 00:09:30刷新；v1于00:09:55实际
  `7 passed / 0 failed / 0 skipped / 0 inconclusive`。
- full `BattleRuntimeSelfCheck`于00:11:22实际运行，但在调用序列更早的既有CPoint throw-Vz断言停止，
  尚未到达新增presentation owner断言；Play和joint trace未运行，保持`RUNTIME_PENDING`。
