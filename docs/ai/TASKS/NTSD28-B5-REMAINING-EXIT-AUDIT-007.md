# Task Contract — NTSD28-B5-REMAINING-EXIT-AUDIT-007

> 状态：`VERIFIED / GOVERNANCE_ONLY / HIT_GROUP_ELIGIBILITY_FROZEN_PAIR_ROUTED`
> 依赖：`NTSD28-B5-SPECIAL-HIT-LATCH-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED`

## 目标

在special-hit latch规则族关闭后，继续对B5 current-authority candidate/consume/hit/armor/damage/combo
production surface做剩余退出审计，识别下一个独立且production-reachable的first difference，或以逐族证据
裁决B5是否可进入最终退出门。

## 边界

- 只读Authority playable closure、Unity production、现有manifest/test与冻结内容统计。
- 本审计不改C#、Scene、Prefab、Config、资源、ProjectSettings或正式Authority目录。
- B6 catch/cpoint/held/weapon、B7 spawn/lifecycle、B8 battle flow/results/combo产品表现、B9/B10表现与音频、H/B11内容继续单独路由。
- 已验证规则族只复核production ownership和残留caller，不重新实现。

## 验收

- 明确列出Authority入口、Unity对应owner、production reachability和first-difference条件。
- 将跨阶段差异与B5可独立实施差异分开；不得把content缺失伪装成runtime默认值。
- 形成下一包唯一方向，并同步Task/Change/Ledger/STATE/handoff/总表。

## 回滚

仅移除本次新增治理记录；没有行为或资产回滚。

## 审计结论

下一独立first difference为native hit-group eligibility与candidate frozen-pair合同：Unity仍用旧same-team
局部分支，state190/group reversal、state180、selected-mode gate、正确type0/type3 opposing-facing极性和
consumer frozen snapshot均未闭合。完整证据见
`docs/ai/MANIFESTS/NTSD28-B5-HIT-GROUP-ELIGIBILITY.md`。

下一执行`NTSD28-B5-HIT-GROUP-ELIGIBILITY-OWNER-AUDIT-001`；本审计没有修改任何脚本、Scene、内容或Authority。
