# Task Contract — NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001

> 状态：`FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / 7_OF_7_PASS / SELF_CHECK_BLOCKED_BY_UNRELATED_CPOINT / PLAY_PENDING / JOINT_TRACE_PENDING / RUNTIME_PENDING / B0_OWNER_PRODUCER_PREREQUISITE / EXISTING_STORAGE_REUSED / NO_SCHEMA_CHANGE`
> 来源：`NTSD28-B0-OWNER-SLOT-PRODUCTION-OWNER-AUDIT-001` route 1；复用并提前执行
> `NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001` 的 carrier 与已闭合 producer 范围

## 目标

解除 Unity generic non-character frame logic 对 `OwnerSlotIndex` 的 target multiplexing：把当前 Authority
`EntityState28+0x3F8 object_ai_target_slot_3f8` 绑定到已有 `PickerStableId` 底层 int，提供 canonical
`ObjectAiTargetSlot3F8` 入口，并让已恢复的 common/preassigned/child 路径只读写该 target carrier。

完成后 `OwnerSlotIndex` 只保留 +0x354 owner/credit 语义，为 direct self、F8 99 与 ordinary OPoint owner
producer 后继包解除前置冲突。本 Task 不宣称整个 object-AI 或 owner producer 已闭合。

## Authority 与 Unity 原状

- Authority `native_ai.cpp::NativeAi28::step_non_character_hit_fa(...)`：
  - common 1/2/3/12/14 以 +0x3F8 验证缓存、升序 strict-nearest 扫描并消费；
  - 4/7 跳过 common scan，直接消费预先写入的 +0x3F8；
  - 5/6 child 的 `owner_slot` 继承 source owner，+0x3F8 写 selected character physical slot；
  - scan miss 不先清除非 `-1` stale target；只有 target 仍为 `-1` 才写 HP=0；
  - scan candidate 为 state14 时，仅旧 cache 非 `-1` 才排除；非 state14 始终检查
    `abs(render_phase_008) <= 2`。
- Unity `NTSDEntityRuntime.PickerStableId` 已默认/reset/copy/ECS/checksum/parity 持久化，并被 specialized
  OID124 hit_Fa12 当 target 使用；不新增第二份 int，不改变现有 snapshot/checksum schema。
- Unity generic `LF2Entity` 当前在 common/4/7/11 中误读写 `OwnerEntityIndex`，5/6 又把 selected target
  写入 `OPointCreateTask.ownerEntityIndex`；两个 factory 未消费现有 `trackedTargetSlot`。

## 修改范围与所有权

- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
  - 新增 canonical `ObjectAiTargetSlot3F8` 属性，直接代理 `PickerStableId` storage；保留旧字段用于兼容。
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
  - 新增 entity canonical 属性；generic common/4/7/11 改读写 +0x3F8；
  - common scan 修正 stale miss 与 state14/render-phase gate；
  - 5/6 task 分别写 source owner 与 selected `trackedTargetSlot`。
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponFrameLogicResolver.cs`
  - specialized 4/12 改用 canonical 名称；12 scan 同步 stale miss 与 gate 顺序，不改变其他 motion 常量。
- `Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs`
- `Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs`
  - entity 已注册、runtime slot 已确定后，把显式 `trackedTargetSlot` 写到 child canonical +0x3F8；普通
    OPoint 的默认 `-1` 不继承 parent target。
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
  - 把既有 common/4/7/11 与 weapon dispatch 断言改为 owner/target 分离，并新增 5/6 child、stale 与 gate
    回归矩阵；保留 raw inactive-slot witness。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B0ObjectAiTarget3F8DeconflictionEditorTests.cs`
  - focused 覆盖同 storage alias、slot0/high/-1、task reset，以及 owner != target 的最小 production witness。

## 明确排除

- 不修改 Authority、Config、Scene、Prefab、resource、ProjectSettings、slot capacity、pass order、RNG、physics、
  hit/damage、relation、lifecycle 或 shutdown。
- 不在本包恢复 hit_Fa 8/9/13/24/25 的独立 spawn 算法；这些旧 Unity 路径即使也使用
  `ownerEntityIndex`，仍须各自 Authority closure，不凭字段相似性改写。
- 不接 +0x2F8 excluded-group carrier/consumer；现有 Spawner-based exclusion 的替换仍属于
  `NTSD28-B6-OBJECT-AI-EXCLUDED-GROUP-CONSUMER-001`。
- 不删除或重命名 `PickerStableId`、ECS `Links.PickerStableId`、parity `pickerIdx`，不升级任何持久化 schema。
- 不改变 hit_Fa4/7 已有 raw inactive-slot Unity adapter；正式 EXE joint trace 后再裁决该边界。

## 验收

- owner sentinel `7` 与 target slot `0`/high slot 可同时保持；common scan、4/7、11 均只改/读 target。
- common valid cache、invalid cache rescan、equal-distance lower slot、state14 与 render-phase gate正确。
- no candidate：初始 `-1` 才 HP=0；非 sentinel stale cache保持且不虚构 target motion。
- hit_Fa5 OID219 与 hit_Fa6 OID220 child：owner=source owner、target=selected character；普通 task target=-1。
- specialized OID124 仍与原 storage/checksum一致。
- runtime/editor compile 0 error；focused test与相关 SelfCheck实际执行；Play/joint trace未完成时最多
  `RUNTIME_PENDING`。

## 回滚

逐文件删除 canonical alias、恢复 generic/specialized caller 与 factory tracked-target consumption，并恢复本包
focused/self-check断言。不得回滚工作树中任何其他 Change 或用户修改。
