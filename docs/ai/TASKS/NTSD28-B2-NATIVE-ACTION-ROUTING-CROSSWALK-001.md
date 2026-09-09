# Task Contract — NTSD28-B2-NATIVE-ACTION-ROUTING-CROSSWALK-001

> 状态：`VERIFIED / SOURCE_CHAIN_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

只读闭合NTSD 2.8 `InputRouter28::step_sampled`在edge/combo10之后的完整动作路由，与Unity当前
combo/direct/action/state resolver逐项交叉；冻结缺字段、状态副作用、RNG和实施依赖。本包不修改脚本，
不把已存在的combo selector误报为production action已对齐。

## 允许修改

- 本Task、对应Change Record、manifest、Ledger、STATE、handoff与总表。
- 禁止修改C#、Config/DAT、Scene/Prefab、ProjectSettings、Packages或authority目录。

## 验收

- 证明authority文件参与正式playable/core build，并列出locked tests入口；
- 闭合combo fields→three button→direction fields→type0 built-ins的严格顺序；
- 列出`apply_action`、direct action和resource policy不可互换的副作用；
- 测量Unity缺失frame字段及当前内容规模；
- 给出可独立test-first、无循环依赖的实施拆分与联合trace要求。

## 结果

验收已满足；完整表与拆分见
`docs/ai/MANIFESTS/NTSD28-B2-NATIVE-ACTION-ROUTING.md`。本包无生产修改，不代表B2动作路由已实现。
