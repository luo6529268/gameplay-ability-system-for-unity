# NTSD28-B4-F04-TYPE0-PHYSICS-CORE-001 — type0 reference-aware physics core

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-TYPE0-PHYSICS-CORE-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4Type0PhysicsCoreEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan physics_integrator.cpp friction/effective-floor/type0 crossing core; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-RED-3-OF-5 / COMPILE0 / FOCUSED5 / RELATED33 / NTSD28-BROAD470 / SELFCHECK-PASS-2026-09-05T05:16:05Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / TYPE0_CORE / ACTIONS_AND_PRODUCERS_PENDING`

只接type0 mechanics consumer；carrier producer与landing action均保持后置。回滚恢复零地面判定并
删除focused test，无数据迁移。

红灯job `90caf552bd2f43ea8b33c71585eef925`为3/5失败：negative floor未摩擦、未landing，
以及已在floor仍重复landing。production现使用reference-aware friction/effective floor/strict crossing。

SelfCheck PH-02仍固定旧C# ±0.0001容差和contact-side不clamp语义；按正式direct compare与type0
contact clamp更正五条断言，cpoint kind2特殊旁路保持不变。

## 验证与未关闭

- compile0；focused job `76d5b6b79b644bd2aa0934ab806313c5` 5/5；related
  `aa15dd677e4f4641a3ae20e724b4c31f` 33/33；broad
  `64a43c4ccdc342d6b96bbb955564aa1f` 470/470。
- SelfCheck先后捕获PH-02与late-opoint同源旧contact-side断言；全部改为clamp但不landing后，
  `2026-09-05T05:16:05Z` PASS。
- Scene仍`0D74E174...D77 / 203477 / 2026-09-04T13:12:45Z`，Console0，diff-check
  无whitespace error，Ledger 217/195 PASS。
- landing action/environment damage、non-character、operation30/linked producer、teleport/next999/input/hit
  均未关闭；不宣称完整type0或F04对齐。
