# Task Contract — CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001

> 状态：`FOCUSED_TEST_PASS / SLOT_LIFECYCLE_SEAM_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / S0_NOT_VERIFIED`
> 创建：2026-08-30

## 1. 目标

在不移动生产源码的前提下，将 `RuntimeSlotTable` 中平台无关的 slot allocation、本地 Generation lease、provisional claim、required side-effect、commit/rollback/release 与 canonical per-slot `allocationEpoch` 拆成 BCL-only seam；`RuntimeSlotTable` 继续持有 `LF2Entity`、raw runtime、paging 和 Client adapter 职责。

## 2. Authority

- 用户于 2026-08-30 精确授权 `CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001`。
- Server `S0-CUT-C-SLOT-LIFECYCLE-IDENTITY-001` 与 48-line golden journal。
- C++ release live Authority400、`0/20/50` ascending first-free、free/reuse 与 successful reset-cooldowns order。
- 已关闭 StageSpawn rest correction：成功清 rest，冲突 lease fail closed。

## 3. 允许修改范围

- 新增 `Assets/NTSD/Scripts/Simulation/RuntimeSlotLifecycleState.cs` 与 `.meta`。
- 修改 `RuntimeSlotTable.cs`、`SimulationWorld.Registry.partial.cs`、`BattleParitySnapshot.cs`。
- 修改 `SimulationQueryAndLinkModule.cs` 的rest operation seam：无`ItrRest` tracker的诊断/实体类型仍清C++全局slot rest但不绑定tracker；有tracker继续acquire-first原子绑定。
- 新增 `RuntimeSlotLifecycleSeamEditorTests.cs` 与 `.meta`；必要更新 `BattleParityStructuralWitnessEditorTests.cs` 与 `BattleRuntimeSelfCheck.cs`。
- Server package 只允许增加 .NET test-only linked-source consumer；不得移动 production source。
- 更新双仓库 Task/Change/Ledger/State/Handoff/Queue/matrix/stage progress。

`RuntimeSlotAllocator.cs` 与 `RuntimeEntityHandle.cs` 是允许范围内的 BCL dependency，但只有聚焦失败证明必要时才修改；预定实现不改它们。

## 4. 必须保持的合同

- 400 slots 与 `0/20/50` bands 不变。
- Generation 继续只做本地 stale-handle safety；claim/rollback/release可推进，不进入formal witness。
- allocationEpoch仅在required side-effect和全部注册检查成功后的commit递增一次；失败claim、side-effect、rollback、release、peek、lookup均不递增。
- fresh world reset清allocation epochs；本包不扩张snapshot/recovery schema。
- 成功 structural allocate event使用canonical epoch；buffer不再自行计数。
- rest store保持Client-owned，Cut C只调用显式side-effect operation seam。

## 5. 验证

先记录失败测试，再实现；随后运行Unity编译、seam/allocator/table/structural/StageSpawn/snapshot/same-tick focused tests、BattleRuntimeSelfCheck、S0 8/8、lockstep 9/9，以及Debug/Release .NET seam consumer。最后运行双Ledger与Server workflow/matrix检查。

## 6. 禁止

不移动Cut C production source，不改package manifest/lock/asmdef/version，不改battle rules、30 Hz、Scene、资源、Input Actions、TargetTick/InputDelayFrames、transport、Socket、数据库、公网、snapshot/recovery schema、S1 wire、formal AI、formal marker或default stage.dat。

## 7. 回滚

只回退本包新增seam、RuntimeSlotTable/registry/witness adapter、focused tests、.NET test-only links和治理记录；保留既有RNG/FrameInput/StageSpawn及用户其他修改。

## 8. 结果

- `RuntimeSlotLifecycleState` 已成为Client-owned、BCL-only的provisional claim、required side-effect、commit、rollback/release、本地Generation和canonical per-slot `allocationEpoch` owner；`RuntimeSlotTable`仍只做Unity entity/raw/page adapter。
- Registry只在rest操作和其余注册检查成功后commit；失败side effect/后续拒绝均rollback，不形成成功epoch或structural allocate event。Structural buffer不再维护第二套counter。
- Unity最终编译无C# error；seam `5/5`、StageSpawn/rest `2/2`、structural `4/4`、snapshot `3/3 + 9/9`、same-tick `8/8`、pending-destroy `7/7`、S0 `8/8`、lockstep `9/9`以及fresh `BattleRuntimeSelfCheck`全部通过；warmed 1,024次循环为`0 B`。
- .NET test-only seam consumer的Debug/Release均通过；Server solution Debug/Release `0 warnings / 0 errors`，四组Server suites双配置及Release no-network Host均通过。
- 未移动production source，未改package manifest/lock/asmdef/version、snapshot/recovery schema、formal AI或marker。S0/S5仍NOT_VERIFIED；后续shared-owner move必须取得新的具名授权。
