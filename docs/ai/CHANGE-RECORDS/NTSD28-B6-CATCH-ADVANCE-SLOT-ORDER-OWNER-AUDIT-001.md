# NTSD28-B6-CATCH-ADVANCE-SLOT-ORDER-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CATCH-ADVANCE-SLOT-ORDER-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::advance_catch_relations single ascending mixed kind1/kind2 pass and separate settle_catch_relations pass; Unity BattleInteractionPipeline three sweeps and BattleCpointWriter exact/compat consumers; EXE B1E13AE1, closure 39DDDA15.
evidence: two-sweep Unity order proven non-equivalent for low-target/high-catcher mutation; exact CatchSourceSlot90 consumer migration frozen; current throw vaction 180/181 has zero kind2 cross-match; release has two action343 throw rows with vaction132/next344 and 134 type0 definitions exposing action132 kind2; low/high slot acceptance matrix and exact-consumer mixed-pass production package defined; runtime witness pending; production held; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / SINGLE_ASCENDING_MIXED_PASS_REQUIRED / EXACT_CONSUMER_PACKAGE_DEFINED / PRODUCTION_HELD`

> 补充验收：`NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001` 已将 mismatch/negative-release
> terminal `continue`、`AttackingCounter` carrier 与旧 fallback-tail 断言纠正纳入同一后续 production 包。

Authority advance是一个按slot升序、每slot当场二选一执行kind1或current-kind2的混合pass；Unity当前先跑
全体kind1、再跑全体kind2，因而错误地把所有caught都放在所有catcher之后。release存在2条
action343 `throwvx=5/vaction132/next344`，且134个type0 definition的action132为kind2，足以建立低/高slot
方向性fixture；当前Unity vaction180/181无kind2交叉匹配，所以当前内容暂不触发。

后续包还必须把advance与settlement从compat `CatcherSlotIndex`迁到exact `CatchSourceSlot90`，但保持
settlement为advance后的独立pass。现有runtime栈未清，真实Hinata/Neji输入路径仍待Play；本轮无脚本、
content或Scene修改。

## CORPUS CORRECTION（2026-09-08）

current throwvx rows=58、type0 definitions=42；全量vaction×target-kind2联结仍为0，故current无mixed-order
witness结论保留。旧2/29基数失效，以multiline总correction为准。
