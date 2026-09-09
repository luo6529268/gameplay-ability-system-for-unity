# Task Contract — NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001

> 状态：`FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_15_OF_15 / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_2 / DIRECT_AND_STAGE_SELF_OWNER_ONLY`

## 目标

让 Unity direct combatant、BattleTest direct participant、ordinary stage row 与 results-reserve row 在
physical runtime slot 已明确后，将 Authority `Entity28+0x354 owner_slot` 初始化为自身 slot，并保证显式
owner 在实体第一次注册快照前可见。

## Authority 与原状

- 正式 `game_session.cpp::initialize()` 对每个 combatant 写
  `request.owner_slot = static_cast<int>(combatant.slot)` 后 `spawn_at(combatant.slot, request)`。
- 正式 story `spawn_native_story_row()` 在确定 `slot` 后写 `request.owner_slot = slot`，随后同 slot materialize。
- `battle_world.cpp::spawn_at()` 把 `request.owner_slot` 原样写到 entity state。
- Unity `AppManager`、`BattleTestBootstrap` 在 `LF2Character.ModuleBind()` 注册前未声明 required/self owner；
  `SimulationStageWaveModule` ordinary 与 results-reserve 在成功 materialize 后反而显式写 owner `-1`。
- stage factory task 已有 `requiredRuntimeSlot` 与 `ownerEntityIndex` seam，但 owner 当前未由 configurator写入，
  且两个 factory 只在注册完成后消费显式 owner。

## 修改范围

- `Assets/NTSD/Scripts/App/AppManager.cs`
- `Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs`
  - `ModuleBind/Register`后只接受实际slot与requested direct slot相等；失败即走既有统一回收/reset且不写roster。
- `Assets/NTSD/Scripts/Simulation/Runtime/BattleMatchConfigRuntimeAdapter.cs`
  - direct participant index `i` 在 `ModuleBind` 前同时声明 required slot 与 self owner，并只接受
    Authority定义的physical slot `0..19`。
- `Assets/NTSD/Scripts/Simulation/Stage/StageSpawnTaskConfigurator.cs`
  - 有效 required slot 同时成为 stage task explicit owner；默认 `-1` 保持未绑定。
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.Lifecycle.partial.cs`
  - 各对象在自己的 OPoint 初始化步骤中、首次注册前消费 explicit owner；默认 `-1` 不变。
- `Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs`
- `Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs`
  - 保留注册后 exact owner 校验写，不用类型特判替代对象初始化合同。
- `Assets/NTSD/Scripts/Simulation/Stage/SimulationStageWaveModule.cs`
  - ordinary 与 results-reserve 成功路径最终 owner 保持 required physical slot，不再覆盖为 `-1`。
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B0DirectEntitySelfOwnerProductionEditorTests.cs`
  - 覆盖 direct slot 0/9/10/19、stage slot 20/high、logic factory first snapshot、results reserve 与 reset/reuse。

## 不变量与排除

- 不修改 slot 容量、分配算法、战斗 pass、RNG、位置、Team、relation、damage、Scene、Prefab、Config、
  ProjectSettings、shutdown 或正式 Authority。
- 不处理 F8 owner `99`；不传播 ordinary OPoint parent owner；不改变 hit_Fa 8/9/13、state9996 clone `-1`、
  type3/kind hit writer 或 `+0x3F8/+0x2F8`。
- snapshot restore shell 继续从 snapshot state 恢复 owner，不强制 self。
- registration 失败对象仍由既有 rollback/reset 回到 `-1`；不得留下伪有效 owner。

## 验收

- direct participant index/slot `0,9,10,19`：首次 claimed active-slot entity runtime 与其 raw-trace
  projection 中 owner 等于自身 slot；独立 raw-slot backing 保持 reset sentinel `-1`，不得被活动实体镜像污染。
- ordinary stage 与 results-reserve `20` 及 high slot：owner 等于 required slot，factory/direct fallback一致。
- explicit non-self owner task 仍保留显式值，不被通用 self 规则覆盖；默认 task owner 仍为 `-1`。
- `-1`、`20`与更高direct slot在写入前被拒绝且不改变required/owner sentinel；pool reset/rejected
  registration 不保留旧 owner。
- runtime/editor compile 0 error；focused test实际通过；SelfCheck、Play、joint trace状态如实记录。

## 回滚

只移除上述 direct/stage pre-registration owner 写入、stage post-init self owner和本包测试；不得回滚
`NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001`或其他工作树改动。
