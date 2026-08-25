# R3-STEP-01 — release A→B→C debug-unlock policy and deterministic command

> 建立日期：2026-08-23  
> 状态：`POLICY APPROVAL PENDING / NO CHANGE RECORD / NO CODE CHANGE`  
> D-ID：`D-STEP-001`

## Goal

由用户先决定是否把C++ Release live battle的A→B→C debug unlock移植到Unity。若批准移植，再以
deterministic debug-command edge实现progress/flag与F1 tail predicate；若批准省略，则将其登记为
明确的debug-only Unity policy adapter，而不是继续UNKNOWN。

## Scope

### 若选择移植

- Flow scalar新增unlock flag/progress；
- checksum/snapshot/restore/journal覆盖；
- deterministic debug command producer/consumer；
- A/B/C edge sequence与错误重启；
- F1 render-after-return predicate读取flag；
- focused replay与可选physical outer adapter Play验证。

### 若选择省略

- 只更新policy/差异/验收文档；
- default flag0 step gate继续保留；
- 不删除F1/F2 core或candidate-tail修复。

## Authority / Evidence

- C++ `main.cpp:136-155,2280-2299`；
- C++ `entity_collision.cpp:31-39`；
- C++ `game_tick.cpp:994-1005,2067-2077`；
- source closure：`RESEARCH/R8-WP01G-R01B-d-step-debug-unlock-source-policy-20260823.md`。

## Files likely involved（仅选择移植时）

- `BattleRuntimeState.cs`；
- lockstep scalar snapshot/checksum/restore；
- debug-command/input journal adapter；
- `NTSDBattleTickSystem.cs`；
- focused self-check/Editor tests；
- input asset/Play probe仅作为outer physical验证，不作为logic truth。

## Unknowns

- debug command是否进入正式`FrameInputSet`还是独立可重放control journal；
- physical A/B/C与P1 Left(A)同时触发时的outer routing；
- flag在Unity session/battle reset中的产品lifetime是否严格复制process-global C++行为；
- pause overlay是否需要Unity-native表现适配。

## Deliverables

- 用户policy决定；
- 若移植：独立Task revision、Change Record、schema migration、focused replay/compile/self-check/Play证据；
- 若省略：Decision/adapter登记与D-ID状态更新；
- 与`R2-CANDIDATE-TAIL-01`共用明确tail-skip predicate。

## Verification

- source sequence；
- deterministic replay/checksum/restore；
- F1 flag0/flag1、F2、wrong-sequence/restart矩阵；
- candidate carrier retain/clear joint fixture；
- physical Play仅在logic通过后执行；
- C++ trace仍按R1-WP02状态如实报告。

## Stop conditions

- 用户未作policy选择；
- 需要直接Keyboard轮询进入simulation truth；
- 需要改变30Hz/FrameInputSet总体架构、网络协议或服务器；
- 需要修改/运行/构建C++；
- 需要回退批准Unity adapter。

## Out of scope

普通W/S/A/D/J/K/L战斗输入、R4+、render重构、T8、IL2CPP、Android、服务器。
