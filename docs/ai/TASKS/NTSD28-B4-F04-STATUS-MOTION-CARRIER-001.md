# Task Contract — NTSD28-B4-F04-STATUS-MOTION-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / PRODUCER_CONSUMER_DEFERRED`
> 依赖：`NTSD28-B4-F04-STATE12-18-TRANSACTION-PREREQUISITE-AUDIT-001 / VERIFIED`

## 目标

为 NTSD 2.8-Logan 的 7 个 landing/hit-motion status 字段建立 Unity 确定性 carrier：
`StatusDx1C0`、`StatusDy1C4`、`StatusDz1C8`、`StatusGain1CC`、
`StatusHitFacing1D0`、`StatusPickedAction1D4=191`、`StatusPickingAction1D8=185`。

覆盖 full reset、input reset preservation、canonical copy、runtime/full snapshot restore、checksum、
parity snapshot 与 schema version；本包只建立数据合同，不连接 producer 或 consumer。

## 修改范围

- `NTSDEntityRuntime` carrier、默认值、full reset 与 canonical copy。
- entity/full snapshot 与 checksum schema 版本推进。
- checksum 与 parity snapshot 纳入全部 7 个字段。
- focused carrier test 与既有精确 schema 断言同步。
- 治理文档。

## 不变量

- 不实现 state12/18 landing transaction consumer，不接 B5 hit writer producer，不新增 B8 knockout event。
- input-state reset 必须保留 7 个跨 tick carrier；full reuse reset 恢复 authority 默认值。
- `picked=191`、`picking=185` 是正式默认值，不得清为 0。
- 不复用即时速度、Effect、WeaponCount、EnvironmentState320 或 CollisionYReference 作为这些字段。
- 当前 B0 raw entity schema 是 48-leaf exact contract，权威捕获不输出这 7 个字段；本包不单边扩展 raw。raw schema 只有在 authority capture 与 Unity exporter 同步升级时才能另包修改。
- 不修改 Authority、Scene、DAT、资源、Prefab、ProjectSettings 或网络协议。

## 验收

- test-first 红灯证明 carrier 缺失。
- focused test 覆盖默认值、两类 reset、canonical copy、snapshot/restore、checksum、parity 与 4096 次热路径 0 B。
- 相关 carrier/snapshot/checksum tests、全部 `NTSD28*` Editor tests、BattleRuntimeSelfCheck、Console、Scene 与 Ledger 均闭合。

## 回滚

删除 7 个字段与 focused test，恢复 canonical/reset/checksum/parity 写入和 schema version；无数据迁移。

## 验证结论

- test-first compile red：21个`CS1061`，全部为7个缺失carrier。
- fresh compile error0；最终focused `fde4081f5be8412fa57a213b71ad5107` 6/6，逐字段checksum恢复与4096次hot path 0 B通过。
- related `a0638fe62d7441c6b49e842d08c9bf8c` 60/60；精确77-class NTSD28 broad `3e7bd0a900794a46833a1e8dcb515d5e` 527/527。
- 最终SelfCheck `2026-09-05T08:00:57Z` PASS；Scene/Console/Ledger通过。
- 误启动的超范围namespace job `4f2d0b7d668646e3bb9b8b0e82a7c3f9` 执行1600项并因既存版本、GroundRoleNearest、OPoint、structure guard等无关失败终止；未改这些范围外文件，不作为本Change失败。
