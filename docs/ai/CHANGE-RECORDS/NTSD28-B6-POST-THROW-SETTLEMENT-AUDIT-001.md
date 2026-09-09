# NTSD28-B6-POST-THROW-SETTLEMENT-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-POST-THROW-SETTLEMENT-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28 catch settlement and post-catch driver tail; EXE B1E13AE1, closure 39DDDA15.
evidence: authority settlement/accounting/cover/position/caughtact order compared with Unity writer/pipeline; release corpus measured at 1709 kind-1 records, 484 positive injury and no negative injury, with cover 0/1/10/11 = 407/58/8/11; frozen Unity corpus has 9 positive injury and no negative injury; canonical held-injury accounting selected as next owner audit; no new behavior/content/Scene write.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / ACCOUNTING_FACTS_RETAINED / ROUTING_CORRECTED_BY_NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001`

> 纠正：held-injury 计数与 accounting owner 仍有效，但本记录选择的执行顺序已被后继 control-flow audit
> 修正；post-vaction frame/kind-2 preflight 在 injury 前发生，且有 134 个 release 静态 target witness。

继续只读定位 catch settlement 下一首差；完整 MP resource 保持 B7/B8/B11/H
后置，本审计没有修改代码、内容或 Scene。

## 已观察事实

- Authority `settle_catch_relations()` 在每个有效 kind-1/kind-2 关系上先执行
  vaction/hurtable，再于 `injury != 0 && catcher.frame_counter == 0` 时调用同一
  native resource helper；正 injury 然后按 caught `incoming_damage_scale_340` 缩放伤害。
- 正 injury 的正式写入是：受害者 `current_hp -= damage`、
  `effective_max_hp -= damage / 3`、`input_hp_consumed_total += damage`；credit 仅从
  catcher 的直接 `owner_slot` 解析，missing owner 且 catcher type-0 时回退 catcher self，
  并写 `input_score_total_348 += damage`。这不是 resource attacker 的两跳 owner 规则。
- lethal wrapper 要求 caught HP 原值为正、伤害达到 HP，且
  `ordinary_credit_gate_2f4 == -1`；命中时增加同一 credit 的
  `knockout_count_358`。Unity 当前反而以 `KillCount == -1`为 gate，通过
  `HolderCopySlot` 归属并写 `KillStat/world.KillStats`。
- Authority 设 catcher frame counter 为 1；`cover != 3` 时，只在 `cover != 1`
  时设 catcher hold timer=2，只在 `cover != 2` 时设 caught hold timer=-3。Unity 当前
  无条件写两个 timer。
- 直接解析 Authority decoded DAT 得到 1709 条 kind-1 CPoint：484 条正 injury、
  1225 条零 injury、负 injury 为 0。这484条的 cover 分布为
  `0:407 / 1:58 / 10:8 / 11:11`；因此 canonical accounting 全部可达，cover=1
  的单边 timer 差异可达 58 条。当前 Direction-B Unity Config 独立扫描为
  17 条 kind-1、9 条正 injury、负 injury 为0，9条 cover 均为0。
- Unity `ApplyHeldInjury()` 还误写 `ComboCountVic/ComboCountAtk/world.DamageStats`，并漏写
  `InputHpConsumedTotal34C/InputScoreTotal348/KnockoutCount358`。这些是正式状态差异，
  不是 B10 HUD 差异。
- 位置同步发生在伤害后。Authority 额外支持 cpoint `z`，但 release 与当前
  Unity content 的 kind-1 记录都没有显式 `z`；所以该 schema/位置缺口不是当前
  corpus 的下一首差。`catch_anchor_x_58/y_60` 在 settlement 尾写入，但当前
  playable source 中该尾写值没有后续消费者；保留为 carrier/trace 待核对项。
- `hold_injury_events` 在整个 settlement 完成后才驱动 caughtact combo producer；Unity
  当前没有等价事件聚合或 post-settlement producer。该差异依赖真实正 held-injury
  事件，必须在 canonical accounting 之后单独实施。

## 结论与下一包

`HELD_INJURY_ACCOUNTING_ROUTED`。下一包为
`NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / TWO_PRODUCTION_PACKAGE_SPLIT_DEFINED`；
先闭合可独立精确的 display lead、缩放、canonical HP/score/KO accounting 与 cover timer，
再单独连接 post-settlement caughtact event/producer。完整 MP resource 仍保持后置。

## CORPUS CORRECTION（2026-09-08）

current计数改为throwvx58、positive throwinjury48、positive held injury223；OID417另有40个current
post-vaction invalid pairs。accounting事实与路由保留，以multiline总correction为准。
