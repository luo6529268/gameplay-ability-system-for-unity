# NTSD28-B0-ALLOCATION-EPOCH-NORMALIZATION-001 — per-slot allocation epoch normalization

<!-- CHANGE-RECORD
id: NTSD28-B0-ALLOCATION-EPOCH-NORMALIZATION-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
authority: NTSD28-B0-ENTITY-FIELD-SCHEMA-001 allocationEpoch contract; Authority BattleWorld28 presentation_generation producer; Unity RuntimeSlotTable AllocationEpoch semantics; user-approved Unity slot capacity model.
evidence: CPP-BUILD-0-WARN-0-ERROR / PER-SLOT-FIRST-EPOCH-1 / REAL-ALLOCATION-EPOCH-DIFFERENCE-CLOSED / AUTHORITY-CAPTURE-VALID-3-TICKS-6-ENTITIES / AUTHORITY-OUTPUT-GUARD-PASS / TOOL-BUILD-0-0 / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / GLOBAL-LEDGER-PASS / NO-RUNTIME-OR-AUTHORITY-WRITE
-->

> 状态：`FOCUSED_TEST_PASS / CPP-BUILD-0-0 / REAL-DIFFERENCE-CLOSED`

真实首差异为slot1 authority2/Unity1。authority的2来自全局
`next_presentation_generation_++`；contract与Unity handle均定义按槽位生命周期epoch。修正只能发生在
workspace diagnostic writer normalization层，不能改正式authority或Unity slot模型。

回滚：恢复writer直接输出presentation_generation并恢复本ID文档；不触碰authority build tree或Unity。

## 实际结果

- 新增slot-local normalizer：保存每槽last presentation generation与epoch；首次1，变化时+1，空槽不
  清历史。正式authority源码未改，所有build/output仍在workspace Temp。
- C++ build0/0；runner source SHA `8018CB80D81CD09D9D04AB72693719648B92D8B2CF7AF72CC929B1E6F198BEE4`，
  binary SHA `4FD64BC99255BB61FBA1FCE15A9FECD74E4B4010B956D625070D8C120C982DD1`。
- 公共raw valid 3 ticks/6 entities，slot0/slot1 epoch均1，raw SHA
  `9AFFE2F61C52A358900F2FE53642B99D10FC7C6BCE69F25752F8C7CE50475E3C`。
- 真实报告从17 unique/30 equal/99 occurrences变为16 unique/31 equal/96 occurrences；
  allocationEpoch已相等，首差异改为identity.controlSlot。
- authority output guard按预期exit1；工具build0/0、self-test21/21、raw self-test5/5。
- Ledger73 records/14 governed code files/PASS；diff check无error。
