# NTSD28-B5-ARMOR-REDUCED-HIT-AUDIT-001 — armor/reduced hit audit

<!-- CHANGE-RECORD
id: NTSD28-B5-ARMOR-REDUCED-HIT-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28 armor selection/reduced rest live path; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-CALL-CHAIN-CLOSED / FORMAL-ARMOR-18-TYPE1-12-TYPE0-6 / UNITY-ARMOR-0 / IMPLEMENTATION-SPLIT-DEFINED
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED / TYPE1_CONTENT_GATED`

## 审计问题

1. armor record如何由target frame/state/body/interaction选择，优先级和short-circuit是什么。
2. selected armor如何改变伤害、fall/bdefend、动作、runtime armor HP/recovery及rest。
3. `apply_reduced_hit_rest`与standard rest在hold/arest/vrest上的精确差异。
4. Unity `LF2AlternateDamageResolver`、`ApplyAlternateDamage`及armor runtime carrier是否等价。
5. 哪些缺口是算法/载体，哪些因H项内容策略未决只能审计或延后。

本包不改脚本、content或Scene。

## 结论

- formal defense与armor selection并非Unity旧OID 37/6/52启发式；current state、spark/dbdefend/dvx、OID822、type1 record gates共同决定。
- reduced rest的default/packed delay、definition effects、timing reduction和native byte vrest均与Unity alternate不同。
- Unity runtime armor HP/recovery carrier与C25i core已ready，但profile固定false；正式18个block（12 type1/6 type0）在Unity为0。
- type1 production/content受H/B11策略门约束；reduced-rest pure与ordinary defense pure/integration不受该门阻塞。

下一包：`NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001`。完整表见armor manifest。
