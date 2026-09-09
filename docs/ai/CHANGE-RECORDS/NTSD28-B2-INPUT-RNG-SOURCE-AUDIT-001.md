# NTSD28-B2-INPUT-RNG-SOURCE-AUDIT-001 — input/RNG source audit

<!-- CHANGE-RECORD
id: NTSD28-B2-INPUT-RNG-SOURCE-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD 2.8-Logan current-EXE source snapshot; core/playable build scripts; native_random.cpp, simulation_tick_driver.cpp, input_routing.cpp and production RNG consumers.
evidence: AUTHORITY-SOURCE-CURRENT-EXE / NATIVE-RANDOM-IN-BUILD-CLOSURE / INPUT-PHASE-BEFORE-SAMPLING / 1TU-ZERO / 2TU-ALTERNATES-1-0 / AI-SAMPLES-EVERY-TICK / HUMAN-CURRENT-PHASE0-ONLY / RECORDING-BEFORE-PROXY / PROXY-EXACT-0X21 / CRT-LCG-EXACT / RESET-CRT-CALLS-3000 / SYNC-TABLE-3001 / SYNC-CALLSITES-83-EXPRESSIONS-80-IDS / DIRECT-CRT-2 / UNITY-GENERAL-RNG-37 / UNITY-AI-RAND-TEXT-168 / B0-RNG-TOPOLOGY-FIRST-DIFFERENCE / IMPLEMENTATION-SPLIT-6 / NO-SOURCE-CHANGE / NO-AUTHORITY-WRITE
-->

> 状态：`VERIFIED / SOURCE-INVENTORY-CLOSED / IMPLEMENTATION-SPLIT-DEFINED / GOVERNANCE-ONLY`

B2 的输入与双 RNG 权威链、Unity 原状和六个实施包已闭合。当前只允许从独立、未接 world 的
双 RNG 基元及 authority vectors 开始；本记录不表示输入/RNG 已对齐。

> **2026-09-04 correction（不改写原审计历史）：** 当前authority source fresh文本重计为
> `battle_world.cpp=32`、`native_ai.cpp=42`、`input_routing.cpp=2`、tick driver=1、
> `game_session.cpp=15`，总计92个`synchronized_next`文本表达式。排除native_ai后为50，而非本记录建立时
> 使用的43；另有2个direct CRT。新口径及逐项交叉由
> `NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001`接管，旧83/43数字只保留为历史观察。
