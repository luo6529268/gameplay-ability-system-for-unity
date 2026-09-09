# NTSD28-B5-REMAINING-EXIT-AUDIT-010

<!-- CHANGE-RECORD
id: NTSD28-B5-REMAINING-EXIT-AUDIT-010
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28 confirmed hit tails after system-DAT attacker terminal closure; EXE B1E13AE1, closure 39DDDA15.
evidence: read-only scan confirmed reduced state2000 damping polarity/coordinate mismatch in actual and HitPlan; formal OID150 type2 w/1.dat state2000 kind0 reachability; production RED later corrected the preliminary equal-X wording; no behavior writes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / REDUCED_STATE2000_AWAY_DAMPING_ROUTED / B5_EXIT_NOT_READY`

从`resolve_confirmed_unarmored_hit`的system-table消费者之后继续逐语句复核target-type continuation、effect
action、audio/spark、deferred lifecycle及reduced tail，并反查Unity actual/HitPlan/既有B5 records。当前不
预设结论，不修改任何脚本、内容或Scene。

## 审计结论

- Authority reduced tail只在attacker state2000且“X位于target一侧、Vx已朝远离方向（含零速度）”时，
  用`/2.5`衰减X/Z；X精确相等时不衰减，比较使用逻辑double position。
- Unity actual与HitPlan都以整数X识别`movingToward`并只在toward时衰减，因此明确分侧时极性与
  Authority相反，同整数桶又无法按double X识别away/toward；后续production RED确认equal本身未衰减，
  更正本审计起始时“equal误衰减”的初步表述。
- Authority正式catalog OID150为type2 `w/1.dat`，frame0-6与21均为state2000且带kind0/injury60；
  对defending/type1 armor character的reduced route正式可达。
- 下一包：`NTSD28-B5-REDUCED-STATE2000-AWAY-DAMPING-PRODUCTION-001`。本审计无code/content/Scene写入。
