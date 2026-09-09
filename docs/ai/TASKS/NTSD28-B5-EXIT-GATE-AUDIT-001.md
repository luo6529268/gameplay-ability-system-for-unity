# Task Contract — NTSD28-B5-EXIT-GATE-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / B5_PLACEMENT_EXIT_READY / B5_FULL_CLOSE_DEFERRED`
> 依赖：`NTSD28-B5-NATIVE-COMBO-EXPIRY-001 / VERIFIED`

## 目标与结论

只读复核B5连续first-difference审计、已验证production规则族及Authority hit tail到
`expire_ordinary_combo_entries`返回边界。B5审计001～011已从kind8、candidate effect/type、multi-body、
kind4、weapon durability、special latch、frozen hit-group、first-BDY、system-table、reduced state2000推进到
native combo；carrier、ordinary producer和C25后expiry现均验证闭合。没有再发现可独立留在B5实施的新首差。

因此允许从4.7 production规则迁移转入4.8/B6；状态是`B5_PLACEMENT_EXIT_READY`，不是整个碰撞/命中
系统或全项目完全一致。`C-07`旧“全局终止”表述不准确；当前Authority是成功first-BDY终止该attacker
剩余candidates，外层slot循环继续。

## 明确后置依赖

1. B6：caughtact combo producer、catch settlement、cpoint/held resource caller、relation/horizontal impulse。
2. B7：special child suppression与spawn/lifecycle可见性。
3. B8/H/B11：selected-mode resource override、baseMaxMP、weapon-strength与正式内容tuple/数值。
4. B10：combo图集/数字、hit audio/spark及其他可观察表现。
5. 全阶段出口：同seed/input/tick的C++/Unity joint trace与真实Play矩阵。

## 边界与回滚

本审计不改C#、Scene、Prefab、Config、资源、ProjectSettings或Authority；回滚仅移除治理记录与状态更新。
