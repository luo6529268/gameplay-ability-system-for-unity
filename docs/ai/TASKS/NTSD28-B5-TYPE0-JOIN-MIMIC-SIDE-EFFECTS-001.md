# Task Contract — NTSD28-B5-TYPE0-JOIN-MIMIC-SIDE-EFFECTS-001

> 状态：`VERIFIED / TYPE0_JOIN_MIMIC_IMMEDIATE_EFFECTS_ALIGNED / OTHER_TYPES_DEFERRED`
> 依赖：`NTSD28-B5-TYPE0-ENCODED-STATUS-001 / VERIFIED`

## 目标

在 type-0 confirmed hit 中，于 encoded status 写入后立即接入权威
`apply_native_join_and_mimic_side_effects`。

## Authority 合同

- join：当 target `JoinTimer148 > 0` 且 `JoinOverrideActive170 != 1` 时，置active、
  保存 target 当前 `RelationTeam`，再复制 attacker `RelationTeam`。
- mimic：仅 attacker/target 均为 type 0、target counter正数且尚未enabled时，记录attacker
  slot并置enabled。
- 已激活关系不得重入或覆盖原始恢复值。
- 调用位置：encoded producer之后、unarmored reaction之前。

## 不变量

- 复用现有 `NTSD28InputProxyControlLifecycle`，不新增第二套 mimic 生命周期。
- 已实现的 C25 tail join/mimic expiry owner不变。
- 不改Config/DAT/Scene/Authority/资源/snapshot schema/RNG顺序。
- 本包只接type0 standard target；其他target type后置。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type0JoinMimicSideEffectsEditorTests.cs`

## 验收

test-first red；focused覆盖join gate/re-entry、mimic type/counter/enabled gate、combined及production
encoded→side-effect顺序；相关B2/B3/B5、精确NTSD28 broad、SelfCheck、Scene/Console/Ledger。

## 回滚

移除side-effect helper、production调用与focused test；保留既有carrier/expiry/encoded producer。

## 验证结论

- test-first fresh compile red：5个预期`CS0117`。
- fresh compile 0 error；focused `7b8a5ca511204f5e9b9cd09e6734fda0` 10/10。
- B2 proxy/C25 expiry/B5 related `f2f6fa2ef9934644b053e5362a0645ef` 44/44。
- 精确NTSD28 broad `cec99e89ef0640b0ab75a0d98ed4353d` 587/587。
- BattleRuntimeSelfCheck `2026-09-05T11:13:36Z` PASS；Scene unchanged。
