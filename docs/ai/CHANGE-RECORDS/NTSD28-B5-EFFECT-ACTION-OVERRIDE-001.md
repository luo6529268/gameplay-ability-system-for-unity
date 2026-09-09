# NTSD28-B5-EFFECT-ACTION-OVERRIDE-001 — effect8..16 action override

<!-- CHANGE-RECORD
id: NTSD28-B5-EFFECT-ACTION-OVERRIDE-001
status: VERIFIED
change-kind: TEST_FIRST_AUTHORITY_BEHAVIOR_PORT
code-path: Assets/NTSD/Scripts/Animation/LF2CharacterData.cs
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5EffectActionOverrideEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan battle_world.cpp native_effect_action_override_is_suppressed/native_effect_action_target_type_matches/apply_native_effect_action_override and resolve_relation_hit order; EXE B1E13AE1, closure 39DDDA15.
evidence: EFFECT-AUDIT-VERIFIED / AUTHORITY-EFFECT8-CORPUS-808 / PROPERTY-CARRIER-MISSING / PRIMARY-BDY-KIND-CARRIER-MISSING / TEST-FIRST-RED-d45af0c4ea924ef8a51f2401e49833eb-16FAIL-7PASS / COMPILE-0 / FOCUSED-6e911e42281d448d930c38240893a590-24OF24 / HITPLAN-B5-62387abe2aaa41b7b1f70414f3f5302d-320OF320 / EXACT97-BROAD-250000c7231e48bd9780cc3313a21668-698OF698 / SELFCHECK-PASS-20260905T160121Z / CONSOLE0 / LEDGER263-230-PASS / SCENE-CONCURRENT-USER-EDITOR-CHANGE-PRESERVED
-->

> 状态：`VERIFIED / EFFECT8_16_ACTION_OVERRIDE_ALIGNED`

实现统一effect8..16 override kernel及actual/hit-plan owner，不包含随后direct post-effect action。

Test-first 23项中16项按预期失败；既有formal BDY X/Y/W/H值合同不含kind，因此以Frame级首BDY kind
独立载体补足suppression输入，不修改formal geometry equality/hash/content fingerprint。

## 实际改动

- definition `property`由BMP parser进入`LF2CharacterData`；首BDY kind以独立Frame carrier保存，formal
  `BattleBodyBoxValue(X/Y/W/H)`合同不变。
- 统一resolver实现effect8..16 target-type矩阵、action-latch首BDY 50/52、state602/603、property2/3、
  caughtact -2/-3抑制、pickedact previous-state gate及positive catching/caught action。
- target action使用post-damage HP gate，致死反应不被覆盖；action写入不清AttackingCounter。
- actual character、weapon、type3/other及四条hit-plan projection在type3 continuation之后调用同一resolver。
- direct post-effect、reduced armor、audio/spark、effect5000/6000均未扩张。

## 验证

- red `d45af0c4ea924ef8a51f2401e49833eb`：23项，16 fail/7 pass。
- compile0；focused `6e911e42281d448d930c38240893a590` 24/24。
- hit-plan+B5 exact 18类 `62387abe2aaa41b7b1f70414f3f5302d` 320/320。
- exact NTSD28 97类 `250000c7231e48bd9780cc3313a21668` 698/698。
- SelfCheck 2026-09-05T16:01:21Z PASS；Console0；Ledger 263 records/230 code files PASS；
  diff-check仅CRLF警告。
- Scene继续由并发用户/Editor写入，最终观察SHA
  `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`、205625 bytes、
  mtime `2026-09-05T15:44:52.7794120Z`；本包未写Scene，原样保留。
