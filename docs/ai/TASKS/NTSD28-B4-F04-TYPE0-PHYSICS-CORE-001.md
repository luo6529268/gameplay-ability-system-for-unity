# Task Contract — NTSD28-B4-F04-TYPE0-PHYSICS-CORE-001

> 状态：`VERIFIED / TYPE0_CORE / ACTIONS_AND_PRODUCERS_PENDING`

## 目标

让production type0/shared-character physics core消费`Runtime.CollisionYReference`：pre-step integer
Y>=reference时摩擦；negative reference作为effective floor；只有previous precise Y<floor且
current precise Y>=floor才landing；landing clamp到floor；airborne仅Y<floor时加重力。

## 范围与不变量

- 修改`CharacterMechanics.StepBattleLogic`与新focused test。
- 默认reference0保持常规结果；X/Z collision flag消费不变。
- 不改landing action、damage、type1/2/3/4/6、producer、teleport、next999、input/hit或Scene。

## 验收

test-first覆盖negative floor grounded friction、negative floor crossing/clamp、above-floor gravity、
already-on-floor positive Vy不重复landing和default-zero回归；再跑相关、broad、SelfCheck。

## 结果

red3/5→focused5、related33、broad470、SelfCheck PASS；只关闭type0 mechanics core。
