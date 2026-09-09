# Task Contract — NTSD28-B4-F04-IDENTITY-X-EXTRAS-001

> 状态：`VERIFIED / INDEPENDENT_IDENTITY_EXTRAS / FLOOR_LANDING_PENDING`

## 目标

建立单一pure predicate/kernel，使type4或real/alias120执行`+vx*0.2`，real/alias101独立执行
`-vx*0.2`；type4+101两项抵消。接入derived `LF2Weapon.WeaponFlightPhysics`，shared path语义保持。

## 不变量

- extra在directional base integration是否blocked之外独立；本包只决定额外位移值。
- 不改gravity、Z hit_j、floor/landing、sound、DAT/Scene/Authority。

## 结果

red3/6→focused6、related44、broad476、SelfCheck PASS；shared/derived使用同一kernel。
