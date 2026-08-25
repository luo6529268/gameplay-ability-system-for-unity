# R5-LIFE-001A — extended slot/newborn cursor adapter certification

<!-- CHANGE-RECORD
id: R5-LIFE-001A
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:/QQFile/NTSD2.4/ntsd_release/Makefile:22,32;src/entity/collision.cpp:1271-1285;src/entity/game_tick.cpp:577-691,2188-2194;include/game_world.h:GameWorld::free_entity
evidence: SOURCE-CURSOR-VERIFIED / UNITY-MAPPING-VERIFIED / EXTENDED-JOINT-FIXTURE-PASS / FRESH-UNITY-COMPILE-AND-SELF-CHECK-PASS / STALE-ASSEMBLY-PASS-DISCARDED / CPP-TRACE-AND-PLAYMODE-PENDING
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 类型：battle / lifecycle / test / adapter-certification

## 1. 状态与范围

- 当前状态：`RUNTIME_PENDING`
- Work Package：R5-LIFE-01A
- 只改test-only fixture，不改production runtime。
- R5-LIFE-01B（pending/free/render logic-half）独立。

## 2. Authority / Unity差异

- C++：slot50 lowest-free，late cursor升序；低于cursor的newborn延后，高于cursor的newborn同pass。
- Unity：同一顺序已在Authority400与allocator分立fixture覆盖，Extended joint尚未覆盖。
- 本包是adapter认证缺口，不预判存在gameplay defect。

## 3. 计划改动

| 文件 | 符号 | 目标 |
|---|---|---|
| `BattleRuntimeSelfCheck.cs` | `CheckSimulationWorldLateMutation` / test-only mutation entity | MobileExtended与DesktopExtended-growth的>399 high/low cursor矩阵。 |

## 4. 不可回退边界

- MobileExtended/DesktopExtended容量与动态增长必须保留；
- slot/generation、pool、worker、0-GC、30Hz/FrameInputSet、CentralOnly均不改；
- 不修改用户场景、DAT或C++ authority。

## 5. 实际改动

- existing mutation test helper新增可选required child slot，仅用于隔离cursor；
- MobileExtended 1050与DesktopExtended initial512→growth均新增source700/child900 same-pass矩阵；
- 两profile均新增source700/child600 birth-pass 0、next-pass 1矩阵；
- production allocator/registry/pass/profile未修改；lowest-free继续由existing allocator/table tests证明。

## 6. 验收

| 项 | 结果 | 状态 |
|---|---|---|
| source/mapping | 已闭合 | PASS |
| focused/full self-check | fresh assembly上2026-08-22 17:15:48=`PASS` | PASS |
| Unity compile | UnityMCP force refresh；Tundra 23.19s，Assembly-CSharp 17:14:38，无error CS | PASS |
| ledger/scoped diff | final validator PASS（39 Records / 29 governed code files）；scoped diff PASS | PASS |
| C++ trace/PlayMode | 未取得 | BLOCKED/PENDING |

## 7. 风险与回滚

- explicit required slot只隔离cursor；不得用它替代lowest allocator证据。
- 17:10:31 request PASS发生在旧Assembly-CSharp上，已作废且不计入证据；17:15:48 fresh PASS才有效。
- 若fixture失败，先记录first difference，再另建runtime Change Record。
- 回滚只涉及本test fixture与关联文档。
