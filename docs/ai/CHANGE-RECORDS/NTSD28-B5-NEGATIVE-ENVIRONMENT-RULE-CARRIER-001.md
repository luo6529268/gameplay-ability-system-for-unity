# NTSD28-B5-NEGATIVE-ENVIRONMENT-RULE-CARRIER-001 — negative environment rule carrier

<!-- CHANGE-RECORD
id: NTSD28-B5-NEGATIVE-ENVIRONMENT-RULE-CARRIER-001
status: VERIFIED
change-kind: TEST_FIRST_DETERMINISTIC_WORLD_CARRIER
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NegativeEnvironmentRuleCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4SourceCountCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NativeComboCarriersEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5SpecialHitLatchCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitGroupModeGateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5WorldHitResourceRulesCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
authority: NTSD 2.8-Logan ResourceSystemRules28 +0x90 default9 and pre-display negative EnvironmentState320 consumer; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-DEFAULT9-AND-RAW-FALLBACK-READ / TEST-FIRST-RED13 / COMPILE0 / FOCUSED6 / RELATED106 / EXACT-NTSD28-1394 / BUILDS0 / SELFCHECK-BLOCKED-UNRELATED-CPOINT / CONSOLE0 / SCENE-BASELINE-UNCERTIFIED / LEDGER419-349-PASS
-->

> 状态：`VERIFIED / RED_13 / FOCUSED_6_OF_6 / RELATED_106_OF_106 / NTSD28_1394_OF_1394 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_BASELINE_UNCERTIFIED / CARRIER_READY / RECOVERY_CONSUMER_NEXT`

## Authority、原状与计划

正式 `ResourceSystemRules28::negative_environment_damage_90` 默认 `9`；native pre-display
negative-environment transaction把非正值fallback到9。Unity既有world hit-resource rules carrier缺该字段，
因此下一 exact recovery consumer不能硬编码规则或借用WeaponCount/FallDamageDiv。

本包只新增字段并打通 reset、core/full snapshot restore、checksum、full parity和schema。test-first RED、
实际文件/符号、验证结果、风险与未验证项将在实施后追加。consumer、producer、event和legacy schema删除不在范围。

## Test-first RED

新 focused test 导入当前 Unity 后取得13个且仅13个预期编译错误：`CS0117` 1个、`CS1061` 8个、
`CS1501` 4个，全部指向尚不存在的默认常量、runtime/core字段或5参数restore overload；没有范围外错误。

## 不可回退边界与回滚

不触碰Authority、content、Scene、Prefab、ProjectSettings或现有战斗分支。回滚删除本字段/test并恢复
core/aggregate/checksum schema与既有版本断言；不存在数据迁移。

## 实际实施

- `NTSD28HitResourceRulesRuntimeState` 新增默认9的
  `NegativeEnvironmentDamage90`，full reset恢复9；5参数restore保存raw值，既有3/4参数overload继续
  兼容并给新字段Authority默认9。
- `BattleWorldHitResourceRulesScalarSnapshot` 捕获该raw值，full battle restore原样恢复；carrier层
  不把0/负值改成9，fallback仍属于后继consumer事务。
- lockstep checksum和full parity分别追加该字段与稳定键
  `negativeEnvironmentDamage90`。
- world core / aggregate battle snapshot / lockstep checksum schema
  `10/19/22 -> 11/20/23`；entity runtime snapshot保持12，所有精确版本断言已同步。
- 新focused test覆盖default/reset、负数raw restore、immutable capture、full restore、checksum/parity、
  schema和4096次warm capture/checksum 0 B。

## 实际验证与未验证项

- RED13；focused6/6；related106/106；纠正选择器后的精确NTSD28 broad1394/1394。
- runtime/editor串行build分别47/104 warning、均0 error；并行Editor尝试的`CS2012`文件占用已由
  串行成功消歧。
- full SelfCheck仍由既有CPoint throw-Vz断言阻塞；清理预期日志后Console 0。
- Scene当前dirty=false且本包没有Scene写命令，但与上个checkpoint基线不同；因缺少本包事前新鲜hash，
  不声明Scene unchanged，保留现有用户Scene改动。
- 本包只证明carrier及确定性闭包；negative-environment recovery consumer、B6 impact producer、
  B8 event与legacy schema删除仍未实施。下一包为
  `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-PRODUCTION-001`。
