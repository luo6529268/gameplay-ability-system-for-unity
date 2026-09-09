# NTSD28-B0-BASELINE-EXIT-AUDIT-001 — B0 baseline exit audit

<!-- CHANGE-RECORD
id: NTSD28-B0-BASELINE-EXIT-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD28-UNITY-BATTLE-REALIGNMENT-001 B0 definition plus all accepted B0 raw/validator/comparator evidence through NTSD28-B0-DOMAIN-FIRST-DIFFERENCE-001.
evidence: B0-BASELINE-READY / B1-READY / ENTITY-MATURITY-38-0-9 / REAL-INPUT-EQUAL / SLOT-CAPACITY-EXCEPTION-APPLIED / REAL-SLOT-OCCUPANT-EPOCH-LIFECYCLE-SHARED-DOMAINS-EQUAL / RNG-TOPOLOGY-DIFFERENCE-ROUTED-B2 / MISSING-FIELDS-ROUTED-B4-B5-B7 / BASEMAXMP-ROUTED-B11 / FORMAL-EXE-CERTIFICATE-ROUTED-B12 / NO-SOURCE-CHANGE / NO-AUTHORITY-WRITE
-->

> 状态：`VERIFIED / B0-BASELINE-READY / B1-READY / GOVERNANCE-ONLY`

B0工具/证据基线已闭合；后续差异保留并按阶段路由，不宣称行为parity。

## 审计结果

- entity neutral baseline 38/0/9，10 differences/37 equal/60 occurrences；input场景20/27/75。
- domain real compare：input/occupants/lifecycle equal，capacity例外，首差RNG topology。
- B0工具：comparator6/6、domain12/12、trace21/21、raw5/5；Unity compile0与joint9/9；
  C++/dotnet build0/0；Ledger证据通过。
- B1可开始；B2/B4/B5/B7/B11/B12未完成，正式EXE certificate仍false。

