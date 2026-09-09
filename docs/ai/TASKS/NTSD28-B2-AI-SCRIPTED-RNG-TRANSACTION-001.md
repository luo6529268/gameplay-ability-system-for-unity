# Task Contract — NTSD28-B2-AI-SCRIPTED-RNG-TRANSACTION-001

> 状态：`FOCUSED_TEST_PASS / SCRIPTED_TRANSACTION_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

在不启用 production synchronized RNG 的前提下，闭合 AI scripted-coordinate 路径的`0x11/0x12`
显式调用点和候选cursor事务：snapshot持有副本、kernel推进副本、witness返回候选、owner仅显式提交。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiDecisionSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28AiScriptedRandomTransactionEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff、总表。

禁止修改production snapshot捕获/writer、其他AI分支、DAT/Config/Scene/ProjectSettings/Packages或权威目录。

## 不变量与验收

- legacy snapshot默认继续走CRT，所有旧AI tests不变；
- synchronized snapshot只有scripted path可安全执行，左右远距分别只消费`0x11/0x12`；
- near/nonpositive路径不消费；snapshot copy/reset正确携带/清空candidate；
- evaluation不改owner，显式commit一次生效，reset后的stale witness拒绝；
- call-site trace与5000/4096 seam兼容，warm evaluation零分配；
- test-first红灯、focused/related、SelfCheck、Console0、Ledger通过。

## 回滚

移除snapshot/witness cursor carrier、scripted显式site与新增测试；production本就未连接，无内容或场景回滚。

## 结果

red19；focused6/6；相关AI178/178；`0x11/0x12`、candidate copy/publish/commit/stale与4096
zero-allocation通过；SelfCheck PASS、Console0、Ledger120/67。production capture/commit未接，
不表示真实AI已切换同步RNG。
