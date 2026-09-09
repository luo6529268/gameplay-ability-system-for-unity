# Task Contract — NTSD28-B5-REMAINING-EXIT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / KIND8_FAMILY_ROUTED / B5_EXIT_NOT_READY`
> 依赖：`NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001 / VERIFIED`

## 审计结论

B5尚不能退出。status、type3-specific、standard-rest、ordinary-defense、type1-armor与unarmored
HP-consumption规则族已闭合；完整MP resource、CPoint、audio/spark与content分别合法后置到B7/B8/B9/B10/H。
当前仍可在B5独立实施的首差是kind-8 control relation：candidate与consumer均未按2.8 selector/side-effect合同执行。

## 已确认差异

- Authority candidate只把kind3限制为type0；kind8以`bdefend`解释target type selector（0..6 exact、7为
  1/2/4/6、8 unrestricted），再以`respond` 0..4执行group/owner/mode关系。
- Unity `BruteForceSceneQuery.ItrAllowedCore`把kind8与kind3一起硬限制到type0，且candidate-time没有上述classifier。
- Authority consumer再次防御性classifier；`injury!=0`才写heal timer，`caughtact!=0`加current MP，`dvx!=999`
  才改attacker action；`dvy`决定precise X/Y/Z同步，整数坐标不立即改。
- Unity actual多入口与HitPlan缺selector复核/caughtact/dvy/sentinel/zero gates，并固定写X/Z及整数镜像；
  weapon/special入口还把kind8 target硬限制为Character。

## 实施顺序

1. `NTSD28-B5-KIND8-ELIGIBILITY-PURE-CORE-001`：先冻结allocation-free selector truth table。
2. 审计/补齐actual与HitPlan所需字段写面及唯一production owner。
3. 原子接candidate + consumer actual + HitPlan，不能只放开non-character候选后仍由旧consumer拒绝。

## 边界

本审计不改C#、content或Scene；respond-4使用的battle-mode production binding在接线包中必须显式验证，
不得假定聊天语义。kind1/3 catch与B6保持独立。

