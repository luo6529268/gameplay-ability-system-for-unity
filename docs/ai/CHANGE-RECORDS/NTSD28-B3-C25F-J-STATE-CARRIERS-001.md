# NTSD28-B3-C25F-J-STATE-CARRIERS-001 — C25f-j state carriers

<!-- CHANGE-RECORD
id: NTSD28-B3-C25F-J-STATE-CARRIERS-001
status: VERIFIED
change-kind: TEST_FIRST_STATE_CARRIERS
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
authority: NTSD 2.8-Logan C25f computer state, C25h timer/status bank and C25i armor carriers; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-60-MISSING-FIELD / COMPILE0 / 12-CARRIERS / SNAPSHOT6-FULL10-CHECKSUM13 / RAW-ARMOR-41-VERIFIED-7-MISSING / JOINT66 / NTSD28-BROAD410 / SELFCHECK-09-11-30-PASS / SCENE-UNCHANGED / CONSOLE0 / ALGORITHMS-CONTENT-EXCLUDED
-->

> 状态：`VERIFIED / STATE_CARRIERS / SNAPSHOT-CHECKSUM-CLOSED / ALGORITHMS-EXCLUDED`

## 改前事实

- render phase已唯一绑定HitStop，不能再加重复字段。
- C25f computer、C25h injury/native/status/join/poison及C25i armor共12字段仍无Unity carrier。
- raw armor目前固定null；snapshot5/full9/checksum12不承诺这些状态。

## 计划

先以新字段/default/schema/raw断言取得红灯，再补运行时载体和全部确定性边界；不连接任何算法或内容producer。

## 实际改动

- 新增12个C25f/h/i字段，armor recovery sentinel显式为-1，其余为0；统一reset helper并进入canonical deep copy。
- entity runtime/full/checksum schema 5/9/12→6/10/13；lockstep checksum按固定顺序加入全部字段，parity新增`nativeReactionStatus`组。
- `combat.runtimeArmorHp/armorRecoveryTimer` contract与Unity raw由MISSING/null提升为verified真实字段；48-field maturity 39/9→41/7。
- 新测试逐字段证明default/Reset、checksum敏感、parity分组、raw armor与schema；reflection snapshot/warm allocation旧测试共同覆盖复制完整性。

## 验证

- 红灯：60个CS1061，全为新测试引用的12个缺失字段。
- Unity compile0；联合 `1e376912240c4467affabdd880a031c9` 66/66；NTSD28 broad `27ba64974f82438287ae8feed93f6155` 410/410。
- parity tool：并行build/self-test首次因同一obj DLL文件锁导致2个命令失败；随后严格顺序重跑，build 0 warning/0 error、trace21/21、raw5/5。
- 更新后的Unity raw SHA`4DB66F9D21CCD9AEFB7F8F32A2C4FC5ED34BBB6DF3084D44D8D9C71D8C5F0795`；comparison SHA`125A06979D852322B0A05FE99300EE45CB6F34C4419515E5886BB961882C9C4D`，3tick/6pair/288 occurrences、armor两字段equal、unique equal33/diff15。
- final SelfCheck 09:11:30 PASS；未Play，Scene unchanged，post-clear Console0。

## 未关闭边界

- 全部字段当前只有载体与确定性状态边界；没有production algorithm/producer。
- poison/delay/armor正式内容仍受H/B11 gate；AI render-phase consumer、C25f refresh、C25h timer、C25i recovery、C25j owner另包。
