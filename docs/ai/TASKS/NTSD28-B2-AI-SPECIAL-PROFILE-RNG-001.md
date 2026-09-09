# Task Contract — NTSD28-B2-AI-SPECIAL-PROFILE-RNG-001

> 状态：`FOCUSED_TEST_PASS / SITES_3C_6C_READY / SURPLUS_65_ISOLATED / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

为synchronized candidate闭合ordinary入口`0x3C`与默认`use_ai=0`下oid33的`0x6C`，并确保
同步模式不进入旧66-expression character-profile树；legacy CRT树保持不变。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28AiSpecialProfileRandomEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff、总表与B2 AI RNG manifest。

禁止添加/修改DAT/Config `use_ai`，禁止启用production cursor，禁止改held/ordinary tail或legacy profile树。

## Authority/内容边界

- `0x3C`在ordinary movement前必消费；结果>0只结束special-profile helper，ordinary继续。
- 结果0且`native_use_ai_match(33)`才消费`0x6C`；当前Unity正式Config中`use_ai:`为0处，
  按Direction B缺省0，仅objectId33匹配；未来内容策略若引入alias再独立增加carrier。
- `0x6C==0`或target state8/16，且predicted abs X<60、dz<7、PP>150、朝向目标时置combo index2。
  index2按router为`hit_Ua`/legacy `ComboDua`，不采信authority注释中的`hit_aj`误标。

## 验收

- direct helper覆盖3C正值、3C零值非oid33、oid33 6C门和combo；
- full candidate早退trace严格为`0x14,0x3C,0x6C`，无旧profile surplus；
- 4096 zero-allocation、legacy AI全组、SelfCheck、Console0、Ledger通过。

## 回滚

移除native special-profile helper和同步分支，恢复旧profile gate；production未连接。

## 结果

red5；focused6/6；AI303/303；`0x3C/0x6C`、combo index2与65个surplus隔离、4096
zero-allocation通过；SelfCheck、Console0、Ledger124/72。production与ordinary tail未接。
