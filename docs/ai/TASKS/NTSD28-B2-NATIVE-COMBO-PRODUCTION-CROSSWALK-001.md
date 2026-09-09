# Task Contract — NTSD28-B2-NATIVE-COMBO-PRODUCTION-CROSSWALK-001

> 状态：`VERIFIED / CROSSWALK_CLOSED / GOVERNANCE_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

只读闭合2.8 native sample/proxy/edge/combo/action顺序与Unity producer/router/parser现状，明确production
迁移边界并拆包，防止把现有legacy projection误写成native combo已接入。

## 允许修改

- 本Task、对应Change Record、Ledger、STATE、handoff和总表。
- 禁止修改C#、DAT、Config、Scene、ProjectSettings、Packages或权威目录。

## 验收

- 明确第一遍与第二遍各自职责；
- 闭合combo10 route priority、attempt/clear和特殊分支；
- 列出human/AI提前edge/combo的Unity写入点；
- 测量缺失frame字段的权威/Unity内容规模；
- 给出不重叠、可独立验证的实施拆分。

## 结果

验收全部满足。结论与拆分见Change Record；本包无构建/测试要求，也不表示任何production差异已关闭。
