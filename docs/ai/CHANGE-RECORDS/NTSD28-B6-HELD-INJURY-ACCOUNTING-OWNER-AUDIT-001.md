# NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::settle_catch_relations, record_native_knockout and post-settlement caughtact producer; EXE B1E13AE1, closure 39DDDA15.
evidence: canonical held accounting, direct credit, cover timers, transient caughtact event boundary and missing Unity KO feed mapped; two production packages defined; full MP resource remains B7/B8/B11/H deferred; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / TWO_PRODUCTION_PACKAGE_SPLIT_DEFINED / FULL_RESOURCE_DEFERRED`

已冻结 held CPoint injury 的 actual writer、canonical carrier、direct-credit、cover timer 和
post-settlement caughtact event/producer owner；本审计未修改脚本、内容或 Scene。

## 结论

- 第一包只修 `BattleCpointWriter` actual 以及 focused test/SelfCheck/Play probe：复用 display
  lead，改读 `IncomingDamageScale340`，按直接 `OwnerSlotIndex`/type-0 self 解析 credit，
  写 HP、HPBound/3、`InputHpConsumedTotal34C`、`InputScoreTotal348`、
  `KnockoutCount358`和 cover-exclusion timers，并保持所有 legacy stat sentinels。
- 第一包不调用完整 MP resource transaction；B7/B8/B11/H 阻塞未变。
- 第二包在 `BattleInteractionPipeline` 的 held settlement 完成后以 transient applied-event
  集合调用已有 native combo producer；不允许 writer 内立即生产。
- `KnockoutCount358` 已有持久化/parity carrier；Authority world knockout event/feed 在 Unity
  仍缺 owner，保持独立后续审计，不以 `KillStats` 代替。
- release 484 条正 held injury 及当前 Unity content 9 条正 held injury 均使本首差
  可达；两个 corpus 均无负 injury。

完整 owner 矩阵和验收顺序见
`docs/ai/TASKS/NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001.md`。

下一 production 候选：`NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001`；由于
throw package 仍缺 Unity runtime 绿灯，本轮先不叠加新代码修改。

## CORPUS CORRECTION（2026-09-08）

current positive kind1 injury纠正为223，cover分布0:205/1:15/11:3。两production owners保留，测试矩阵扩大。
