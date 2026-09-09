# Task Contract — NTSD28-B4-F04-TYPE1-LANDING-001

> 状态：`VERIFIED / TYPE1_THRESHOLD_BRANCH / FULL_PHYSICS_PENDING`
> 依赖：`NTSD28-B4-F04-PHYSICS-OWNER-AUDIT-001 / VERIFIED`

## 目标

将 production `ApplyCurrentDatNonCharacterLanding` 的 type 1/state 1002 落地分支改为当前
NTSD 2.8-Logan 正式阈值：只有 downward motion 严格大于
`5.2571022450498032e120` 才 action 7 / Vy=-8 / flip；正常有限冲击统一 action 70 / Vy=0。

## 修改范围

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- 新 focused Editor test
- 治理文档

## 不变量

- type 1 非 state1002 仍 action60；invalid contact与非正Y motion不变。
- weapon HP、Vx*0.5、frame counter/attacking、facing与sound副作用保持 Authority 分支语义。
- 不引入 collision-Y reference，不改 type0/2/3/4/6、Audio sink、资源或 Scene。
- 本包只能报告“type1 threshold branch verified”，不能报告完整 type1/physics/B4 已对齐。

## 验收

先取得旧生产分支红灯；再验证纯 predicate 的 state/strict-boundary 矩阵和 production
正常/极大/非state1002三组写入，随后运行相关物理测试、NTSD28 broad、SelfCheck、Console、
Scene baseline 和 Change Ledger validator。

## 结果

红灯1/3后，focused 4/4、physics/placement/allocation相关33/33、最终NTSD28 broad
459/459与BattleRuntimeSelfCheck均PASS；Scene/Console/Ledger保持干净。只关闭type1
state1002 threshold branch，完整F04继续推进。
