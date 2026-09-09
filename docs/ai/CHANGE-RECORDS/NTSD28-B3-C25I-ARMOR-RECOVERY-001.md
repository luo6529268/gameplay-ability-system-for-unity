# NTSD28-B3-C25I-ARMOR-RECOVERY-001 — C25i armor recovery core

<!-- CHANGE-RECORD
id: NTSD28-B3-C25I-ARMOR-RECOVERY-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25IArmorRecoveryEditorTests.cs
authority: NTSD 2.8-Logan battle_world.cpp advance_armor_recovery_slot; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-CS0103-X8 / COMPILE0 / FOCUSED5 / RELATED96 / NTSD28-BROAD437 / SELFCHECK-PASS / FORMAL-CONTENT-PENDING / SCENE-UNCHANGED / CONSOLE0
-->

> 状态：`VERIFIED / PROGRAMMATIC_CORE_AND_PLACEMENT / FORMAL_CONTENT_PENDING`

## 改前事实

- runtime carriers ready；production在C25h后直接执行C25j，C25i缺失。
- Unity无armor block schema，不能从正式content reload。

## 计划

先对programmatic profile与h-i-j顺序取得红灯；再新增默认false只读profile seam及无分配状态机，运行完整相关门禁。

## 实际改动

- `LF2Entity`新增默认false、零分配、无singleton的armor profile只读seam；没有新增状态carrier或猜测正式内容。
- `BattleNativeArmorRecoveryKernel`实现Authority timer/reload状态机。
- `BattleLateEntityLifecycleModule`按timer/body gate短路，在C25h与C25j之间执行C25i；ordinary hold和negative relation冻结，type3忽略hold。
- focused test覆盖纯core、production no-profile、h-i-j顺序与relation/hold/type3 gate。

## 验证

- RED：refresh后8个CS0103，全部仅来自新kernel尚未存在。
- focused：`6899f8637a254c33a134049b268d6470` 4/4；最终`9150aef527e74e8f81c26043489212f0` 5/5。
- 回归捕获：`a400ce4aa54c444e8d8beffb28906543` 96项中14项失败，证明误把timer gate放进C25h；修复后不改旧断言，`3aac0dfedf62460590fa3efe51a77249` 96/96。
- broad：`37f2585449a449d58af2d1b872baf9f8` 437/437。
- 2026-09-05 11:05:02 SelfCheck PASS；post-clear Console error=0；Scene unchanged；未Play。
- 该证据只证明programmatic core/placement，正式armor行为仍为`CONTENT_AND_HIT_PENDING`。
