# NTSD28-B3-C25P-HEALING-OWNER-001 — C25p per-slot healing owner

<!-- CHANGE-RECORD
id: NTSD28-B3-C25P-HEALING-OWNER-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterPostFrameTailPass.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25PHealingOwnerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsCharacterPostFrameTailPassEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsFramePostProcessPassEditorTests.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp per-slot C25o->C25p and battle_world.cpp advance_native_healing_slot; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-CS0103-X3 / COMPILE0 / FOCUSED6 / RELATED32+44 / NTSD28-BROAD443 / SELFCHECK-PASS-2026-09-05T03:27:04Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / PER_SLOT_HEALING_OWNER / GLOBAL_DUPLICATE_REMOVED`

## 改前事实

- `EntityPostFrameTailAll`及其ECS exact分支都推进HealTimer/CatchTimer/HP，并在C25 loop、legacy serial和random-drop之后全局扫描。
- `BattleLateEntityLifecycleModule`尚无C25p owner。
- 当前算法主体与Authority接近，但没有current DAT type0 gate，且位置使当tick新生高slot、lifecycle survivor和F7顺序均可能不同。

## 计划改动

在dynamic live-slot末端调用统一`BattleNativeHealingKernel`；global post-tail删除healing写入，只保留其独立维护职责。测试同时覆盖placement、type gate、两个timer、state1700以及global no-duplicate。

## 实际改动与证据

- `BattleLateEntityLifecycleModule.Run`在survivor完成当前previous-action commit后调用C25p；current DAT非type0或HP<=0直接跳过。
- 新`BattleNativeHealingKernel`按encoded→ordinary→state1700顺序推进；HP上限使用`HPBound`。
- `SimulationWorld.EntityPostFrameTailAll`和`BattleEcsCharacterPostFrameTailPass`不再推进healing，只保留F7、carrier/transient与snapshot维护。
- 依赖旧全局healing位置的`BattleEcsFramePostProcessPassEditorTests`已更正为断言global no-duplicate。
- 红灯：新fixture编译出现3个预期`CS0103 BattleNativeHealingKernel`。
- 绿灯：focused job `f1d3f0e082f64143a4fdf2f85d01b674` 6/6；相关job `1c83b3d300b942f2ad50b7c91c0f6699` 32/32、`b38bc8a2d1754816919667a27ea7bb45` 44/44；broad job `ef6c187785a94a0c80d85a53b4488d06` 443/443。
- SelfCheck重新请求后于`2026-09-05T03:27:04Z`生成PASS；清空预期负例日志后Console error 0。
- Scene SHA/length/mtime仍为`0D74E174...D77 / 203477 / 2026-09-04T13:12:45.1526434Z`；`git diff --check`无whitespace error；Change Ledger validator PASS（204 Records / 187 governed code files）。

## 未关闭项

C25k-o、正式terminal pending、weapon pieces、内容/声音/RNG不在本包；下一进入C25k/m old-Prev与terminal分类前置审计。
