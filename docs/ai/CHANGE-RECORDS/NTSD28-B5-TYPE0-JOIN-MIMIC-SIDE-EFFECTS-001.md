# NTSD28-B5-TYPE0-JOIN-MIMIC-SIDE-EFFECTS-001 — type0 join/mimic immediate effects

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE0-JOIN-MIMIC-SIDE-EFFECTS-001
status: VERIFIED
change-kind: TEST_FIRST_CONFIRMED_HIT_SIDE_EFFECT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type0JoinMimicSideEffectsEditorTests.cs
authority: NTSD 2.8-Logan apply_native_join_and_mimic_side_effects in confirmed type0 hit live path; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-SOURCE-AND-CALL-ORDER-READ / EXISTING-CARRIERS-EXPIRY-AND-PROXY-LIFECYCLE-READ / TEST-FIRST-COMPILE-RED-CS0117-X5 / COMPILE0 / FOCUSED10 / RELATED44 / NTSD28-BROAD587 / SELFCHECK-PASS-2026-09-05T11:13:36Z / SCENE-UNCHANGED
-->

> 状态：`VERIFIED / TYPE0_JOIN_MIMIC_IMMEDIATE_EFFECTS_ALIGNED / OTHER_TYPES_DEFERRED`

## 改前事实

- encoded producer已能写`JoinTimer148`和`InputProxyCounter14C`，但production尚未触发即时关系副作用。
- join override carrier/expiry和mimic proxy control lifecycle均已存在。
- 本包只闭合confirmed hit后的即时激活，不改变C25 expiry owner。

## 验证状态

focused test已先写入，fresh compile得到5个预期`CS0117`缺
`ApplyNativeJoinAndMimicSideEffects`；现在进入最小生产实现。

- join仅在positive timer且未active时保存原group并复制attacker group；active re-entry保留
  original/current，non-positive无副作用。
- mimic复用既有`NTSD28InputProxyControlLifecycle.TryEnableFromConfirmedHit`，保持type0/type0、
  positive counter、not-enabled与attacker slot gate。
- production调用紧随encoded producer；production test证明本次命中新写入join/mimic counter后
  同次即时激活。
- fresh compile 0 error；focused `7b8a5ca511204f5e9b9cd09e6734fda0` 10/10；
  related `f2f6fa2ef9934644b053e5362a0645ef` 44/44；精确NTSD28 broad
  `cec99e89ef0640b0ab75a0d98ed4353d` 587/587。
- BattleRuntimeSelfCheck `2026-09-05T11:13:36Z` PASS；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged。其他target types后置。
