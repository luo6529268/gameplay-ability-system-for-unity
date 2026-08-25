# HANDOFF — R6-PRES-06 visual scale / held anchor adapter certification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（no-code）  
> 对应登记：`A-RENDER-002`

## Result

1.5× scale只进入body center-to-pivot和held相对表现补偿；逻辑pixel/world换算、runtime position、碰撞和
移动参数不乘1.5。Central command与Legacy diagnostic共用同一helper。fresh 19:49:12 full self-check覆盖
right/left、invalid/reuse/dormant、central immutable snapshot、central-vs-legacy与实际held样例并PASS。

## Pending

真实Play Mode仍需用户检查实际DAT/atlas下的body、held weapon、shadow相对锚点；无C++ runtime trace。

## Next

进入`R6-PRES-007` fixed-world camera / presentation camera no-code adapter certification。

