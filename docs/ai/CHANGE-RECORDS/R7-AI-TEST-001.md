# R7-AI-TEST-001 — stale dead-AI remainder fixture correction

<!-- CHANGE-RECORD
id: R7-AI-TEST-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp::InputHandler::prepare_ai_input + src\entity\game_tick.cpp release live path
evidence: SOURCE-CONTRACT-VERIFIED / FRESH-EDITOR-COMPILE-PASS / FOCUSED-2-OF-2-PASS / AI-SENSING-111-OF-111-PASS / FULL-SELF-CHECK-PASS / TEST-ONLY-VERIFIED
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 类型：test / input

## 1. 状态与范围

- 当前状态：`VERIFIED / TEST-ONLY`
- 所属 Work Package：`R7-AI-TEST-01`
- 只允许修正一条 stale Editor fixture；production AI 不得改动。
- 关联 Change ID：`R3-AI-LIFE-001`

## 2. Authority / 需求依据

- C++ `prepare_ai_input(...)` 不以 self HP 为 callback early-return；active dead AI 在后续
  death/respawn cleanup 前仍经过输入链。
- `R3-AI-LIFE-001` 已据此移除 legacy、indexed kernel 与 sensing subject 的三处 self-HP gate。
- Evidence：C++ release source contract `VERIFIED`；C++ runtime trace仍`BLOCKED`。

## 3. Unity 原状与已确认差异

- `AiSensingSoACandidateEditorTests.cs:500-520` 把 `dead` 与 `coordinate` 合并为同一
  “ineligible / zero attempt”测试。
- UnityMCP job `6fdd44f773344cffbce04404bfddfd86` 在 dead case 实际得到
  `AiSoADecisionRemainderEligibleAttemptCountForDiagnostics == 1`，旧断言期望 0。
- coordinate path 仍应在 remainder 绑定前返回，保持 zero attempt。
- 这是测试合同陈旧，不是 source-confirmed production gameplay difference。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs` | stale parameterized test | dead / coordinate 都断言 zero attempt | dead 断言 eligible/applied；coordinate 单独保持 zero attempt |

## 5. 不可回退边界

- 不修改 CentralOnly / Texture2DArray / dynamic Mesh / URP / 1.5× / fixed camera；
- 不修改容量、30 Hz、FrameInputSet、slot/generation、SoA/ECS、pool、worker、0 GC；
- 不修改 production AI、RNG、target/special scan、input edges、pass order或C++ authority；
- `R3-AI-LIFE-001` 继续保持 `RUNTIME_PENDING`，不得由本测试升级为 VERIFIED。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs` | decision remainder eligibility fixtures | 将旧 dead/coordinate parameterized zero-attempt测试拆为两个独立测试；dead验证eligible/applied/context bind各1且无fallback/hard failure，coordinate继续验证zero attempt。 | 只纠正测试合同；production AI无改动。 |

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| source | C++ `prepare_ai_input` + R3-AI-LIFE-001 | dead self无HP early return；coordinate独立早退 | `PASS` |
| observed regression | UnityMCP job `6fdd44f773344cffbce04404bfddfd86` | old dead assertion expected 0 / actual 1 | `FAILURE CAPTURED` |
| 编译 | UnityMCP scripts refresh；读取 fresh Editor assembly / Console | `Assembly-CSharp-Editor.dll` 21:01:39晚于测试源码21:00:50；Console error 0。production DLL因无production脚本改动保持20:41:14。 | `PASS` |
| exact focused | job `8c74d8e0a76e427fac3fd7920f5ac234` | dead + coordinate 2/2 PASS。 | `PASS` |
| AI sensing/profile focused | job `5c6bad85dc0b43c2a6949d03cfd256fc` | 111/111 PASS，0 fail/skip。 | `PASS` |
| full self-check | menu `NTSD/验证/运行战斗运行时自检` | `Temp/NTSD_BattleRuntimeSelfCheck.result` 2026-08-22 21:04:52 PASS；Console仅两条既有rest-binding negative control。 | `PASS` |
| governance | validator + scoped diff check | 45 records / 32 governed code files PASS；diff check PASS。 | `PASS` |
| Play Mode / C++ trace | 不属于本测试修正 | | `BLOCKED / OUT OF SCOPE` |

## 8. 风险、回滚与未关闭项

- 风险：若只改 expected count 而不拆职责，未来 coordinate/dead gate容易再次混淆。
- 回滚：仅回退本 Change ID 对测试方法的局部拆分；不得回退 R3-AI-LIFE-001 production合同。
- 未关闭：真实 dead AI lifecycle Play Mode、C++ runtime trace、完整 AI decision source认证。

## 9. Git / 交接

- 修改前工作树很脏；目标测试文件在本次写入前没有 tracked diff。
- 不提交、不清理、不回滚用户修改。
- validator：45 records / 32 governed code files PASS。
- handoff：`docs/ai/HANDOFFS/HANDOFF-R7-AI-TEST-01-dead-eligibility-fixture.md`

本Record的`VERIFIED`仅裁决测试合同已纠正；`R3-AI-LIFE-001`仍为`RUNTIME_PENDING`，完整AI
source/runtime/Play Mode均未由本Record认证。
