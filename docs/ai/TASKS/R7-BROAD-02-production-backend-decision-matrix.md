# R7-BROAD-02 — production collision broadphase decision matrix

> 日期：2026-08-23
> 状态：`DECISION COMPLETE / RETAIN BRUTEFORCE / NO CHANGE`

## Goal

依据C++ candidate authority、Unity role-aware parity、pair reduction与真实stress证据，决定当前production
是否应从默认BruteForce切到LooseQuadtree。先裁决，不因1000 synthetic收益直接改GameConfig。

## Current facts

- `GameConfig.asset.BattleCollisionBroadphaseName`为空；resolver默认BruteForce；
- C++ authority仍是slot pair升序、双方向exact；Loose只允许减少不可能pair并恢复authority order；
- existing 1000 synthetic：Brute理论499,500 pairs，configured Loose 500 pairs，candidate/RNG一致；
- 历史真实1000-AI stress本身显式使用MobileExtended+LooseQuadtree但仍远低于30Hz；
- 尚无同一当前build、同seed/input/DAT、Brute vs Loose的真实production A/B报告。

## Decision matrix

| Gate | Required to switch default | Current |
|---|---|---|
| source/order parity | candidate order、双方向、RNG、fallback一致 | focused覆盖，待fresh rerun |
| synthetic reduction | 1000 layouts显著减少pairs | 已有500 vs 499,500 witness |
| real workload parity | 同DAT/input/seed的Brute/Loose gameplay checksum/trace一致 | 未完成 |
| real workload performance | current build同负载P50/P95/GC明确改善 | 未完成 |
| failure/fallback pressure | degenerate/fallback不把real workload退化回近O(N²) | synthetic覆盖；real distribution未闭合 |
| R8 scene acceptance | 真实技能/对象生命周期/候选命中无差异 | 未完成 |

## Planned evidence

- fresh role-aware shadow/formal/Loose/participant focused suites；
- focused后same-domain full self-check（D-TEST-001已修）；
- source/config deployment recheck；
- 形成retain/switch/defer决策，不修改代码或配置。

## Stop conditions

- 没有real production A/B却准备修改default；
- parity failure或fallback异常；
- 需要新增stress backend参数或生产配置改动：必须另建脚本Change Record；
- 需要R8 Play Mode或用户负载选择。

## Out of scope

- 本包不修改`GameConfig.asset`、resolver、collector或harness；
- 不处理capacity、render、AI、C++ trace或服务器。

## Result

- fresh job `623e91b88792432a87bccd0969b08ba9`：shadow/formal/Loose/participant 80/80 PASS；
- extra AirRole nearest job `01bea6cea2e340e088683226c81cf713`：8/8 PASS；合计88/88；
- focused后不reload域，03:13:57 full `BattleRuntimeSelfCheck=PASS`；
- synthetic 1000 fixture继续证明500 vs 499,500 authority-pair量级收益；
- current-build real production Brute/Loose A/B、R8 scene parity与real fallback distribution仍未获得；
- 决策：保留空配置→BruteForce默认。未来切换必须另建配置Change Record并先补上述证据。
