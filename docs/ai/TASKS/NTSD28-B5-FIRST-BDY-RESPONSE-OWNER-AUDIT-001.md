# Task Contract — NTSD28-B5-FIRST-BDY-RESPONSE-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED`
> 依赖：`NTSD28-B5-REMAINING-EXIT-AUDIT-008 / VERIFIED`

## 目标

冻结first-current-BDY 1xxx/2xxx/encoded response所需的最小数据合同、纯决策边界、actual写入时点、
同步RNG事务、HitPlan shadow与per-attacker终止owner，形成可测试、可回滚且不遗漏fallback的实施拆分。

## 边界

- 只读审计；不修改脚本、Scene、Prefab、Config、资源、ProjectSettings或Authority。
- 不扩展`BattleBodyBoxValue`四字段几何合同；响应只读取当前帧第一个BDY，不需要冻结overlap body。
- 不把H/B11 encoded内容迁移、B6 cpoint/held、B8 combo产品表现或B10音频混入。

## 验收

- parser/carrier、pure decision、RNG plan/commit、actual writer、runner abort与HitPlan观察各有唯一owner。
- 明确成功、chance failure、active armor/reduced defense、non-character selected armor与type1 fallback分支。
- 明确源码文件、测试矩阵、zero-allocation和真实Play见证要求。

## 回滚

仅移除本审计治理记录；没有行为或资产回滚。

## 结论

按以下三包顺序实施：

1. `NTSD28-B5-FIRST-BDY-RESPONSE-CARRIER-001`：保留四字段formal geometry；一般化现有first-kind读取并新增
   first-BDY respond parser/carrier及content-focused测试。
2. `NTSD28-B5-FIRST-BDY-RESPONSE-PURE-CORE-001`：纯解码/分支决策，外部传入roll，不直接读实体或消费RNG。
3. `NTSD28-B5-FIRST-BDY-RESPONSE-ATOMIC-PRODUCTION-INTEGRATION-001`：shared runner在consume effects前执行
   plan/commit；actual writer、同步RNG、type1 fallback、HitPlan attempt shadow及per-attacker abort原子接线。

完整owner矩阵见`docs/ai/MANIFESTS/NTSD28-B5-FIRST-BDY-RESPONSE-OWNER-AUDIT.md`。
