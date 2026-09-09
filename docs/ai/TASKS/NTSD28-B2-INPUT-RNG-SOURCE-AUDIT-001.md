# Task Contract — NTSD28-B2-INPUT-RNG-SOURCE-AUDIT-001

> 状态：`VERIFIED / SOURCE-INVENTORY-CLOSED / IMPLEMENTATION-SPLIT-DEFINED / GOVERNANCE-ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

闭合 NTSD 2.8-Logan 正式源码快照中的输入相位、human/AI 采样、0x21-byte proxy、
CRT/synchronized 双随机流与生产调用点，并映射 Unity 当前输入和 RNG 所有者，形成可独立验证、
可回滚的 B2 实施包。本包只读，不修改运行逻辑。

## 权威和 build closure

- 唯一权威根：`J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan`。
- 正式 EXE SHA-256：`1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75`。
- `source/README_SOURCE.md` 明确声明源码快照对应当前发行 EXE；playable/core build 脚本均显式纳入
  `src/simulation/native_random.cpp`。本审计只读，没有在权威目录构建或写入输出。

## 权威已观察事实

### 输入

- 每次 `SimulationTickDriver28::step` 先推进 `input_update_phase_4a0b90`：1tu 恒为 0；2tu 从 0
  初值按 1/0 交替。
- 第一遍按 slot 升序执行 non-character `hit_Fa`、AI producer，再采样。AI 每 tick 都把 pending
  采入 current；human/replay 每 tick先 `previous=current`，仅 phase 0 令 `current=pending`。
- slot 0..7 的录像物理输入在采样后、proxy 前冻结。
- 第二遍按 slot 升序处理 proxy 和输入行为。proxy 精确复制 Entity+0xBE..0xDE 共 0x21 bytes：
  7 个 edge window、defend re-entry cooldown、7 previous、7 current、10 combo bytes 与 tail byte；
  不复制 pending、key history 或 run accumulator。
- `step_sampled(..., apply_remap)` 只在 phase 0 应用 remap；human/AI/linked ownership 和升序
  可观察顺序必须保留。

### RNG

- `Msvcr80Random28` 精确执行 `state = state * 214013 + 2531011`，返回 bits 16..30。
- `reset_from_seed` 先 seed CRT，再用 3000 次 `crt_next()%255+1` 填表 0..2999，并清
  serialized byte 3000；因此 reset 后 CRT call count 为 3000。
- synchronized stream 对 `upper_bound < 1` 返回 0 且不推进；否则 counter 按 1234、index 按
  3000 环绕，记录 call count/last call-site，并返回 `(table[index]+counter)%upper_bound`。
- 生产源码中解析到 83 个 synchronized 调用表达式、80 个唯一 call-site ID：
  `battle_world.cpp=29`、`native_ai.cpp=40`、`input_routing.cpp=1`、
  `simulation_tick_driver.cpp=1`、`game_session.cpp=12`；`0x13/0x14/0x15` 各重复 2 次。
- production direct CRT 调用仅 `battle_world.cpp:782-783` 两次 jitter。

## Unity 已观察事实与差异

- Host 在 tick 开始前把完整 `FrameInputSet` 应用到 world；human roster 当前每 tick 都 enqueue
  complete packet key，尚无 1tu/2tu phase cadence、录像冻结边界或精确 0x21-byte proxy 合同。
- world 只有共享 `DeterministicRng`，AI 又使用独立的 `AiDecisionRandomStream`；二者都是同类
  MSVCR80 LCG，但没有权威的 3001-byte synchronized table/counter/index/call-site 状态。
- 非测试脚本静态盘点有 37 个通用 RNG 消费表达式，分布为 LF2Entity 4、AppManager 2、
  BoundaryWall 2、SimulationWorld 1、LateLifecycle 10、RandomWeapon 12、Stage 6。
- AI 三个候选生产实现中有 168 个 `.Rand(...)` 文本调用（46/61/61）；其中存在 legacy/optimized
  重复路径，必须先闭合 live owner，不能把文本数直接当作每 tick 调用数。
- B0 真实三 tick domain compare 的首差已经是 RNG stream topology；输入 mask 相等不等于采样
  相位、proxy 或随机调用顺序已经对齐。

## 实施拆包与依赖

1. `B2-NATIVE-DUAL-RNG-PRIMITIVE`：先以 authority vectors 建立独立双流基元和测试，不接 world。
2. `B2-RNG-WORLD-STATE`：接 world seed/reset/snapshot/checksum/trace schema，不迁移消费者。
3. `B2-INPUT-PHASE-PROXY`：实现 phase、human/AI sample、recording freeze 和 0x21-byte proxy。
4. `B2-AI-RNG-CALLSITES`：闭合 live AI owner后按权威 ID/顺序迁移 synchronized call-sites。
5. `B2-NONAI-RNG-CALLSITES`：迁移 battle/stage/lifecycle 调用；用户批准的随机掉武器行为保留，
   但必须隔离其额外消费，不能污染权威双流。
6. `B2-JOINT-TRACE-EXIT`：同 seed/input/tick 验证 input phase/proxy、双流 state/delta/call-site，
   再决定 B2 exit；未到此包不得称 B2 aligned。

## 边界与回滚

- 不修改共享 Server-owned `DeterministicRng`，不改 Server 协议/transport，不改 DAT/资源、Scene、
  Input Actions、B3+ pass 或 B11 内容策略。
- 选择流程和结果表现仍按用户排除；`game_session.cpp` 中会影响战斗逻辑的 RNG 调用仍需 B8
  交叉验收，不能因页面排除而遗漏。
- 每个脚本实施包独立 Change ID；先测试、后生产代码。回滚按包移除新增 owner/接线并恢复原调用，
  不回退用户或其他包的工作树改动。

## 后续口径纠正

2026-09-04对当前authority source fresh重计得到92个`synchronized_next`文本表达式：battle-world32、
native-AI42、input2、tick-driver1、GameSession15；排除native-AI后为50，另有2 direct CRT。本Task中的
83/43为建立时历史库存，不再用于后续实施；由`NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001`接管。
