# CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001 — StageSpawn Reused-Slot Rest Alignment

<!-- CHANGE-RECORD
id: CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.StageWave.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Simulation/RuntimeRestStore.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ItrRestTracker.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/StageSpawnRestAlignmentEditorTests.cs
authority: User exact authorization dated 2026-08-30; C++ release live spawn_stage_immediate_entry_slot -> spawn_at -> spawn_into_slot -> reset_cooldowns.
evidence: USER_AUTHORIZED / FOCUSED_TEST_PASS / STAGE_SPAWN_REST_ALIGNMENT_READY / GOVERNANCE_CLOSED / S0_NOT_VERIFIED
-->

> 创建日期：2026-08-30
> 最后更新：2026-08-30
> 类型：battle / simulation / test

## 1. 状态与范围

- 当前状态：`FOCUSED_TEST_PASS / STAGE_SPAWN_REST_ALIGNMENT_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / S0_NOT_VERIFIED`
- 所属 Work Package：`CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001`
- 不属于本次范围：Cut C source move、formal marker、battle rules、tick/input/network/recovery/AI/Scene/resource/default stage deployment。
- 关联审计：Server `GOVERNANCE-S0-STAGE-SPAWN-REST-ALIGNMENT-PREREQUISITE-001`。

## 2. Authority / 需求依据

- C++ release normal-build chain：`spawn_stage_immediate_entry_slot -> spawn_at -> spawn_into_slot -> reset_cooldowns(slot)`。
- `reset_cooldowns` 清 `s_arest[slot]`、完整 VRest victim row 与 attacker column；该成功路径是 battle authority。
- 用户额外要求保留 Unity-native conflicting rest lease fail-closed：不失效 lease、不泄漏 pool、不产生成功 allocationEpoch。
- Evidence 等级：C++ clear-on-success 为 `VERIFIED` static live-path authority；冲突 lease 边界为用户明确合同。

## 3. Unity 原状与已确认差异

- `SimulationWorld.RegisterCoreFromStructuralWriter` 对 `StageSpawnAt` 跳过 `ResetCooldownsForRuntimeSlot`。
- `RestoreStageSpawnRestState` 只在实体创建后绑定 rest store，因此复用槽 ARest/VRest 被保留。
- `RuntimeRestStore.ResetSlot` 会先使已有 binding 失效；直接在 StageSpawn 路径调用会破坏用户要求的冲突 lease fail-closed。
- 现有 SelfCheck 成功夹具仍断言 preserve，属于已废止 C# authority；同一夹具已有冲突 lease/pool rollback 检查。
- `BattleRuntimeSelfCheck.cs` 已有 FrameInput seam 的无关 diff；本包只能修改 StageSpawn/rest fixture，必须保留该差异。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `RuntimeRestStore.cs` | binding/reset transaction | `ResetSlot` 会失效既有 binding | 新增先取得 lease、再只清 values 的原子入口；冲突时零 mutation |
| `LF2ItrRestTracker.cs` | tracker binding | 只能普通 bind/reset | 必要时接管新取得的有效 handle，并清本地 fallback state |
| `SimulationQueryAndLinkModule.cs` | `ResetCooldownsForRuntimeSlot` | reset 后重新 bind，冲突安全性不足 | 使用原子 reset-and-bind；无 tracker 时保留 raw reset |
| `SimulationWorld.Registry.partial.cs` | structural registration | StageSpawnAt 跳过 cooldown reset | 所有成功注册在提交前完成相同 rest reset；失败走既有 rollback |
| `SimulationWorld.StageWave.partial.cs` | StageSpawn post-create check | 创建后再次尝试 bind | 只验证 registration 已建立预期 binding，保持 fail closed |
| `StageSpawnRestAlignmentEditorTests.cs` | focused tests | 不存在独立聚焦入口 | 覆盖 rest transaction 成功清理与冲突零 mutation |
| `BattleRuntimeSelfCheck.cs` | StageSpawn fixture | 成功复用断言 preserve | 改为 C++ clear；保留 lease/pool/allocation rollback 断言 |

## 5. 不可回退边界

- Authority400、runtime slot first-free、Unity local Generation/lease token 与 formal `(slot, allocationEpoch)` 分工不变。
- 30 Hz、FrameInput、RNG、worker、对象池所有权与既有 closed Change IDs 不变。
- 冲突 lease 不能被 invalidate、重建或静默覆盖。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `RuntimeRestStore.cs` | `TryResetSlotAndAcquireBinding` / `ResetSlotValues` | 先取得空闲lease，再清ARest、victim row、attacker column；既有`ResetSlot`复用纯value清理 | 冲突lease时零mutation；普通`ResetSlot`语义不变 |
| `LF2ItrRestTracker.cs` | `TryResetAndBind` | 只在未持有active binding时接管transaction handle，并清本地fallback | StageSpawn成功后tracker直接拥有清零store state |
| `SimulationQueryAndLinkModule.cs` | `TryResetAndBindStageSpawnCooldowns` | 新增StageSpawn专用原子入口；普通`ResetCooldownsForRuntimeSlot`未改 | 不把冲突lease语义扩张到普通registration/pass |
| `SimulationWorld.Registry.partial.cs` | `RegisterCoreFromStructuralWriter` / `RestoreStageSpawnRestState` | StageSpawn registration在allocate event/OnAdded前完成transaction；post-create入口只验证binding | 失败走既有slot/raw-writer rollback且不发布成功allocationEpoch |
| `StageSpawnRestAlignmentEditorTests.cs` | 2 focused tests | 覆盖成功三向清理及冲突lease/data零mutation | 提供独立StageSpawn rest transaction门槛 |
| `BattleRuntimeSelfCheck.cs` | binding owner scan、production lifecycle、Audit7 StageSpawn fixture | 成功断言由preserve改为C++ clear；冲突断言增加rest/lease/pool/allocation event | 覆盖真实StageSpawn factory/direct链与失败清理 |

`SimulationWorld.StageWave.partial.cs`经复核无需修改：成功路径保留post-create binding验证；factory在registration失败时已有`ReleaseRejectedSpawn`，direct fallback没有pool owner，现有冲突fixture负责验证无泄漏。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 事前治理 | 双仓库 Task/Record/Ledger/Queue/matrix | 事前创建并激活；实现后回写最终证据 | `PASS` |
| focused red | 新test调用尚不存在的transaction API；随后尝试离线generated csproj | test-first source先写；离线Editor csproj因既有FrameInput shared-owner迁移产生40个陈旧引用错误且未包含新test，不能作为本包有效red | `EVIDENCE_INSUFFICIENT` |
| 编译 | 当前唯一 Unity Editor；AssetDatabase refresh 后读取 Console | 新测试已导入，`Assembly-CSharp-Editor.dll` 于 14:06:44 更新；Console `error CS` 0 条 | `PASS` |
| focused test | MCP EditMode category `StageSpawnRestAlignment`，job `c80716bd59df4ff893b616e6eeba9854` | `2/2 passed / 0 failed / 0 skipped` | `PASS` |
| self-check | 菜单 `NTSD/验证/运行战斗运行时自检`；`Temp/NTSD_BattleRuntimeSelfCheck.result` | MCP 菜单等待 30 秒超时，但 Unity 继续完成并于 14:13:29 写入 fresh `PASS` | `PASS` |
| 回归 | `NTSD.Test.InProcessLockstepAuthoritySessionEditorTests`，job `27bd8f4a426947ffad209aa14114450c` | `8/8 passed / 0 failed / 0 skipped` | `PASS` |
| 回归 | `NTSD.Test.BattleLockstepSessionEditorTests`，job `0d081e9b93994c1288e60842232cfe5f` | `9/9 passed / 0 failed / 0 skipped` | `PASS` |
| C++ authority 对照 | release live call chain与实现后静态复核 | 成功路径三向清理；冲突路径在任何rest mutation前拒绝；allocation event在transaction后发布 | `PASS` |
| 静态/治理 | Client Ledger、Server workflow/Ledger、matrix exact-set、scoped diff与范围审计 | Client `112 records / 21 governed files`；Server workflow `43 rows / ACTIVE 0 / READY 0 / GATED 4 / DEFERRED 6`；Server Ledger `57 / 86`；matrix `57/57`；diff check通过 | `PASS` |

## 8. 风险、回滚与未关闭项

- 风险：若先清 values 后发现 binding 冲突，会破坏现有 lease；实现必须先成功 acquire 再清理。
- 风险：StageSpawn post-create 二次 bind 不能成为独立副作用点。
- 未关闭项：本包没有真实战斗 Scene / Play Mode 人工表现验收；该层未获本包要求，也不能据此把完整S0/S5或formal marker写成完成。
- 回滚：只撤销本记录的原子 transaction、StageSpawn 接线和测试；不触碰无关工作树。

## 9. Git / 交接

- 修改前工作树：已有多项受治理 Client/Server 修改与未跟踪文件；全部视为用户/既有工作并保留。
- 实际 diff 范围：四个runtime文件、一个新focused test及`.meta`、`BattleRuntimeSelfCheck.cs`的StageSpawn/rest段；StageWave未改。
- 提交 hash：未请求提交。
- `Tools/Validate-ChangeLedger.ps1`：事前与最终校验均通过；最终计数记录在本包交付与双仓治理文档。
- 交接优先读取：本 Task/Record、Server Queue、StageSpawn prerequisite audit、S0 dossier。
