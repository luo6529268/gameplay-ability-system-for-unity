# NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001

<!-- CHANGE-RECORD
id: NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_RUNTIME_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponFrameLogicResolver.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B0ObjectAiTarget3F8DeconflictionEditorTests.cs
authority: NTSD 2.8-Logan native_ai.cpp NativeAi28::step_non_character_hit_fa common 1/2/3/12/14, preassigned 4/7, no-target 11 and child 5/6; EntityState28 owner_slot +0x354 and object_ai_target_slot_3f8 +0x3F8; Unity existing PickerStableId storage and trackedTargetSlot seam; EXE B1E13AE1, closure 39DDDA15.
evidence: implementation reuses PickerStableId storage through ObjectAiTargetSlot3F8, separates common/4/7/11 and 5/6 target flow from OwnerSlotIndex, preserves declared exclusions, entered Unity assembly reload with no C# compile errors, and exact v2 focused execution passed 7/7 after correcting the invalid NUnit category. Current SelfCheck reaches the pre-existing unrelated CPoint throw-Vz assertion before this package's checks. Play and joint trace remain pending.
-->

> 状态：`FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / 7_OF_7_PASS / SELF_CHECK_BLOCKED_BY_UNRELATED_CPOINT / PLAY_PENDING / JOINT_TRACE_PENDING / RUNTIME_PENDING / B0_OWNER_PRODUCER_PREREQUISITE / EXISTING_STORAGE_REUSED / NO_SCHEMA_CHANGE`

本 Change 原子完成已恢复 object-AI target 路径与 +0x354 owner 的解耦。完整范围、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B0-OBJECT-AI-TARGET-3F8-DECONFLICTION-001.md`。

## 实际修改与验证

- `NTSDEntityRuntime` 新增 `ObjectAiTargetSlot3F8` canonical property，直接代理已有
  `PickerStableId`；copy/reset/ECS/checksum/parity storage 与 schema 均未改变。
- `LF2Entity` 的 Authority 已闭合 common/4/7/11 路径改为只读写 target carrier；common scan
  保留非 `-1` stale target，并按 state14 / render-phase 顺序筛选；5/6 child task 分别写 source
  `OwnerEntityIndex` 与 selected `trackedTargetSlot`。8/9/13 保持原状。
- `LF2WeaponFrameLogicResolver` 的 specialized 4/12 使用 canonical target 名称；12 的 strict-nearest
  上限、stale miss 与 state14/render-phase gate 与 generic 路径一致。
- `LF2ObjectPointFactory`、`BattleLogicEntityFactory` 在 entity 注册成功、slot 已确定后消费显式
  `trackedTargetSlot`；普通 task 默认 `-1` 不产生 target 传播。
- 既有 SelfCheck 的 common/4/7/11/weapon 断言已迁移为 owner/target 分离，并加入 near-full hit_Fa5
  child owner/target witness；新增 focused Editor test 覆盖 alias copy/reset、task reset、owner=7/target=0、
  stale target 与 state14/render-phase gate。因当前工具上下文没有 Unity MCP 且 computer-use 输入未获
  Unity 授权，同一测试文件内新增最小 `InitializeOnLoad` request runner；它只运行该 focused class，
  以 `Temp/NTSD28-B0-ObjectAiTarget3F8.request/result` 交换结果，不改生产逻辑。
- 2026-09-08 新鲜进程外编译：
  - `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal`：`0 error`（既有依赖版本 warning）。
  - `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:quiet -clp:ErrorsOnly`：临时只在生成
    csproj 中加入本包新 focused test source 后 `0 error / 104 warnings`；验证后已移除临时 csproj 行，
    未把生成工程作为项目改动保留。
- 2026-09-08 Unity Editor 刷新证据：`Assembly-CSharp.dll` 与 `Assembly-CSharp-Editor.dll` 均更新为
  `20:13:04`，新 focused test 已进入生成 csproj；Editor log 记录该 GUID 导入、script compilation 完成
  后 assembly reload，附近无 `error CS`。因此本包 production/runtime 与当时的 focused test body 已取得
  Unity compile 证据。
- 2026-09-08 当前程序集 SelfCheck 实际执行：request 于 `20:14:26` 被消费并写出 `FAIL`；首个失败为
  既有 `CheckCpointThrowRawAndTransformMatrix` 的 `character raw throw mode=0: victim Vz must reset to zero`
  （当前 source line 10938/Expect line 32810），发生在本包 object-AI 检查之前，不能算本包断言失败，
  也不能算 SelfCheck pass。
- 新增 request runner 后再次执行 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:quiet
  -clp:ErrorsOnly`：`0 error / 104 warnings`；Change Ledger validator 再次 PASS。Editor 自动刷新仍关闭，
  runner source 时间晚于 `20:13:04` assembly，因此必须再做一次 AssetDatabase refresh 后才能触发 focused。
- 2026-09-08 `20:31:07`，第一版 focused runner 已实际运行精确测试 class，结果
  `state=Failed(Child) / passed=0 / failed=7 / skipped=0 / inconclusive=0`。由于三个不创建
  `SimulationWorld` 的 alias case 也全部 error，这不是可归因到单一 production assertion 的结果；第一版
  runner 只写 suite 汇总，child exception 尚未知。
- 已为同文件 runner 增加 leaf failure message/stack 输出，并把新入口版本化为
  `Temp/NTSD28-B0-ObjectAiTarget3F8-details.request/result`，防止旧 assembly 在 refresh 前抢先消费。
  详情版再次进程外 Editor compile：`0 error / 104 warnings`；versioned request 已预置，待 Unity refresh。
- 2026-09-08 `20:42:19` 详情版执行确认七个 child 均为 `Failed:Invalid(Parent)`，共同异常为
  `OneTimeSetUp: Category name must not contain ',', '!', '+' or '-'`。根因是本包测试类的
  `[Category("NTSD28-B0")]`，没有任何 production assertion 实际运行。
- 已把非法 category 改为 `NTSD28_B0`，并将修复后入口再次版本化为
  `Temp/NTSD28-B0-ObjectAiTarget3F8-v2.request/result`，避免旧详情版 runner 抢跑；v2 Editor
  进程外编译为 `0 error / 104 warnings`。
- 2026-09-08 `20:47:52` Unity Editor assembly 已晚于修复脚本，v2 request 被消费并于 `20:48:05`
  写出：`state=Passed / passed=7 / failed=0 / skipped=0 / inconclusive=0`。alias、task reset、
  owner=7/target=0、stale target 与 state14/render-phase gate 均为实际 Unity EditMode focused pass。
- 未通过/未完成：完整 SelfCheck、Play、正式
  EXE/Unity joint trace。状态
  不得高于 `FOCUSED_TEST_PASS / RUNTIME_PENDING`。
