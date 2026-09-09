# NTSD28-B0-F8-OWNER99-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B0-F8-OWNER99-PRODUCTION-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_RUNTIME_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/BattleRandomWeaponDropModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B0F8Owner99ProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan playable GameSession28::step post-tick drop_objects consumer, consume_native_f8_drop, NativeFunctionKeyDropSpawn28 owner_slot=99 and SpawnRequest28 materialization; EXE B1E13AE1, closure 39DDDA15.
evidence: pre-change audit proves Unity legacy Mode2Request==1 materializer created a null-parent task with owner -1, while physical PendingObjectCommand consumption remains absent and excluded. After correcting only the test oracle for existing mode2 post-init Z +1, pre-production v1 at 2026-09-08 23:19:13 local produced exact RED 1 pass/2 fail: normal-drop owner/RNG/position passed, while mode2 slot50 and occupied-prefix slot399 both expected owner99 and observed -1 after frame/slot prerequisites. Production now writes only spawnTask.ownerEntityIndex=99 immediately before CreateObjectImmediate. Runtime and Editor out-of-process builds passed with 0 errors/47 and 0 errors/104 warnings. Unity refreshed assemblies at 23:24:36; v1 passed 3/3 at 23:25:56. Full SelfCheck ran at 23:27:20, passed this earlier random-weapon check, and stopped later at the pre-existing unrelated CPoint throw-Vz assertion. Play, physical-F8 integration and joint trace remain pending.
-->

> 状态：`FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_3_OF_3 / SELFCHECK_REACHED_UNRELATED_CPOINT_AFTER_NEW_CHECK / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_3 / MODE2_MATERIALIZER_OWNER_ONLY / PHYSICAL_F8_EFFECT_WIRING_EXCLUDED`

完整范围、Authority、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B0-F8-OWNER99-PRODUCTION-001.md`。

## 实施与验证

- 新focused测试已由full asset refresh进入Unity Editor程序集。首次运行`1/3`时先暴露测试遗漏
  mode2 factory既有post-init Z `+1`；该测试判据已纠正，不改变production。
- 纠正后的pre-production v1于2026-09-08 23:19:13本地实际得到
  `1 passed / 2 failed / 0 skipped / 0 inconclusive`：normal-drop owner/RNG/position保护通过；mode2
  lowest-free slot50和occupied-prefix slot399均在frame/slot前置通过后精确失败为
  `expected owner 99 / actual -1`。
- `BattleRandomWeaponDropModule.SpawnMode2RandomWeapons()`仅在既有null-parent task完成构造后、
  `CreateObjectImmediate`调用前写`ownerEntityIndex=99`；`RunNormalDrop()`、required slot、候选、RNG、
  frame、位置和post-init均未改。
- `CheckRandomWeaponDropAuthorityContract()`新增mode2 slot50 owner99/四次RNG/frame/位置检查，并给既有
  normal-drop补owner`-1`与claimed entity/raw backing分离断言。focused另覆盖occupied-prefix slot399。
- 进程外编译：`Assembly-CSharp.csproj`为`0 errors / 47 warnings`；
  `Assembly-CSharp-Editor.csproj`为`0 errors / 104 warnings`。
- Unity于23:24:36刷新runtime/editor程序集；v1于23:25:56实际
  `3 passed / 0 failed / 0 skipped / 0 inconclusive`。
- full `BattleRuntimeSelfCheck`于23:27:20实际运行；本包random-weapon检查位于调用序列前部并已通过，
  随后仍在既有、无关的`CheckCpointThrowRawAndTransformMatrix` throw-Vz断言处停止。
- 正式物理F8 pending consumer、Play与joint trace未执行，保持`RUNTIME_PENDING`。
