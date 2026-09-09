# NTSD28-B3-C06-NESTED-PHYSICS-PRODUCTION-001 — C06嵌套physics production

<!-- CHANGE-RECORD
id: NTSD28-B3-C06-NESTED-PHYSICS-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C06 slot loop and BattleWorld28 normalize_native_dead_character_resources; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-C06-LINES-591-609 / PHYSICS-THEN-IMMEDIATE-NORMALIZE / NORMALIZE-TYPE0-HP-LE-0 / HPBOUND-PP-ONLY / CURRENT-MP-MAPS-PP / RED-2-OVERRIDE-SEAMS / UNITY-COMPILE-0 / FOCUSED-6-OF-6-JOB-CBFBAF36 / ACTUAL-C04-C06-22-OF-22-JOB-A128FF5C / NTSD28-92-OF-92-JOB-CA1B89BD / FRAME-RUNTIME-23-OF-23-JOB-9CEDAE22 / WORKER-20-OF-20-JOB-A5960241 / PHYSICS-RELATED-22-OF-22-JOB-40BEEFB3 / SELFCHECK-2026-09-05T01-38-19-PASS / TARGETED-C06-PLAY-PASS / C06-RESULT-SHA-50BA937F / C05-REGRESSION-PLAY-PASS / C05-RESULT-SHA-8064DFCE / CLEANUP-PASS / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / FULL-31-PARTIAL-4 / NEXT-C07 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C06-PRODUCTION-OWNER / TARGETED-PLAY-PASS / NEXT-C07`

## 改前事实

- Authority按升序slot执行`step_physics`并收集audio，随后在同一slot内立即调用normalizer；下游hit造成的死亡
  延到下一tick归一化。
- Normalizer只在current definition type0且`current_hp<=0`时写`effective_max_hp=0/current_mp=0`。
- Unity production的character physics在`SimTransit`，non-character physics与部分state/death在`SimTU`，全部位于
  一个后续逐实体`SerialTickAll`；没有C06显式owner，也没有相同时点的dead-resource normalizer。

## 计划

- 先写预期失败的focused tests，再添加显式C06 phase与per-slot production owner。
- 拆分物理入口与`LF2SpecialAttack`非物理尾；后续serial production只运行非物理剩余职责。
- 保留direct `SerialTickAll`行为，并以相关回归与真实Play验证没有重复物理。

## 实际改动

- production新增`NestedPhysics` phase，位置固定在C05后、现有serial remainder前；full occurrence 30→31，
  input-clear partial仍为4。
- `SimulationWorld`在同一升序slot循环内运行现有physics owner并立即normalizer；exact character继续复用
  `BattleEcsCharacterFrameAdvancePass`，未知/多态壳走virtual seam。
- 后续production `SerialTickAll`不再重复SimTransit/SimTU physics，只运行post-native-physics remainder并刷新snapshot；
  direct重载保持旧行为。
- `LF2SpecialAttack`的物理部分与state-entry/state15/death尾拆开；后者未提前到C06。
- C05 Play probe改用新C06 seam冻结后续physics，仅修正测试观察边界。

## 验证

- red compile：2项missing override seam；实现后Unity compile error 0。
- focused首轮5/6的失败来自synthetic current-DAT fixture fallback，未改production；final `6/6` PASS，job
  `cbfbaf3636254388aedef9a02a6fd0b2`。
- actual/C04～C06 `22/22` job`a128ff5cb00a4b539c2e77883cd19232`；NTSD28 `92/92`
  job`ca1b89bd29d74f469b5c43128ae22868`；frame/runtime `23/23` job`9cedae2211b14041a8b6744d9f460966`；
  worker `20/20` job`a596024100ef4b0da7487d8e6ae2b25a`；physics相关 `22/22`
  job`40beefb38dc447edab9ab23066b9eb03`。
- SelfCheck `2026-09-05 01:38:19 +08:00` PASS。
- targeted Play首次在完整tick末断言PP=0时失败，观测到后续资源pass可再写PP=6；这是越过C06边界的
  test-only假设。最终probe改为高slot physics入口观察低slot，得到C06 checkpoint `HPBound=0/PP=0`，
  无关字段保留、cleanup PASS；SHA`50BA937F5FF8F983C7136D8B60156E92D823906181D0C629B949D8E04820D38B`。
- 新C06首次复跑旧C05 probe因probe仍靠覆盖SimTU冻结physics而失败；改用新C06 test seam后C05再次PASS，
  SHA`8064DFCED92494BBDAEF23342499890DE9ECEA6DE890851DB86CAA1D1B778C7B`。两次均未因失败修改production。
- Scene SHA`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`不变，Play退出，
  最终Console error 0。

## 未关闭边界

- C06物理公式、gate与audio细节仍需B4/B10逐项权威对照；本包只闭合production owner和嵌套normalize边界。
- C07 revival、C25 nested tail、mutation/reuse精确语义不在本包；下一结构首差为C07 revival对现有serial remainder。
