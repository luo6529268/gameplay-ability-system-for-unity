# Task Contract — NTSD28-B5-SPECIAL-HIT-LATCH-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ATOMIC_INTEGRATION_READY`
> 依赖：`NTSD28-B5-SPECIAL-HIT-LATCH-CARRIER-001 / VERIFIED`

## 目标

冻结 Authority `special_hit_latch_0eb` 的四个true producer、两个pre-writer consumer gate与Unity
唯一production改写面，为下一test-first原子接线包提供可恢复合同。本包只读，不修改脚本、Scene、内容或正式权威。

## 结论

- Authority两个kind9 producer与kind0 type3 locked/generic两个producer均写目标实体latch=true；无运行时false writer。
- Authority ordinary eligibility与special relation dispatch都在ITR替换/任何writer前检查“attacker latch && target type0”。
- Unity actual恰有四条对应type3 `HitConfirm2=1`写面；shared candidate runner的一条pre-writer gate同时覆盖两类dispatch。
- Unity HitPlan有五处target-type3 projection写面：locked/generic、kind9、两条D1 identity投影；它们共享actual的四个producer语义。
- HitPlan需新增`TargetSpecialHitLatch0EB`初值、producer投影和difference mask；type3投影不得继续写`TargetHitConfirm2`。
- ordinary weapon/object writer及其`TargetHitConfirm2`投影保持legacy临时语义，两个既有clear边界也保持不变。

## 下一原子包

`NTSD28-B5-SPECIAL-HIT-LATCH-ATOMIC-PRODUCTION-INTEGRATION-001`必须同时：

1. 四条actual type3 producer改写`Runtime.SpecialHitLatch0EB=true`，不再写旧`HitConfirm2`。
2. shared runner改读新carrier；true+type0 whole-attacker abort，non-type0继续，旧`HitConfirm2`不能触发新门。
3. HitPlan新增独立bool snapshot/projection/diff，并迁移五处type3 projection。
4. 验证C25与candidate clear只清旧字段、新latch跨tick保留；full reset/reuse仍清新字段。
5. 迁移现有type3/consumer/SelfCheck/Play probe断言；普通weapon `HitConfirm2`行为不得改变。

## 排除

- B6 catch/cpoint、B7其他lifecycle、B8产品规则、B10 audio/spark、H/B11内容；
- 删除或永久化旧`HitConfirm2`；
- 正式权威目录写入。
