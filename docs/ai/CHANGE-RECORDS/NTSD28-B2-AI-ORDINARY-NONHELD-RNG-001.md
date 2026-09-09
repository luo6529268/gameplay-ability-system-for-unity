# NTSD28-B2-AI-ORDINARY-NONHELD-RNG-001 — ordinary non-held synchronized RNG

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-ORDINARY-NONHELD-RNG-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiDecisionSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiOrdinaryNonHeldRandomEditorTests.cs
authority: NTSD 2.8-Logan derive_difficulty_scalars + step_ordinary_combat + try_generic_close_attack + step_profiled_combat; playable GameSession28 native_ai option writers.
evidence: PRE-CODE-AUTHORITY-CALL-ORDER-CLOSED / TEST-FIRST-11-EXPECTED-COMPILE-ERRORS / FOCUSED-15-OF-15-JOB-43F0B6DAA03E48E69F9A323D376AEB58 / ALL-AI-342-OF-342-JOB-A9775155524A4AA89DACDAF60A830826 / FULL-CANDIDATE-14-3C-1C-1E-1F-21-37-38-COMPLETE / BATTLE-MODE-CARRIER-SEPARATE-FROM-CADENCE / GLOBAL-DIRECTION-LOCK-PRODUCTION-DEFAULT-0 / SURPLUS-69-ISOLATED-IN-SYNC-MODE / WARM-4096-ZERO-ALLOC / SELFCHECK-PASS-2026-09-03T10-34-40 / CONSOLE-0 / LEDGER-125-RECORDS-74-FILES / LEGACY-CRT-PRESERVED / PRODUCTION-CURSOR-UNCONNECTED
-->

> 状态：`FOCUSED_TEST_PASS / NONHELD_SITES_READY / SURPLUS_69_ISOLATED / PRODUCTION_UNCONNECTED`

## 改前事实

- synchronized candidate 已闭合 `0x11..0x19`、`0x3C/0x6C`，但 ordinary movement/tail 仍调用
  只允许 legacy 的无 site `Rand(int)`，所以真实同步 full path 会 fail closed。
- Unity 的 `widePath` 把 oid18/5/31 与 low-HP spacing 混合；authority 动态 site 只由静态
  `battle_mode==1` 的 low-HP 条件选择。
- Unity `InputPhase` 现为 1tu/2tu cadence；继续把它当 authority `battle_mode` 会让相同实体的
  AI 难度、target selection 和 low-HP 分支随 tick 奇偶漂移。
- Unity 两个 prewrite 共 6 个随机表达式，但 authority 只有单一 `0x26/0x27`；剩余 4 个没有 live ID。
- 旧 profiled helper 的 family、`0x21` 阈值和动作条件与 authority 不同。

## 预期改后职责

- snapshot 显式携带静态 BattleMode；同步 context 与 ordinary helper只读该值。
- 同步非 held ordinary 路径按 authority 条件、bound、短路和动作桥消费所有对应 site。
- legacy CRT 路径继续调用原有 movement/prewrite/profile helper，避免本包改变生产行为。
- held、production cursor commit 与最终 joint trace 保持未连接状态。

## 验证记录

- test-first refresh：11 个预期错误，10 个缺 native helper、1 个缺 BattleMode carrier；无任务外错误。
- 新增同步 ordinary movement/common/low-HP/profiled helper；legacy movement、两个 prewrite 与旧
  `ProcessSubHelper` 只在 legacy CRT 路径继续执行。
- `AiDecisionWorldState.BattleMode` 从 production `BattleGameModeId` 原样捕获，`WorldEquals`纳入；
  同步 `CreateContext` 使用静态模式，cadence `InputPhase`不再改变同步 AI 难度/筛选。
- 动态 movement site、`1F→20`、`26→27`、`37→38→39/3A/3B`均保留严格短路；
  `0x21`改为权威 predicted X `<80`、无旧 oid gate。
- full non-held candidate 实际到达`Complete`并得到
  `0x14→0x3C→0x1C→0x1E→0x1F→0x21→0x37→0x38`，证明主 kernel 不再因普通
  非 held 无 site RNG fail closed。
- focused job `43f0b6daa03e48e69f9a323d376aeb58`：15/15；全 AI job
  `a9775155524a4aa89dacdaf60a830826`：342/342；4096 warm helper evaluation+commit 0 allocation。
- SelfCheck 10:34:40 PASS；清理预期负例日志后 Console 0 error。
- Change Ledger validator PASS：125 records / 74 governed code files。

## 未关闭项

- synchronized held `0x28..0x35`仍未迁移；link/held candidate继续 fail closed。
- production snapshot尚未注入 synchronized cursor，只有测试显式启用并提交候选游标。
- Unity `BattleGameModeId` 与 authority mode 的完整 G-04 场景语义仍属 B8；本包只建立不与 cadence
  混用的原样 carrier。
- authority missing-stage-bounds fail-closed只会出现在没有正式 stage context 的异常调用者；当前
  Unity world始终提供 stage fallback，本包未扩展 stage治理。
