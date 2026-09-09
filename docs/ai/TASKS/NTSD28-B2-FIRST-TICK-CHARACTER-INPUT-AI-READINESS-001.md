# Task Contract — NTSD28-B2-FIRST-TICK-CHARACTER-INPUT-AI-READINESS-001

> 状态：`FOCUSED_TEST_PASS / FIRST_TICK_READY / AI_NATIVE_RNG_JOINT_EQUAL / NEXT_FIRST_DIFFERENCE_AI_KEY_HISTORY / FORMAL_EXE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / INPUT-AND-DUAL-RNG`  
> 建立日期：2026-09-04

## 目标

消除 Unity `CharacterInputAll` / `AiInputAndComboAll` 对 `tickIndex == 1` 的整体跳过，让第一个有效 completed tick
执行角色输入与原生 AI pass；保持 `tickIndex <= 0` 为无效调用且不执行。用既有 accepted-only RNG v3 场景验证
authority 的 AI 同步调用数 `6/7/8` 不再整体晚一 tick，并保持 human fixtures 不回归。

## Authority 与当前首差

- 正式 playable `SimulationTickDriver28::step(...)` 每次 step 在 phase advance 后直接遍历 slot；符合
  `control.native_ai` 的角色在首次 step 即调用 `NativeAi28::step_main(...)`，没有“首 tick 跳过”分支。
- authority AI fixture completed tick 1/2/3 的同步 RNG 调用数为 `6/7/8`。
- Unity v3 raw 当前 tick 1 为0次，tick 2/3逐项等于 authority tick 1/2，严格定位到
  `SimulationWorld.AiInputAndComboAll` 与 `CharacterInputAll` 的 `tickIndex <= 1` guard。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task、Change Record、Ledger、STATE、handoff与总表

禁止修改 AI 决策算法、call-site 表、RNG 算法/cursor、输入 phase 规则、pass 顺序、Config/DAT、Scene/Prefab、
ProjectSettings、Packages、权威源码或正式 EXE。

## 不变量

- `tickIndex <= 0` 仍为无效输入，不准备 AI、轮询 human 或消费 RNG。
- 首个有效 tick 只移除外层错误跳过；现有 phase、producer、two-pass、AI eligibility 与 commit 规则不变。
- accepted-only observer 默认 null；生产默认路径仍不启用诊断采样。
- input-common 与 standing-attack human v3 fixture 必须继续 equal；AI fixture 必须达到 equal 或冻结新的真实首差。

## 验收

- 先把 AI exporter 期望改为 authority `6/7/8`，确认修复前精确失败；
- Unity compile 0，focused exporter/input/AI/lockstep tests 通过；
- 三个 v3 fixture 双端 valid，human 两组继续 equal，AI 不再有 first-tick delay；
- full SelfCheck、Console 0、Change Ledger validator 通过；formal EXE 证书仍独立 pending。

## 回滚

仅恢复两个 invalid-tick guard 与本包对应 test/self-check 基线；不回退 accepted-only trace、直接 RNG pre-draw、
defend refresh 或此前已闭合的 B2 行为。

## 完成证据

- test-first job `5153cb5d...`：tick1 expected6/actual0；实现后compile0。
- exporter job `0c280f95...` 11/11；broad job `520105c6...` 149/149。
- 18:38:00 full SelfCheck PASS；预期负向日志清除后Console error 0。
- common/standing v3各3tick/6pair equal；AI synchronized RNG逐次全equal。
- 新首差下移到tick1 slot1 `keyHistory[0]` authority `-1` / Unity `0`，另包处理。
