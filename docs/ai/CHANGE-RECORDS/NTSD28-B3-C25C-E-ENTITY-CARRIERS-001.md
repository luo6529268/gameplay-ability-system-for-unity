# NTSD28-B3-C25C-E-ENTITY-CARRIERS-001 — C25c-e entity carriers

<!-- CHANGE-RECORD
id: NTSD28-B3-C25C-E-ENTITY-CARRIERS-001
status: VERIFIED
change-kind: TEST_FIRST_STATE_CARRIERS
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 fields consumed by C25c-e and C25d display mirror; C25c-e inventory; current_mp PP correction prerequisite.
evidence: RED-MISSING-FIELDS / COMPILE0 / FOCUSED4 / SNAPSHOT-CHECKSUM29 / NTSD28-BROAD405 / SELFCHECK-PASS / SCENE-UNCHANGED / ALGORITHM-EXCLUDED
-->

> 状态：`VERIFIED / ENTITY-CARRIERS / SNAPSHOT-CHECKSUM-CLOSED / ALGORITHM-EXCLUDED`

## 改前事实

- C25d四值四step完全无Unity载体；C25c/e多个status/attribution/full-restore字段缺失。
- Entity runtime snapshot依赖`TryCopyCanonicalStateTo`；新增字段若未复制会被reflection test捕获。
- checksum schema11、entity snapshot4、full snapshot8尚未承诺这些字段。

## 计划

先新增字段/schema/checksum预期测试取得红灯，再补NTSDEntityRuntime与所有确定性状态边界；不连接resource/display算法。

## 实际改动

- `NTSDEntityRuntime`新增6个resource/status、7个attribution/damage和8个C25d display value/step字段；带有native默认值的gate、mode scale与source slot均显式初始化。
- 新字段进入完整Reset和canonical deep copy，因此entity runtime snapshot capture/restore与raw slot copy共享同一确定性真值。
- `BattleWorldEntityRuntimeSnapshotBuffer` 4→5、`BattleStateSnapshotBuffer` 8→9、`BattleLockstepChecksumModule` 11→12；所有旧精确schema断言同步更新。
- checksum按固定顺序包含21个字段；parity JSON新增与旧`presentation/PpDisplay`分离的`nativeResourceDisplay`组。
- 新focused测试逐一证明默认/Reset、每个字段影响parity checksum、独立JSON组与schema版本；既有reflection snapshot测试证明全部字段完整复制且warm capture不分配。

## 验证

- 红灯：字段加入前Unity编译得到100个预期missing-field错误，来源均为新test-first测试。
- 编译：现有Unity Editor force/all refresh及domain reload后C# error=0。
- focused `1c9acfafa03e4618a3f1852ecf7883e0`：4/4 PASS。
- snapshot/checksum/action/environment联合 `7fc4a6910983464bbbb6aab4ee647eb7`：29/29 PASS，包含warm 0-allocation断言。
- `NTSD28*` broad `602a215500bf4952904303fb36a9cd54`：57类、405/405 PASS。
- 最终SelfCheck：2026-09-05 08:16:42 PASS；post-clear Console error=0。
- Scene：未进入Play，`Assets/NTSD/Scene/NTSD_Battle.unity` SHA-256仍为`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`。
- 额外诊断：一次误传`filter`导致全量2006项EditMode被执行；其中既有渲染资源、Server package版本、structure guard、OPoint/pool等测试失败，均不命中新载体/schema路径。该运行不作为本包绿色证据；随后独立精确broad 405/405与domain reload后的SelfCheck PASS为正式证据。

## 未实现与风险

- 不含C25c pre-resource、C25d display progression、C25e post-resource算法。
- 不含mode world rules、definition stats presence/max_mp/regen/bound、frame cmp/chp/bmp frame_0mp。
- `CatchSourceSlot90`不等于`CatcherSlotIndex`；environment/impact attribution、KO/score和display step producer仍待B5/B8闭合。
- 因未改变可观察战斗行为，本包无需Play；后续算法包必须追加同tick trace和真实Play证据。
