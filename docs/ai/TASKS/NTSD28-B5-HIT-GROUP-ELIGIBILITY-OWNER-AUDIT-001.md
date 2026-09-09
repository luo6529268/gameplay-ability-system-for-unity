# Task Contract — NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / FOUR_PACKAGE_SPLIT_DEFINED`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-007 / VERIFIED`

## 目标

冻结native hit-group eligibility的Unity唯一pure owner、三collector candidate-time接线、consumer frozen-pair
读取、candidate store复制边界、world selected-mode carrier和HitPlan责任，产出可独立验证的实施顺序。

## 边界

- 只读，不改脚本、Scene、content、Prefab、ProjectSettings或Authority。
- 不把background视觉cycle纳入；只审计其mode record中会改变战斗group eligibility的`hurtable_18`数值carrier。
- 不重开已验证kind8、effect-type、multi-body、special latch或damage writer规则族。
- B6/B7/B8/B9/B10/H producer与内容保持独立。

## 验收

- 枚举所有生产collector、candidate carrier/store、runtime consumer和HitPlan入口。
- 明确最小frozen snapshot字段、生命周期、复制/容量/zero-allocation与schema义务。
- 明确world mode gate carrier的default/reset/snapshot/checksum/parity/raw与producer边界。
- 给出test-first分包顺序，禁止partial candidate/consumer迁移。

## 结论

- 唯一pure owner、world scalar、完整18-payload+valid pair snapshot、producer/consumer/HitPlan边界已冻结。
- 三条formal collector在`TryRecordReleaseCandidate`汇合，可在同一位置执行snapshot与pre-nearest gate。
- persistent world field需要schema `8→9 / 17→18 / 20→21`；ephemeral candidate snapshot不进入persistent
  schema，但必须进入store shadow与HitPlan identity比较。
- 后续严格按world carrier→pair carrier→pure core→atomic production→exit audit执行。
- 详细清单：`docs/ai/MANIFESTS/NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNERS.md`。

## 回滚

仅移除本治理记录；无行为或资产回滚。
