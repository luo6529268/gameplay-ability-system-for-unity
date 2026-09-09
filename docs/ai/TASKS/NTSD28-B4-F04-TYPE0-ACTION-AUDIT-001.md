# Task Contract — NTSD28-B4-F04-TYPE0-ACTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / TYPE0_SPLIT_DEFINED`

## 目标

只读闭合正式type0 airborne action、ordinary landing、state12/18 landing与environment damage，
对照exact `LF2Character` 和 transformed/shared character 两套Unity owner并定义最小实施顺序。

## 结论

- Authority ordinary landing仅在strict crossing且state非12/18时触发，先clamp effective floor、Vy=0、post-friction Vx/3，再按state100→94、current action212或state6→215、`hit_g`、219的优先级选action并reset counter。
- Unity exact/shared两套普通landing大体有94/215/219，但都遗漏刚补齐的`hit_g`，且landing handler再次把Y写0，覆盖negative collision reference。
- Authority state12 airborne action读取post-gravity Vy；negative environment的180族override读取upcoming native phase12。Unity用absolute Y与`tickIndex`推相位，未消费`NativeResourcePhase12` owner。
- Authority state12/18在contact side进入专用transaction，不要求strict crossing；Unity只在`Landed` edge调用handler。
- Unity把`WeaponCount`当摔落伤害源并清零；Authority消费`EnvironmentState320`，按`IncomingDamageScale340`缩放，同时写HP/HPBound、InputHpConsumed、credit score、KO，并把environment state写1。现有多数carrier已存在，但status dx/dy/dz/gain/facing/picked/picking action的正式carrier尚未建立。
- exact `LF2CharacterDamageStateResolver` 与shared `LF2Entity` duplicate landing owner可能产生漂移；不得在一个大包内同时重写普通落地、airborne selection、environment credit和hit-motion transaction。

## 实施顺序

1. type0 result/ordinary landing single body（使用现有hit_g，修negative reference，exact/shared共用）。
2. state12/18 airborne selector与upcoming world phase seam。
3. state12/18 status carrier/credit owner专项审计。
4. environment damage + soft/hard landing transaction。
5. legacy duplicate/owner retirement与B10 Audio/B9 effect分流。

## 不变量

不修改Authority、Scene、DAT、资源；environment credit/KO与缺失status carrier未闭合前不得伪造。
