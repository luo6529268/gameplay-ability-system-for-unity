# Task Contract — NTSD28-B4-F04-HARD-MOTION-CONSUMER-001

> 状态：`VERIFIED / PURE_KERNEL_READY / PRODUCTION_TRANSACTION_DEFERRED`
> 依赖：`NTSD28-B4-F04-STATUS-MOTION-CARRIER-001 / VERIFIED`

## 目标

实现 `consume_native_hard_landing_motion(...)` 的无状态 Unity kernel，精确消费7-field carrier中的
dx/dy/dz/gain并更新 Vx/Vy/Vz；本包不把kernel接入state12/18 production transaction。

## Authority 合同

- dx为0不改Vx；dx严格`>500`时直接写`dx-550`。
- 其余dx按`StatusHitFacing1D0`把当前Vx转换为命中朝向空间，仅在低于正阈值或高于负阈值时钳制；写回时恢复世界朝向符号。
- dy/dz为0不变；严格`>500`直接写`value-550`，否则在现有速度上累加。
- 调用结束总是清 `StatusDx1C0/StatusDy1C4/StatusDz1C8/StatusGain1CC`。
- `StatusHitFacing1D0`、picked/picking actions保持不变。

## 不变量

- kernel本身不判断`StatusGain1CC`；authority caller负责只在gain==1时调用。
- 不选择action、不改变frame counter、Y/floor、HP、EnvironmentState320、score/KO。
- 不接B5 producer、state12/18 transaction或B8 event，不改snapshot schema。
- 不修改Authority、Scene、DAT、资源、Prefab或ProjectSettings。

## 验收

test-first覆盖dx方向钳制、正负阈值、严格500/501 sentinel、dy/dz add/override、零值、清理与保留项；
compile、focused/related/NTSD28 broad、SelfCheck、Console/Scene/Ledger闭合。

## 回滚

删除pure kernel与focused test；carrier数据合同保持不变。

## 验证结论

- test-first compile red：7个`CS0103`缺少pure kernel。
- fresh compile error0；focused `1a800f4ae1e648c79aed9c433f167f72` 15/15。
- related `85fdd851075843ac8469f5f10d676127` 55/55；精确NTSD28 broad `d2876a6c37bb4fb68c78a05bf2c32584` 542/542。
- SelfCheck `2026-09-05T08:12:40Z` PASS；Scene/Console/Ledger通过。
