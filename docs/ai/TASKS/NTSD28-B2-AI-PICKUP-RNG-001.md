# Task Contract — NTSD28-B2-AI-PICKUP-RNG-001

> 状态：`FOCUSED_TEST_PASS / PICKUP_SITES_16_17_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

在synchronized candidate模式闭合2.8 pickup state `1000/2004`选择门与左右远距site`0x16/0x17`；
legacy CRT继续使用既有`1004/2004`，production cursor仍不启用。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiSensingKernel.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28AiPickupRandomEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff、总表与B2 AI RNG manifest。

禁止修改其他pickup/threat策略、production捕获/提交、DAT/Config/Scene/ProjectSettings/Packages或权威目录。

## 验收

- native candidate sensing选择state1000/2004、不把1004当native pickup；legacy gate保持1004/2004；
- target左/右远距在`0x14`之后只消费`0x16/0x17`；250以内不消费16/17；
- direction-lock短路与nonpositive合同不变；
- focused/related、4096 zero-allocation、SelfCheck、Console0、Ledger通过。

## 回滚

移除native pickup-state参数、恢复两个无site调用并删除新增测试；production未切换。

## 结果

red1；focused6/6；AI sensing/decision/shadow297/297；native1000/2004、legacy1004/2004、
`0x16/0x17`与4096 zero-allocation通过；SelfCheck、Console0、Ledger通过。完整selection与production另包。
