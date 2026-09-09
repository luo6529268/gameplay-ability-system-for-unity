# Task Contract — NTSD28-B5-ITR-DEFENSE-FIELDS-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / BEHAVIOR_UNCONNECTED`
> 依赖：`NTSD28-B5-REDUCED-HIT-DAMAGE-PURE-CORE-001 / VERIFIED`

## 目标

补齐 Authority `InteractionRecord28.spark/dbdefend` 在 Unity ITR 数据契约中的 typed carrier、parser、copy 与
HitPlan immutable projection/fingerprint，使已验证 ordinary-defense resolver 可在后续生产接线读取完整输入。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5ItrDefenseFieldsCarrierEditorTests.cs` 与 `.meta`
- 本 Task/Change、Ledger、STATE、handoff、总表与 armor manifest

## 不变量

- 字段缺失默认 0；只解析精确 token `spark`、`dbdefend`。
- `CopyFrom`、HitPlan projection 与两种 fingerprint 必须保持字段；kind5 replacement 不得留下 stale 值。
- 不改变 selection、damage、rest 或 production route；不修改 Config、Scene、Prefab、snapshot schema。
- 当前 Unity 138-DAT 与 Authority character/object DAT 的 ITR/weapon-strength `spark/dbdefend` 均未显式声明，
  因而默认 0 行为保持不变；本包只关闭非默认 contract 缺口。

## 验收

test-first 覆盖 default、parser、CopyFrom、raw/projection fingerprint、projection carrier 与 kind5 replacement；
随后 compile、focused、B5、HitPlan、NTSD28 broad、SelfCheck、Console、Scene 与 Ledger。

## 验收结果

- test-first red：job `f48684ea52144bceae017beaa2f59826`，4/4 按预期失败。
- focused green：job `1ab891ed476240ef87b227e975f6610c`，4/4 通过。
- B5：job `2ad1951e7f3c4d43ad42a6c5ad07cf90`，269/269；HitPlan：job
  `d66f5bc32d6e4002aaaca9e5db76477b`，184/184。
- NTSD28 broad：job `6c5f4a3e449b4be3a01085f4db404465`，745/745。
- Runtime/Editor compile `2026-09-05T23:11:31Z / 23:11:32Z`；23:18:31Z SelfCheck PASS；
  filtered Console 仅 7 条预期 rest-binding 自检日志，无 CS 诊断。
- Scene SHA/长度/mtime 不变；Ledger validator PASS（285 records / 247 governed code files）。

下一 ordinary-defense/reduced-hit 原子生产接线；正式 armor content 仍受 H/B11 门约束。
