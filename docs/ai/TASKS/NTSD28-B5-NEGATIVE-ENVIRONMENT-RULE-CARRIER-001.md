# Task Contract — NTSD28-B5-NEGATIVE-ENVIRONMENT-RULE-CARRIER-001

> 状态：`VERIFIED / RED_13 / FOCUSED_6_OF_6 / RELATED_106_OF_106 / NTSD28_1394_OF_1394 / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_BASELINE_UNCERTIFIED / CARRIER_READY / RECOVERY_CONSUMER_NEXT`
> 依赖：`NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001 / VERIFIED`

## 目标

在既有 `NTSD28HitResourceRulesRuntimeState` 中新增 world-owned
`NegativeEnvironmentDamage90`，Authority 默认值为 `9`，并闭合 reset、core scalar snapshot、
完整 battle snapshot restore、lockstep checksum、full parity 和热路径零分配合同。

本包只建立确定性数据 carrier；negative-environment recovery consumer、B6 impact producer、
B8 knockout event feed、legacy stat/schema 删除均不接入。

## Authority 合同

- 当前正式 EXE SHA-256 为
  `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`，playable closure 为
  `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- `battle_world.h::ResourceSystemRules28::negative_environment_damage_90` 默认 `9`，对应 active
  battle-rule record `+0x90`；字段缺失或非正时 native consumer fallback `9`。
- `battle_world.cpp::advance_native_resources_pre_display_slot(...)` 在 negative
  `EnvironmentState320` 且 `ResourcePhase12==0` 时读取该 rule；本包不实现该 consumer。
- 该值属于 world/session rule，不属于 entity、WeaponCount、FallDamageDiv、DAT frame 或表现层。

## 事前原状

- `NTSD28HitResourceRulesRuntimeState` 已持有 `+0x18/+0x1C/+0x34/+0x38`，缺 `+0x90`。
- core scalar snapshot/restore、checksum 与 full parity 已覆盖上述四项，缺本字段。
- 当前 schema 为 core `10`、aggregate battle snapshot `19`、lockstep checksum `22`、entity runtime
  `12`。新增 world scalar 后预期推进为 `11/20/23`，entity runtime保持 `12`。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- 新 `NTSD28B5NegativeEnvironmentRuleCarrierEditorTests.cs` 及只含版本断言更新的既有 tests。
- 治理恢复入口、Ledger、Task/Change Record。

## 不变量

- 不修改两个现存 recovery 分支；不改变 `WeaponCount`、`EnvironmentState320`、native phase 或伤害。
- 不修改 C++ Authority、DAT/PNG/WAV、Scene、Prefab、ProjectSettings、Input Actions 或网络协议。
- 不新增 per-frame allocation；snapshot/checksum保持 warm 0 B。
- 不改变 entity runtime snapshot schema、B0 raw schema或既有字段的默认值、顺序和语义。

## 验收

1. focused test先于生产代码导入并取得只由缺失 field/property/signature/schema 导致的预期 RED。
2. 默认/reset为9；显式 restore 精确保留正值、零值和负值，不在 carrier 层应用 consumer fallback。
3. immutable core capture与完整 battle snapshot restore保留字段。
4. 每次字段变化都会改变 lockstep checksum，full parity包含稳定键和值。
5. core/aggregate/checksum schema推进为 `11/20/23`，entity runtime保持 `12`。
6. 4096次 warm core capture + checksum为0 managed bytes。
7. fresh compile、focused、相关 snapshot/checksum、精确 NTSD28 broad、SelfCheck、Console、Scene与
   Change Ledger按实际结果记录；carrier包无需单独战斗伤害 Play 验证，若运行 focused Play wrapper则如实记录。

## 回滚

删除新字段与 focused test，恢复 snapshot/restore/checksum/parity写入和 schema `10/19/22`，并恢复
既有测试版本断言。本包不涉及内容迁移或 Scene/Prefab 回滚。

## 验证结论

- test-first Unity compile RED 为13个预期错误：`CS0117 x1 / CS1061 x8 / CS1501 x4`，均为缺失
  constant/field/core property/5参数restore overload；无范围外错误。
- fresh Unity refresh后Console compile error为0；focused job
  `b5e29b11c0f54a45930fc9046f6f3aeb` 为6/6。
- 19个相关 snapshot/carrier class job `b257da1ab978434ba4386040bc382f7d` 为106/106。
- 首次 broad选择器误包含上层namespace，job `39a0100a45894194b2a3cf235b7c6232` 执行1404项，
  仅既知CPoint fixture因非法Category名产生10个OneTimeSetUp失败；不作为通过证据。纠正为163个精确
  `NTSD28*EditorTests` class并排除该已知无效fixture后，job
  `4ad5e50b6ad54ce3a0f1e04f1af07038` 为1394/1394。
- 串行 `dotnet build Assembly-CSharp.csproj --no-restore` 为47 warning/0 error；
  `Assembly-CSharp-Editor.csproj` 为104 warning/0 error。一次并行Editor尝试因与runtime工程共同写
  `obj/Debug/Assembly-CSharp.dll` 得到`CS2012`文件占用，串行重跑已证明不是源码错误。
- full `BattleRuntimeSelfCheck` 仍在既有
  `BattleRuntimeSelfCheck.cs:10939` CPoint throw mode0 Vz断言失败，与此前阻塞一致；未在本包修复。
  清理该预期negative-path日志后Unity Console error为0。
- 当前Scene为`NTSD_Battle`、dirty=false、roots=13，磁盘SHA
  `D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11`、209891 bytes、
  mtime `2026-09-09T00:34:59.7782493Z`。它与上个checkpoint的Scene基线不同，且本包开始时未重新
  采集基线，因此不声称Scene unchanged；本包未发出Scene编辑/保存命令，文件中也没有新carrier token，
  现有用户Scene改动保持不动。
- Change Ledger validator最终为419 records / 349 governed code files PASS。
