# Task Contract — NTSD28-B2-AI-RNG-CALLSITE-CROSSWALK-001

> 状态：`VERIFIED / LIVE_CLOSURE_CLOSED / GOVERNANCE_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

只读闭合 NTSD 2.8-Logan `NativeAi28::step_main` 正式可达调用链中的 synchronized RNG
调用表达式、call-site ID、条件消费顺序与 Unity canonical AI 对应点，形成后续 production
consumer migration 的唯一清单。

## 允许修改

- 本 Task、对应 Change Record、Ledger、STATE、handoff、总表；
- 必要时新增 `docs/ai/MANIFESTS/` 下的只读审计清单；
- 禁止修改 C#、DAT、Config、Scene、ProjectSettings、Packages 或权威目录。

## 验收

- 区分全生产源码静态库存与 `step_main` live closure；
- 列出 live expression、全部可能 ID、bound、短路/条件消费顺序和 authority symbol；
- 对应到 Unity canonical kernel 的精确 symbol/line，区分 exact、semantic mismatch、missing、legacy-only；
- 明确 unused helper、non-AI first-pass、custom-profile/locked-corpus 边界；
- 拆出可独立测试、不会让 shadow 重复提交 RNG 的 production 实施包。

## 回滚

本包无生产代码或资源改动；回滚仅移除本次新增的治理条目和审计清单。

## 结果

验收全部满足：authority live closure为38 expressions/40 possible IDs；Unity canonical为107
expressions，其中69个没有live ID；逐项表与短路顺序已冻结。production尚未修改，下一包从
call-site-aware stream seam开始。
