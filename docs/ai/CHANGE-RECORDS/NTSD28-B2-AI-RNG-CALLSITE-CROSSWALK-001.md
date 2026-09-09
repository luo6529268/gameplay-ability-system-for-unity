# NTSD28-B2-AI-RNG-CALLSITE-CROSSWALK-001 — AI synchronized RNG live-call crosswalk

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-RNG-CALLSITE-CROSSWALK-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD 2.8-Logan native_ai.cpp NativeAi28::step_main live closure; current Unity DataOrientedCanonical AiDecisionKernel/AiCharacterDecisionModule and RNG ownership.
evidence: AUTHORITY-NATIVE-AI-42-TEXT / EXCLUDED-4-NONLIVE / STEP-MAIN-LIVE-38-EXPRESSIONS / LIVE-40-POSSIBLE-IDS / UNITY-CANONICAL-107-RAND-EXPRESSIONS / UNITY-SURPLUS-69 / LEGACY-BUTTON-CROSSMAP-ATTACK-KEYJUMP-JUMP-KEYDEFEND-DEFEND-KEYATTACK / SHORT-CIRCUIT-ORDER-CLOSED / IMPLEMENTATION-SPLIT-DEFINED / AUTHORITY-READ-ONLY / NO-SOURCE-CHANGE
-->

> 状态：`VERIFIED / LIVE_CLOSURE_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`

## 已观察事实

- 早期 `83 expressions / 80 synchronized IDs / direct CRT 2` 是全 production source 的静态库存，
  不是 `NativeAi28::step_main` 单一 live closure 数量；该事实保留，但不得直接作为 Unity AI 接线清单。
- `native_ai.cpp` 中 `0x0E` 属于 non-character first-pass；`0x13` 的
  `select_local_character_target` 版本以及 `0x14/0x15` 的 `respond_to_state3000_target` 版本所在 helper
  未被 `step_main` 调用，不能重复接入。
- `native_ai.cpp` 共有42个文本表达式，排除上述4个后，`step_main` live closure为38个表达式；
  两个ordinary spacing表达式各动态选择两个ID，因此覆盖40个可能ID。
- Unity `DataOrientedCanonical`代码面当前有107个`.Rand`表达式（kernel61 + character module46）；
  其中最多38个有authority结构候选，69个没有唯一live ID。旧profile树贡献65个surplus，两个
  prewrite函数相对authority单一`0x26/0x27`又多4个。
- Unity legacy三按钮名称需先交叉映射：`KeyJump=native attack`、`KeyDefend=native jump`、
  `KeyAttack=native defend`。应用该映射后，多处表面动作错位实际同构；真实差异集中在消费时点、
  threshold、combo index、`0x14`缺四个force-attack carrier、profile sites缺`use_ai` carrier，
  以及`0x26/27`必须合并后才能赋予单一site。
- 完整逐项证据、消费顺序与Unity候选见
  `docs/ai/MANIFESTS/NTSD28-B2-AI-RNG-LIVE-CALLS.md`。

## 后续实施拆分

1. call-site-aware synchronized stream seam：显式site、trace与zero-allocation，不改变AI行为；
2. native prefix/target/ordinary core：关闭`0x11..0x21`及`0x26/27`的条件、动作和顺序；
3. held sites：独立关闭`0x28..0x35`；
4. profile sites与carrier：`0x37..0x3C/0x6C`、force-attack/use_ai数据来源及旧69表达式隔离；
5. authoritative-only cursor commit：shadow/legacy/diagnostics不得推进world stream；
6. 与native input producer migration共同做同seed/input/tick joint trace。

## 边界

- 权威目录只读，本包不修改任何脚本、正式内容或运行行为。
- Unity legacy/shadow 中相似 `.Rand(...)` 文本不自动获得 native call-site ID；只有正式 canonical
  语义与消费顺序确认后才可进入下一实施包。
