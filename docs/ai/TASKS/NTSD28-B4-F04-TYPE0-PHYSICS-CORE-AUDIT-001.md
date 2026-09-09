# Task Contract — NTSD28-B4-F04-TYPE0-PHYSICS-CORE-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / TYPE0_CORE_SEAM_DEFINED`

## 目标与结论

在collision-Y carrier就绪后，审计type0 `CharacterMechanics.StepBattleLogic`可独立闭合的
physics core。首差为Unity仍以`YInt>=0`、`Y>0`和`Y<0`判断摩擦/接触/重力；Authority以
`collision_y_reference`选择摩擦平面和negative effective floor，并要求previous precise Y严格在
floor上方、current precise Y到达/穿过floor才产生landing。

## 下一包

`NTSD28-B4-F04-TYPE0-PHYSICS-CORE-001`只修改type0/shared-character mechanics core：
reference-aware friction、effective floor、strict crossing、clamp和airborne gravity。landing action、
environment damage、non-character types、operation30 producer、teleport/next999/input/hit另包。

