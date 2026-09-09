# NTSD28-B2-NATIVE-ACTION-ROUTING-CROSSWALK-001 — native action routing source closure

<!-- CHANGE-RECORD
id: NTSD28-B2-NATIVE-ACTION-ROUTING-CROSSWALK-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD 2.8-Logan input_routing.cpp/input_state.h/battle_world.h, simulation_tick_driver.cpp, core/playable build scripts and locked input routing tests.
evidence: FORMAL-BUILD-INCLUSION / STEP-SAMPLED-ORDER-CLOSED / COMBO10-PRODUCTION-CALLER-MISSING / APPLY-ACTION-TRANSACTION-DIFFERENCE / DIRECT-HOLD-DIRECTION-GAPS / BUILTIN-STATES-0-1-2-4-5-85-86 / SYNC-RNG-82-83-84 / AUTHORITY-INPUT-TESTS-46-DEFINITIONS-46-MAIN-CALLS / INITIAL-44-COUNT-CORRECTED / MISSING-FIELD-COUNTS / DATA-CARRIER-TYPE-DEFAULT-PRODUCER-OWNER-MAP-CLOSED / SNAPSHOT-SCHEMA-2-3-4-5-CHECKSUM-7-8-PLANNED / IMPLEMENTATION-SPLIT-5 / AUTHORITY-READ-ONLY / NO-SOURCE-CHANGE
-->

> 状态：`VERIFIED / SOURCE_CHAIN_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`

## 已观察事实

- `input_routing.cpp`参与`ntsd28_core/scripts/build.ps1`与正式
  `ntsd28_playable/scripts/build.ps1`；core build同时编译`input_routing_tests.cpp`。
- authority `step_sampled`严格执行：dead clear→可选current remap→jump-edge suppression→edge/history→
  combo10→depth intent→combo fields→three-button direct/hold→direction direct/hold→built-ins。
- authority `input_routing_tests.cpp`有46个独立test函数且46个均由`main`实际调用，覆盖字段消费、费用/fallback、same-sample
  combo、history priority、状态0/1/2/4/5/85/86、action215、182/188、dead clear和remap。
- Unity exact combo selector能读取10个combo字段，但没有production caller；现有routing只消费投影后的
  legacy 9-combo。exact index6/7/9无法投影，exact attempt也没有production clear事务。
- Unity action helper不等于native `apply_action`；当前缺失/未闭合action lock、state redirect、frame `hp`、
  recmp/mode multiplier、double cost、waiver、三层fallback、last-action mirror、effective-max-HP cost及
  caller-specific frame-counter规则。
- Unity只有`hit_a/d/j`直接字段路由；没有hold和direction字段的frame carrier/parser/consumer。
- authority input built-ins消费synchronized call-site `0x82/0x83/0x84`；Unity站立/武器攻击仍用通用
  `BattleRandInt`，不能与AI已迁移的同步stream混为一谈。
- 详细字段数量、现有Unity对应和实施依赖见manifest。
- carrier addendum已逐项冻结input lock/last action/remap/bound、resource transaction totals与flags、
  frame hp、stats recmp/caughtact、use_ai、hit_ja globals/fusion字段，并区分B2 consumer与B3/B5/B7/B8
  producer/timer所有权；snapshot/checksum schema升级路径已固定。

## 边界

- 本包只读；authority与Unity C#均未修改。
- Unity正式Config仍由B11策略门保护；新增parser/carrier能力不授权迁移内容值。
- 当前producer migration尚待Play reload闭环；action实现包不得反向改变producer两遍顺序。
- 初始清单误记为44 tests；2026-09-03以函数定义与`main`调用分别直接重计，结果均为46，已同步
  纠正总表、handoff、STATE、Ledger与manifest；不把旧计数继续传播为权威事实。
