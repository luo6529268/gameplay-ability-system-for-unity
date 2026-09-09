# Task Contract — NTSD28-B4-F04-STATE12-18-TRANSACTION-PREREQUISITE-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CARRIER_AND_OWNER_SPLIT_DEFINED`

## 目标

只读闭合state12/18 contact transaction所需的status motion、environment damage、credit/KO与
world lookup/event边界，确认Unity已有carrier与缺口。

## 结论

- 缺失7个正式持久字段：status dx1c0、dy1c4、dz1c8、gain1cc、hit-facing1d0、picked-action1d4（默认191）、picking-action1d8（默认185）。Unity无同语义carrier，不能用Effect/即时速度猜代。
- 现有carrier可复用：EnvironmentState320、IncomingDamageScale340、InputHpConsumedTotal34C、InputScoreTotal348、KnockoutCount358、CatchSourceSlot90、EnvironmentSourceSlot160与OwnerSlotIndex；Health.HPBound映射effective max HP。
- Unity没有正式knockout event列表/sequence sink；本阶段可闭合count/score/HP状态，但session knockout feed的可观察事件应回链B8，不能伪称完整结果流。
- hard landing consumer需精确实现dx/dy/dz sentinel/方向clamp并在消费后清dx/dy/dz/gain；soft landing保留gain/status供以后hard landing。
- producer同时来自通用encoded status与qualifying hit-motion arm，属于B5 hit writer；carrier可先建，consumer可用注入测试闭合，但production end-to-end必须等B5 producer。
- mechanics result还需contact-side state12/18 resolution标志；already-grounded state12/18也会进入transaction，不能复用strict `Landed`。

## 实施顺序

1. 7-field deterministic carrier（reset/copy/snapshot/checksum/parity/raw）。
2. pure hard-motion consumer kernel。
3. state12/18 contact/environment/count-score transaction single owner。
4. B5 producer接线与B8 knockout feed事件闭包。

## 不变量

不在consumer包猜写producer；不把现有WeaponCount当EnvironmentState320；不改Authority/Scene/内容。
