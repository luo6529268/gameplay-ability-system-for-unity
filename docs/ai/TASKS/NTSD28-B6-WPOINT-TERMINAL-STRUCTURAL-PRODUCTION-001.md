# NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-PRODUCTION-001 — Task Contract

> Goal12 Part A / 2026-09-10 / PLANNED / TEST_FIRST；脚本修改前建立。

## Authority / 原状
用户明确授权；已接受Goal1-11。正式EXE B1E13AE1与playable closure39DDDA15不变。
Authority `BattleWorld28::settle_held_refill_objects` battle_world.cpp:7843起，refill/exhaustion在terminal之前；7967-7970 `weapon_action>=1000 -> despawn(child_slot) -> continue`，child action/facing/hold/pose/DVX/kind3/RNG全部在后。
Unity当前real/generic未实现terminal structural gate，real旧Frame.D null guard会挡住terminal/refill。
既有BattleEntityLinkLifecycleWriter原子清理已验收；本包只复用，不改变合同。

## 精确写入范围
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`：WeaponActResult瞬态TerminalDespawnRequested。
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs`：仅terminal绕过旧current-frame-null guard；refill耗尽优先后、写frame之前产生outcome；非terminal missing-action不动。
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs`：real outcome先返回，generic pre-pose terminal outcome；kind3已验收行为保持。
- `Assets/NTSD/Scripts/Simulation/Passes/Interaction/SimulationQueryAndLinkModule.cs`：唯一Free consumer；Free前捕获holder generation handle，Free后仅resolve仍有效handle刷新，不再读旧held。sink-only held pass occurrence及terminal事件供C09/C20/slot trace区分，不影响规则/持久化。
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleStructuralWriter.cs`：仅如既有Free seam无法承载时最小适配；预期无需修改。
- 新 `Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointTerminalStructuralProductionEditorTests.cs`及Unity生成meta：focused与同文件scoped Play probe。
- 本Task/同ID Record、CHANGE-LEDGER.md、STATE.md、对齐总表、Temp产物。

Part B为独立零文件改动只读甄别，结论只写最终报告；不写入本Record或其它文件。

## 不变量 / 停止条件
terminal只通过StructuralWriter.Free，无Destroy、碎片或WeaponBrokenSound。refill当次耗尽仍留实体、保留旧kick RNG，未耗尽先完成refill再free。terminal outcome必须截断Goal11 kind3 continuation；terminal本身0 RNG，不提前改frame/pose/motion。
不改BattleEntityLinkLifecycleWriter、全局RNG、Direction-B资源、Scene/InputActions、+2F8、missing-action、OnThrown/weaponHP、snapshot/recovery、33ms或十一阶段关闭。
任何既有测试（含kind3 92/fullSelfCheck）失败、free后访问已回池对象、耗尽优先遗漏、需要Destroy/改变cleanup合同、清单外diff或Scene SHA漂移，立即停止并报告，不自行扩包修复。

## test-first / 验收
新增focused先实际RED，再写production：real/generic、1000/>1000、current child frame缺失/存在、DVX+kind3并存、slot0/high、renderer-backed与logic-only；观察free前frame/facing/hold/pose/motion/weaponHP与RNG，free/unregister/generation release各1及无Destroy/碎片sound。
OID122/123 × 未耗尽/耗尽 × current frame缺失；第三方held/catch/plain/encoded0x2000、self-link holder alias、same-slot reuse新generation；同pass下一child继续、C09/C20 occurrence trace、actual full tick structural ordering。
先跑focused，再lifecycle/structural/held-refill/B6与Goal11 focused；任一旧断言失败立即停。fullSelfCheck、两套dotnet/Unity compile、validator、真实Play terminal witness、Console0与Scene SHA。
Scene基线：D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11；指定Editor gameplay-ability-system-for-unity@b1b02287 / 2022.3.62f3 / NTSD_Battle，不启动第二实例。

## Play / 生命周期 / 回滚
同文件probe使用current terminal witness（可选RockLee254）在真实World建立scoped holder/child，通过既有held producer、实际driver tick观察terminal Free与generation失效；精确注明输入/动作注入方式，不把writer调用当物理按键。
observer、临时renderer/pool/roster和scoped实体归fixture所有；清理所有owned binding，finally移除observer，普通有序ExitPlay。不引入runtime manager/queue，outcome不持久化；trace counters仅diagnostic，无独立停止/drain职责。
回滚需用户明确批准，仅反向本包五个允许production路径中的实际增量及新增fixture/治理，不影响Goal1-11或用户既有修改。
验收后VERIFIED仅terminal精确子集；报告含PartB甄别并停于GOAL13_USER_HOLD。

最终状态（2026-09-10）：VERIFIED_TERMINAL_SUBSET。事前PLANNED事实保留，完整验收以同ID Record最终部分为准；GOAL13_USER_HOLD。
