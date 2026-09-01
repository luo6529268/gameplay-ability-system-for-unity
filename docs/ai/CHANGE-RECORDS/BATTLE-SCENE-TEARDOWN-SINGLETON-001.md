# BATTLE-SCENE-TEARDOWN-SINGLETON-001 — Scene close must not recreate battle singletons

<!-- CHANGE-RECORD
id: BATTLE-SCENE-TEARDOWN-SINGLETON-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/BattleRuntimeAllocationGate.cs
code-path: Assets/NTSD/Scripts/Test/Editor/PlayDomainReloadPoolLifecycleEditorTests.cs
authority: USER-REPORTED-UNITY-SCENE-CLOSE-LEAK-2026-08-31; Unity teardown lifecycle; existing non-creating MMSingleton.TryGetInstance contract
evidence: COMPILE_0 / FOCUSED_1_1_PASS / LIVE_PLAY_TEARDOWN_PASS / POST_STOP_SINGLETONS_0_0 / CLEANUP_WARNING_0
-->

> 创建日期：2026-08-31  
> 当前状态：`VERIFIED / COMPILE_0 / FOCUSED_TEST_PASS / LIVE_TEARDOWN_PASS`

## 1. 用户报告与已观察事实

- Unity 关闭 Scene 报告未清理对象：`LF2ObjectPointFactory_AutoCreated`、`LF2ObjectPool_AutoCreated`。
- `MMSingleton<T>.Instance` 在静态实例为空时会创建 `<Type>_AutoCreated`；`TryGetInstance()` 只返回现有实例，不创建。
- `SimulationTickDriver.OnSingletonDestroyed()` 调用 `EndBattleAllocationSeal()`，后者在 allocation gate 已 sealed 时进入 `BattleRuntimeAllocationGate.Unseal(...)`。
- `Unseal(...)` 当前用 `LF2ObjectPointFactory.Instance` 与 `LF2ObjectPool.Instance`。当 Scene teardown 的销毁顺序先销毁了原 factory/pool，这两次访问会在关闭 Scene 期间重新创建日志中同名对象。

## 2. 改动范围

1. 只把 `BattleRuntimeAllocationGate.Unseal(...)` 的 Unity singleton lookup 改成 `TryGetInstance()`；正常 prepare/seal 路径保持现有按需创建语义。
2. 增加聚焦 Editor test：模拟 gate sealed 且两个 singleton 已不存在，调用 teardown unseal 后断言没有新 scene GameObject、gate 正常转为 unsealed。
3. 不修改对象池容量规则、opoint 时序、战斗 tick、Scene、资源或 C++ 对齐行为。

## 3. 验收与回滚

- compile 0 error。
- `PlayDomainReloadPoolLifecycleEditorTests` 全部通过；新增用例明确覆盖两个 `_AutoCreated` 名称不出现。
- 相关 allocation gate tests 通过；能执行时做一次真实 Scene/Play teardown Console 复验。
- 回滚为恢复 `Unseal(...)` 的两次 lookup 并移除对应聚焦测试；不触碰其他用户/并行改动。

## 4. 实际实现

- `BattleRuntimeAllocationGate.Unseal(...)` 的 factory/pool 访问由创建型 `.Instance` 改为 `TryGetInstance()`；只有 teardown/unseal 不再查找或创建，`PrepareNonUnityCapacity(...)` 与 `Seal(...)` 保持原实现。
- `PlayDomainReloadPoolLifecycleEditorTests` 新增 `AllocationGateUnseal_DoesNotResolveOrAutoCreateFactoryOrPool`：通过反射模拟 sealed gate，把两个 singleton 静态引用置空，执行 unseal，断言引用仍为空、场景组件数量不增加、gate 正常转为 unsealed，并在失败时清理可能产生的 `_AutoCreated`。
- 未修改 `MMSingleton<T>` 公共行为，避免扩大到其他单例；未修改 Scene、对象池容量、opoint、战斗 tick 或逻辑结果。

## 5. 验证

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo /m:1 /v:minimal`：0 error、56 warnings；warnings 为工作区既有/并行代码与 Unity 依赖提示。
- Unity 强制 scripts refresh/compile 后，新用例 job `7e61f8efa78f4a8d9c08c81403840859`：1/1 PASS。
- 既有 overlay teardown job `7ebb5576640f4427900969aee9aae0f6`：1/1 PASS。
- 整个 `PlayDomainReloadPoolLifecycleEditorTests` 的先前 job `3af8996a2e814353825fc0e6581f9602` 有一个既有且无关的 `RestartPolicy_IsBoundedAndStateDriven(...expected 5)` 失败（actual 1）；新增 teardown 用例不在失败列表，不把该整组记录为通过。
- 真实 Play Mode：初始化完成后 `LF2ObjectPointFactory_AutoCreated=1`、`LF2ObjectPool_AutoCreated=1`；退出 Play Mode 后两者均为0，Console 对 `Some objects were not cleaned up when closing the scene` 的匹配数为0。
- `git diff --check`：目标代码/记录无 whitespace error，仅有现有 LF→CRLF 提示。
- `Tools/Validate-ChangeLedger.ps1` 已运行；本 Change ID 的两个脚本路径均被覆盖，但全局结果仍被无关 `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001.md` 缺少 `code-path` 元数据阻塞，未修改该并行记录。
- Scene 未保存或改写。
