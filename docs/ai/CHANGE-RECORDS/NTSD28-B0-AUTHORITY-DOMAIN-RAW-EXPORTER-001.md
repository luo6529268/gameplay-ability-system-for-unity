# NTSD28-B0-AUTHORITY-DOMAIN-RAW-EXPORTER-001 — authority B0 domain raw exporter

<!-- CHANGE-RECORD
id: NTSD28-B0-AUTHORITY-DOMAIN-RAW-EXPORTER-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
authority: NTSD 2.8-Logan unchanged playable/core GameSession28, ScenarioLoader28, NativeRandom28 and BattleWorld28 source-model; formal EXE SHA remains identity-only and output remains non-certificate diagnostic evidence.
evidence: CPP17-BUILD-0-WARN-0-ERROR / FORMAL-EXE-SHA-REVERIFIED / AUTHORITY-SOURCE-MANIFEST-C59BD8D3264B5CBF15EDBCFE2BAE64BC0F3BBC41926BEF6A4723EC2F571F2D75 / RUNNER-SHA-05E352F58389856E8C5227EA53DBE634A6B529E3AAA6DC531E84870B5A9B09DC / BINARY-SHA-B928F2D813BB281D2EB04DAF1C1A195574ECCB1351915A7CD7F5C98C08908657 / REAL-INPUT-MASKS-17-2-1-96-0-12 / AUTHORITY-DOMAIN-RAW-VALID-3-TICKS / ENTITY-RAW-VALID-3-TICKS-6-ENTITIES / DETERMINISTIC-RERUN-BOTH-SHAS / LEGACY-OPTIONAL-MODE-VALID / DOTNET-BUILD-0-0 / DOMAIN-12-12 / EXISTING-21-21 / RAW-5-5 / FORMAT-PASS / GLOBAL-LEDGER-89-RECORDS-16-FILES-PASS / SCOPED-DIFF-CHECK-PASS / AUTHORITY-DIRECTORY-READ-ONLY / PRODUCTION-UNITY-UNCHANGED / CERTIFICATE-FALSE
-->

> 状态：`FOCUSED_TEST_PASS / CPP-BUILD-0-0 / REAL-DOMAIN-VALID / AUTHORITY-READ-ONLY`

本包只扩展仓库内诊断runner与场景，不修改权威源码、Unity runtime、DAT、Scene或资源。

## 实施前事实

- `GameSession28::world()->random().state()`给出CRT state/calls与synchronized state；
  `synchronized_table_hash()`给出source-native 64-bit hash。
- Scenario input是tick内完整held状态；现有runner每tick先`set_input`再`step`，因此可在同一处记录
  applied-to-tick mask，不需要读取post-input history/cooldown。
- initial world在首step前已具备combatant occupant，必须作为delta基准而不是把初始角色误报为tick1 birth。

## 验收结果

- source manifest `C59BD...F2D75`保持；runner `05E352...B09DC`，binary `B928F2...08657`；
  正式EXE SHA重验，C++ build 0/0。
- scenario SHA `CF3D4D...58542`；真实mask `(17,2)/(1,96)/(0,12)`。
- initial CRT/sync calls `3000/1`，tick delta全0，tableHash64 `A1BA1B90EA55796D`；initial
  slot0/1 epoch均1，三tick无伪birth/death/reuse。
- entity raw `609D39...56C32`=valid 3/6；domain raw `A3C337...C39A6`=valid 3 ticks；重跑SHA相同。
- 不传`--domain-output`的旧neutral capture仍valid；.NET 0/0、12/12、21/21、5/5、format通过。
- Ledger89/16与diff通过；authority只读，Unity未改，certificate false。
