# Task Contract — NTSD28-B5-REMAINING-EXIT-AUDIT-008

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIRST_BDY_RESPONSE_ROUTED`
> 依赖：`NTSD28-B5-HIT-GROUP-ELIGIBILITY-EXIT-AUDIT-001 / VERIFIED`

## 目标

在hit-group eligibility规则族关闭后，继续审计B5 current-authority的candidate/consume/hit/armor/
damage/combo production surface，选出下一个独立且production-reachable的first difference，或以逐族证据
裁决B5是否可进入最终退出门。

## 边界

- 只读Authority playable closure、Unity production、现有测试及两端DAT inventory。
- 本审计不改C#、Scene、Prefab、Config、资源、ProjectSettings或正式Authority目录。
- B6 catch/cpoint/held/weapon、B7 spawn/lifecycle、B8 battle flow、B9/B10表现与音频、H/B11内容继续单独路由。
- 不重开已经验证的hit-group、special-hit latch、kind4、kind8、weapon durability等规则族。

## 验收

- 冻结Authority入口、先后顺序、边界值、RNG消费、写副作用和per-attacker终止语义。
- 明确Unity当前缺失的data carrier、actual writer、shared runner abort和HitPlan projection。
- 证明差异可由正式内容触发，且不把内容迁移策略与B5 runtime实现混为一包。
- 形成下一包唯一方向并同步Task/Change/Ledger/STATE/handoff/总表。

## 回滚

仅移除本次新增治理记录；没有行为或资产回滚。

## 审计结论

下一独立first difference为`FIRST_BDY_ACTION_AND_ENCODED_RESPONSE`：Authority在普通未减伤尾部之前读取
目标当前帧第一个BDY；1xxx/2xxx直接改动作/阵营，编码BDY还可消耗同步RNG、改双方动作、施加停顿或
手工伤害。成功响应跳过普通伤害/连击并终止当前攻击者剩余candidate。Unity只保留第一个BDY的kind
用于另一条effect suppression，丢弃BDY `respond`，且actual/shared runner/HitPlan均无响应分支。

下一执行`NTSD28-B5-FIRST-BDY-RESPONSE-OWNER-AUDIT-001`；完整证据见
`docs/ai/MANIFESTS/NTSD28-B5-FIRST-BDY-RESPONSE.md`。本审计没有修改任何脚本、Scene、内容或Authority。
