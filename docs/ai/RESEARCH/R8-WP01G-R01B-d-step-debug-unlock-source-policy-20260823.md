# R8-WP01G-R01B — D-STEP-001 debug-unlock source/policy closure

> 日期：2026-08-23  
> 结论：`SOURCE-CONFIRMED DIFFERENCE / POLICY DECISION REQUIRED / UNFIXED`  
> 证据：C++ Release source + Unity current source；未运行或修改C++/Unity

## 1. Verdict

`D-STEP-001`不再是`UNKNOWN`。C++ Release live battle外层明确实现A→B→C物理按键边沿序列，成功后
永久写`g_dword_449048=1`；`game_tick(...)`只在该flag为0时执行F1 slow render-after-return。

Unity当前：

- 已实现默认`BattleStepMode/BattleStepGate`；
- 没有debug-unlock flag/progress字段或producer；
- step-wait总是在RenderDispatch后返回；
-旧parity snapshot把`battleStepFlag449048`固定输出0。

因此“解锁后的F1 wait分支”是明确未实现的source difference。是否把release debug功能纳入Unity产品，
需要用户policy决定；在决定前不能擅自新增raw keyboard或把它标为批准省略。

## 2. C++ Release source contract

### 2.1 Build与字段

- Makefile列入`src/core/main.cpp`、`src/entity/entity_collision.cpp`与`src/entity/game_tick.cpp`；
- `entity_collision.cpp:31-36`定义：
  - `g_battle_step_mode`；
  - `g_dword_449048=0`：unlock flag；
  - `g_dword_44FD3C=0`：A→B→C进度；
  - `g_dword_44905C`：per-tick F2 gate。

### 2.2 A→B→C writer

`main.cpp:136-155`的`process_battle_debug_unlock_key`：

1. flag已经非0时不再处理；
2. 只接受A/B/C；
3. progress0期望A、1期望B、2期望C；
4. 正确输入progress++；到3时flag=1并锁progress=3；
5. 错误输入若为A则重启为progress1，否则清0。

`main.cpp:2280-2297`在`SceneState::BATTLE`每个outer frame读取SDL/Windows A/B/C当前态，按
down-edge调用上述writer。该代码不在`DEBUG_SKIP_CHARSEL`条件内，是release live path。

全仓production writer inventory没有普通battle reset：除diagnostic准备代码外，flag/progress只由该序列写入，
成功后在进程生命周期中保持1/3。

### 2.3 game_tick consumer

默认step gate保持：

- `game_tick.cpp:994-1005`：mode1/gate0仍跳过完整post-cooldown input callback；
- `game_tick.cpp:2067-2077`：只有
  `mode==1 && gate!=1 && g_dword_449048==0`
  才设置pause overlay并在render后return。

因此unlock后：

- F1 mode1/gate0仍**不轮询/应用human+AI input**；
- T03～render照常；
- 因flag=1不early-return，继续执行FramePostProcess、late entity、mode2 tail、entity post-frame tail和全局reset；
- F2 mode2→gate1本来就不会命中slow return，与unlock flag无额外分歧。

## 3. Unity crosswalk

| C++ field/producer | Unity current source | 结论 |
|---|---|---|
| step mode/gate | `BattleFlowRuntimeState.BattleStepMode/BattleStepGate`，checksum/snapshot/restore均覆盖 | default F1/F2 core已由R3-INP-002映射。 |
| unlock flag/progress | 无runtime field、无FrameInputSet/debug-command producer | `SOURCE-CONFIRMED MISSING`。 |
| A/B/C outer edge | 无独立deterministic debug edge；A同时是玩家left binding | 不能直接在simulation内轮询Keyboard而破坏lockstep。 |
| render-after-return predicate | `NTSDBattleTickSystem`只使用stepWaitGate，未读取unlock | 解锁分支未实现。 |
| parity/checksum |旧parity固定flag0；lockstep checksum没有unlock/progress | 若决定实现，必须同时定义deterministic snapshot/checksum/restore边界。 |

## 4. 与 D-SCHED-008 的依赖

`D-SCHED-008`的candidate carry只在**实际跳过entity post-frame tail**时发生：

- flag0 + stepWait：C++ early return，必须retain candidate carrier；
- flag1 + stepWait：C++继续tail，必须normal clear，不能retain。

因此`R2-CANDIDATE-TAIL-01`不得用裸`stepWaitGate`作为capture条件，必须使用明确的
`willSkipPostFrameTail`/`didSkipPostFrameTail` predicate。当前Unity没有unlock字段时该predicate等价于stepWait；
未来若实施D-STEP，predicate可增加flag条件而不重写candidate store。

## 5. Policy choices（未替用户决定）

### 选择A：移植release debug unlock

需要独立`R3-STEP-01`：

- 新增deterministic unlock flag/progress到Flow scalar；
- snapshot/checksum/restore全部覆盖；
- 通过可记录的debug-command edge进入，不在simulation tick内直接轮询Unity Keyboard；
- A/B/C sequence与错误重启规则按C++；
- scheduler early-return predicate读取flag；
- Play Mode验证physical edge只是outer adapter，逻辑fixture必须可重放。

### 选择B：明确不移植release debug unlock

必须由用户明确批准为debug-only Unity omission，并把D-ID转为approved policy adapter。即使如此，
default flag0 F1/F2行为与`D-SCHED-008`修复仍必须正确；不能把“省略unlock”扩大为删除step gate。

## 6. Evidence boundary

- C++ writer/read/lifetime：`VERIFIED（source）`；
- Unity缺失field/producer/consumer：`VERIFIED（source）`；
- D-ID：`SOURCE-CONFIRMED DIFFERENCE / POLICY DECISION REQUIRED / UNFIXED`；
- C++ runtime trace：`BLOCKED（R1-WP02）`；
- physical Unity input/Play Mode：未运行；
- gameplay/test脚本修改：0；
- T8/IL2CPP/Android/server均未进入。
