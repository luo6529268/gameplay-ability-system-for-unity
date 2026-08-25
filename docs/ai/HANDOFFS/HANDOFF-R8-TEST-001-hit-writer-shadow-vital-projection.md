# HANDOFF — R8-TEST-001 hit writer shadow vital projection

> 日期：2026-08-23
> 状态：`VERIFIED / DIAGNOSTIC-ONLY`
> Change ID：`R8-TEST-001`

## Current

R8-WP01A full EditMode在1357项后FAILED；fresh exact 2/2复现。统一mask解码为HP/HPBound(or PP)/
ComboCountVic。production R4-HIT-001/003已写这些字段，ShadowCompare projection未同步。

## Implemented

只修改了`BattleEcsHitExecutionPlan.cs`的normal weapon与type3 kind0 projection；production writer/tests不变。
fresh compile clean、converted exact 2/2 PASS；class 178项仅余dead-air HP旧断言。Record已修订，授权把
该一条期望从0改为-10；其余tests与production不变。随后重跑dead-air→class→full→self-check→validator。

最终dead-air 1/1、class 178/178、full 1357/1357、同域与fresh self-check均PASS；本diagnostic/test合同关闭。
R4-HIT真实Play Mode/C++ trace仍按R8/R1-WP02边界独立待验。

## Persistent boundaries

R1-WP02 full trace仍BLOCKED；T8与Android排除；C++ authority只读；R8 Play Mode/Player尚未开始。
