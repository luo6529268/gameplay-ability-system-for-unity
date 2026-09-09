# Task Contract — NTSD28-B2-AI-TARGET-PREFIX-RNG-001

> 状态：`FOCUSED_TEST_PASS / SITES_13_14_15_18_19_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

在candidate cursor测试路径中闭合`0x13/0x14/0x15/0x18/0x19`的精确site、条件消费顺序和
Unity legacy三按钮交叉映射，不启用production同步RNG，不触碰尚未闭合的pickup `0x16/0x17`。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28AiTargetPrefixRandomEditorTests.cs`及`.meta`
- 本Task、Change Record、Ledger、STATE、handoff、总表和B2 AI RNG manifest。

禁止修改sensing/pickup、production捕获/提交、其他AI分支、DAT/Config/Scene/ProjectSettings/Packages或权威目录。

## 不变量与验收

- legacy CRT路径结果与tests不变；
- `0x13`在cached active后、definition type0前消费；
- `0x14`在有效selected target后必消费；当前authority source四个force字段无production writer，
  synchronized路径不得用Unity boundary flag代替；
- state3000 subject state7不消费`0x15`；其他状态按nonpositive合同消费并写native defend
  （legacy `KeyAttack`）；
- abnormal左右分别只消费`0x18/0x19`且顺序在`0x14`之后；
- focused/related、zero-allocation、SelfCheck、Console0、Ledger通过。

## 回滚

恢复五个表达式为旧无site调用并移除本包测试；production同步模式本就未启用。

## 结果

red7/7；final7/7；AI185/185；五个site与cached/type、state7、boundary/force顺序已闭合；
4096 zero-allocation、SelfCheck PASS、Console0、Ledger121/68。production与pickup未切换。
