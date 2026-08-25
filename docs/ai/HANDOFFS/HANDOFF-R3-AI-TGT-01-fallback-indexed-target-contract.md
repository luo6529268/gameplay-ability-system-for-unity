# HANDOFF — R3-AI-TGT-01 fallback / indexed AI target-selection contract

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change Record：`R3-AI-TGT-001 / RUNTIME_PENDING`

## 已完成的最小闭环

- C++ release source 的 target scan、team/phase、air override和`unk_360` cache contract已从
  `src/input/input_handler.cpp:1667-1750`闭合；
- Unity fallback、SoA/indexed与canonical decision kernel均已建立静态 crosswalk；
- `BattleRuntimeSelfCheck` 新增固定 Unity seed 的 legacy/data-oriented profile-pair fixture：
  - phase2 foreign-team 同距 ground/air low-slot tie；
  - phase1 non-team5 reject、team5 candidate accept、team5 self foreign accept；
  - cached living character的 nonzero retain 与 zero refresh；
  - final `Unk360`、input signature、RNG state与RNG call count同值。
- final UnityMCP scripts refresh后 filtered `error CS`=0；
  `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 2026-08-22 01:53:46 +08:00 为 `PASS`；
  ledger和`git diff --check`均 PASS。

## 已记录的 first-difference，不可遗忘

1. **01:45:50**：fixture在 register 后只写 `Runtime.Unk360`；legacy读取mirror、indexed读取bind-time
   canonical row，得到 `legacyTarget=7 / indexedTarget=4`。这是不一致的 fixture initial state，不是
   production AI bug。
2. **01:51:01**：fixture只经 `CharacterInputWriter.CommitAiDecisionState` 更新store；该 writer不镜像
   `Unk360` 到Runtime，正式 `BattleAiInputWriter` 才会做该动作。仍是fixture setup不完整。
3. **最终修正**：通过canonical writer设store，并同步同值的Runtime mirror；随后 self-check PASS。

不得删除这两次失败记录，也不得把它们写成“已经找到并修复 production AI behavior”。它们说明 data-oriented
initial state必须同时满足canonical store与compatibility mirror。

## 未关闭项 / 不可扩大结论

- 该 PASS 仅证明固定 Unity fixture中的 fallback/data-oriented parity，不是 C++ executable trace；
- R1-WP02 full trace仍 BLOCKED，禁止为此运行/修改C++；
- 真实 AI Play Mode、特殊对象 target override、combo、held/link、collision/lifecycle依赖仍未验收；
- 不修改AI default profile、spatial index、worker/ECS layout、physical input、scene/DAT、capacity或渲染。

## 连续下一步

`R3-PHY-01` 是用户负责的 InputAction / Inspector / W-S-A-D-J-K-L Play Mode验证，按 D-011 保持非脚本
`UNKNOWN`，不阻断代码链。连续的下一个代码级 Work Package 是 `R3-FRAME-01 / D-MOV-001～003` 的**只读
source preflight**：先从 C++ `frame_advance.cpp`、`physics.cpp`与`game_tick.cpp`重新闭合 current-key
lifetime、landing raw frame writer、integer-sync/respawn时点，再建立新 Task Contract与 Change Record；未建
Record前不改脚本。
