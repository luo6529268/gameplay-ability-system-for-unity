# Task Contract — NTSD28-B5-NEGATIVE-ENVIRONMENT-CLAMP-CORRECTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / SOURCE_HASH_MATCH / CLAMP_TO_ZERO_REQUIRED / PRIOR_NO_CLAMP_CLAUSE_SUPERSEDED`
> 依赖：`NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001 / VERIFIED`

## 目标

纠正前置 owner audit 对 negative-environment transaction 尾部 clamp 的错误记录。只更新治理事实，
不修改 Unity/C++ 脚本、content、Scene、Prefab、ProjectSettings或运行行为。

## 新鲜证据

- 当前 playable closure 中 `source/ntsd28_core/src/simulation/battle_world.cpp` 的 SHA-256 为
  `EB37E8EC1186BF0349A3B3B5759666C3A8F44F0E1640C159E06F805189FBCCB1`，与仓库既有
  `docs/ai/MANIFESTS/NTSD28-B5-HIT-GROUP-ELIGIBILITY.md` / `...FIRST-BDY-RESPONSE.md` 记录一致，
  mtime仍为 `2026-09-04T07:45:13.7505174Z`；没有 source drift。
- `battle_world.cpp:2266-2278` 先执行 `current_hp -= actual_damage`、
  `effective_max_hp -= actual_damage / 3` 和 exact counters，随后明确
  `std::max(value, 0)` clamp两项生命值。
- `battle_world_tests.cpp::test_native_negative_environment_damage_survives_missing_credit_source` 对
  HP/HPBound `5/5`、默认rule9断言最终 `0/2`；
  `test_native_lethal_negative_environment_damage_records_owner_knockout` 同样断言lethal victim HP为0。
- 源码注释把该块回链到 `FUN_0041C3D0 @ 0x0041F9D0..0x0041FA3F`；当前没有新的正式EXE动态
  过量伤害观察产物。此前“no clamp”文字也没有单独可复现的EXE artifact，因此不能覆盖当前无漂移的
  playable source contract与其focused tests。

## 裁决

- `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001` 中仅“HP/HPBound不clamp”一句被本记录
  supersede；正确合同是所有 exact counter/credit 写入后，把 victim HP和HPBound分别clamp到0。
- 该前置 audit 的其余结论继续有效：negative `EnvironmentState320`、native phase12、rule +0x90
  positive-or-9、actual `900/scale`、CatchSource decode、最多两跳Owner、lethal KO、+0x34C按rule、
  +0x348按actual、不写Combo/world legacy stats、不重置EnvironmentState320、不受step-wait gate。
- 后继 production test必须包含 overkill clamp case；不能因为Unity旧分支“碰巧也clamp”而保留其错误
  WeaponCount触发、tick cadence、scale或legacy stat写入。

## 回滚

本记录是事实纠正，不应回滚到已证伪的no-clamp文本。若未来取得固定SHA正式EXE的可重复动态反证，
必须另建 correction record，附观察方式、输入、输出与identity，再调整实现。
