# NTSD28-B0-DOMAIN-FIRST-DIFFERENCE-001 — B0 domain first-difference

<!-- CHANGE-RECORD
id: NTSD28-B0-DOMAIN-FIRST-DIFFERENCE-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/B0DomainRawComparator.cs
code-path: Tools/NTSD28Parity/B0DomainRawComparatorSelfTest.cs
code-path: Tools/NTSD28Parity/Program.cs
authority: Validated NTSD 2.8-Logan authority-source-model and Unity diagnostic B0 domain raw captures; user-approved Slot capacity model exception applies only to capacity numeric equality, not slot occupant/lifetime semantics.
evidence: RELEASE-BUILD-0-WARN-0-ERROR / COMPARATOR-SELFTEST-6-OF-6 / EXISTING-DOMAIN-12-12 / TRACE-21-21 / RAW-5-5 / FORMAT-PASS / REAL-3-TICKS-INPUT-EQUAL / REAL-SLOT-OCCUPANTS-EQUAL / SLOT-CAPACITY-1000-400-EXCEPTION-APPLIED / REAL-LIFECYCLE-EQUAL / REAL-RNG-STREAM-TOPOLOGY-DIFFERENCE / AUTHORITY-CALLS-CRT-0-0-0-SYNC-0-0-0 / UNITY-CALLS-1-2-1 / REPORT-SHA-B0AED0EA81D3A6A1AA844BAEC499FD14AB4597C24F09F44EAF7A9D7795BCF185 / GLOBAL-LEDGER-91-RECORDS-18-FILES-PASS / SCOPED-DIFF-CHECK-PASS / RNG-CROSS-STREAM-MAPPING-NOT-AUTHORIZED / PRODUCTION-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / COMPARATOR-6-OF-6 / REAL-FIRST-DIFFERENCE`

本包只实现domain comparator/self-test/CLI，不修改任一runtime/exporter。

## 验收结果

- build0/0；comparator6/6、domain12/12、trace21/21、raw5/5、format通过。
- 真实3 ticks：input/occupants/lifecycle equal；capacity1000/400 exception applied；RNG topology unequal。
- first difference `rng.streamAvailability / STREAM_TOPOLOGY_DIFFERENCE`；authority CRT+sync calls均
  0/0/0，Unity deterministic 1/2/1；report SHA `B0AED0...CF185`。
- Ledger91/18与diff通过；production未改，RNG映射未授权。
