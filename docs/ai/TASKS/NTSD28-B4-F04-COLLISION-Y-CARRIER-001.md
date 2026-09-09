# Task Contract — NTSD28-B4-F04-COLLISION-Y-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / PRODUCERS_UNCONNECTED`
> 依赖：`NTSD28-B4-F04-COLLISION-Y-CARRIER-AUDIT-001 / VERIFIED`

## 目标

新增 `NTSDEntityRuntime.CollisionYReference` signed int carrier，并覆盖default/input/full reset、
canonical copy、runtime/full snapshot restore、checksum、parity snapshot与B0 raw projection；将
`combat.collisionYReference`从missing/null改为已验证绑定。

## 修改范围

- runtime carrier及确定性snapshot/checksum/parity链
- Unity raw exporter与NTSD28Parity field contract
- 新focused carrier/trace tests和必要schema version更新
- 治理文档

## 不变量

- 本包不连接operation30、linked platform、defusion、physics、teleport、next999、input或hit writer/reader。
- 默认值0保证现有零地面行为不变；非零只由test/snapshot显式注入。
- `EnvironmentState320`、Y/YInt、stage boundary与render offset保持独立。
- 不修改Scene、DAT/资源、Authority或网络协议。

## 验收

test-first覆盖default/reset/copy、snapshot/restore/checksum/parity、raw非零投影与contract maturity；
再跑tool selftests、carrier/快照相关、NTSD28 broad、SelfCheck、Console/Scene/Ledger。

## 结果

红灯14→focused6、联合29、最终broad465，tool trace21/raw5与SelfCheck均PASS。
carrier确定性闭包和raw binding完成；任何生产者或行为消费者仍未连接。
