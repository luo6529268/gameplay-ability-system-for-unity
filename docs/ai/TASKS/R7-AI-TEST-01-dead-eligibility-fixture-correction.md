# R7-AI-TEST-01 — dead AI eligibility fixture correction

> 日期：2026-08-22  
> 状态：`VERIFIED / TEST-ONLY`

## Goal

修正一条仍把 active、`HP <= 0` 的 character DAT AI 当成 input-ineligible 的旧 Editor
诊断测试，使它不再与 C++ release `prepare_ai_input(...)` 以及已落地的
`R3-AI-LIFE-001` 合同冲突。

## Scope

- 只拆分并更正
  `AiSensingSoACandidateEditorTests.DecisionRemainder_IneligibleCharacterInput_DoesNotCountAttempt`；
- dead case 必须证明 decision remainder 仍可尝试并完成；
- coordinate case 继续证明 `Unk3FC > -1000` 在 remainder 绑定前走独立坐标路径；
- 不修改任何 production AI、RNG、target/special scan、pass ordering 或 profile 配置。

## Authority / Evidence

- C++ release `src/input/input_handler.cpp:1615-1759`；
- C++ release `src/entity/game_tick.cpp` 的 input-before-death-cleanup 调用顺序；
- `docs/ai/CHANGE-RECORDS/R3-AI-LIFE-001.md`；
- UnityMCP EditMode job `6fdd44f773344cffbce04404bfddfd86`：在旧 dead 断言处失败，
  `EligibleAttempt` 实际为 1。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs`；
- `docs/ai/CHANGE-RECORDS/R7-AI-TEST-001.md`；
- ledger / STATE / 当前 handoff。

## Unknowns

- 本包不裁决完整 AI decision tree 或 C++ runtime trace；
- dead AI 的真实 respawn / Play Mode joint 行为仍由 R8 承担。

## Deliverables

1. dead 与 coordinate 两个职责独立的测试；
2. 精确用例和 AI sensing focused test 结果；
3. compile / full self-check / ledger validator 证据；
4. 结构化 handoff。

## Verification

- Unity 编译 0 error；
- 精确旧失败用例通过；
- AI sensing/decision-remainder focused suite 通过；
- `BattleRuntimeSelfCheck` 通过；
- `Tools/Validate-ChangeLedger.ps1` 与 scoped diff check 通过。

## Stop conditions

- 修复需要改变 production AI 或 `R3-AI-LIFE-001` 行为；
- dead case 出现 RNG、target、input parity 差异；
- first mismatch 指向 AI decision tree 的其他模块。

## Out of scope

C++ 修改/运行/trace、Unity gameplay、完整 AI decision 认证、Play Mode、R8、T8、服务器和渲染。

## Result

- fresh `Assembly-CSharp-Editor.dll`：2026-08-22 21:01:39，晚于测试源码；Console `error` 为0；
- 精确job `8c74d8e0a76e427fac3fd7920f5ac234`：2/2 PASS；
- AI sensing/profile job `5c6bad85dc0b43c2a6949d03cfd256fc`：111/111 PASS；
- full `BattleRuntimeSelfCheck`：2026-08-22 21:04:52 PASS；
- ledger validator：45 records / 32 governed files PASS；scoped diff check PASS。

`VERIFIED`只表示这条测试合同修正已完成，不提升`R3-AI-LIFE-001`或完整AI gameplay状态。
