# R8-CANDSTORE-DIAG-001 — candidate store stress accounting correction

<!-- CHANGE-RECORD
id: R8-CANDSTORE-DIAG-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs
authority: USER-APPROVED-R8-WP01G-R05 / test-harness evidence integrity
evidence: COMPILE-0 / FOCUSED-84-84 / STRESS-256-256 / CURRENT-LEGACY-SMOKEPASS-HASH-MATCH-ZEROGC / SELF-CHECK-183505 / LEDGER-80-94
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / diagnostics

## 1. 状态与范围

- 当前状态：`VERIFIED / TEST-HARNESS ONLY`
- 所属 Work Package：`R8-WP01G-R05`
- 只修正压力报告对candidate store消费计数的错误验收关系，不修改candidate生产、顺序、RNG、consume或gameplay。
- 不属于本次范围：collector、store、hit、PreInteraction production、AI、render、容量、worker、T8、IL2CPP、Android、服务器。

## 2. Authority / 需求依据

- C++ Release只读source规定candidate收集后由character/object pass按分支消费；它不规定Unity诊断计数器。
- fresh R05 current run：35/35 store authority、35/35 legacy oracle、shadow mismatch/invalid/fallback均为0，最终hash与forced legacy相同，0 GC通过；但stress validator在首次非空候选时以`entryReadCount == post-tick HitCandidateCount sum`误报失败。
- Evidence等级：诊断缺陷`VERIFIED`；gameplay行为由R05另行分层验收。

## 3. Unity 原状与已确认差异

- `CaptureProductionCounters()`在完整tick及两段consume结束后读取当前`Runtime.HitCandidateCount`。
- store的`EntryReadCount`记录实际消费窗口中的entry读取；candidate可能在consume期间被清理、替换或因abort提前终止，因此它与post-tick carrier count没有一一相等合同。
- 当前validator要求严格相等，使首个candidate读取出现时把零的post-tick count与一的entry read比较并永久污染harnessValidity。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs` | `EvaluateCollisionCandidateStoreAuthorityValidityForReport` | 将post-tick candidate sum误当作精确消费entry数 | 只使用可成立的下界关系，并保留tick/cadence/fallback/failure/oracle mismatch硬门 |
| `Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs` | candidate authority validator tests | 未覆盖consume后carrier变化造成entry reads更高 | 覆盖额外合法entry read通过、少于可观察post-tick下界失败 |

## 5. 不可回退边界

- 不改CentralOnly/Texture2DArray/Mesh/URP；不改容量合同；不改30 Hz、FrameInputSet、slot/generation、SoA/ECS、池、worker或0 GC。
- 不降低store shadow mismatch、invalid、fallback、failure、tick cadence与restore门槛。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `ProductionEntityStressHarness.cs` | `EvaluateCollisionCandidateStoreAuthorityValidityForReport` | `EntryReadCount == postTickCandidateSum`改为`>=`，并说明consume后carrier仅为下界 | 只消除已证实的诊断假失败；其他authority完整性硬门不变 |
| `ProductionEntityStressEditorTests.cs` | `CollisionCandidateStoreAuthority_EntryReadsUsePostTickCountAsLowerBound` | 新增extra read合法、相等合法、低于下界失败三段断言 | 防止validator再次把post-tick carrier误当精确消费计数 |

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity fresh refresh | 0 project compile error | `PASS` |
| focused test | candidate/PreInteraction/validator | 84/84 PASS；完整stress Editor 256/256 PASS | `PASS` |
| Play Mode / 集成 | identical 50-AI current/forced-legacy R05 reports | 双方SmokePassed；20项hash全等；zero-GC与cleanup PASS | `PASS` |
| C++ authority 对照 | release source只读crosswalk | 已闭合；本Change不定义gameplay | `VERIFIED` |
| full trace | R1-WP02 | 未获得 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 风险：把无效的严格相等改得过松。必须保留`entryReadCount >= post-tick candidate sum`下界及所有独立完整性硬门。
- 回滚：只回退validator关系和对应测试，不触碰production gameplay。

## 9. Git / 交接

- 修改前工作树很脏，全部既有改动视为用户工作。
- 实际diff仅允许上述两个脚本及本Record/ledger/state/handoff。
- full self-check：2026-08-23 18:35:05 PASS；Console清理预期负路径与MCP噪声后0 error。
- `Tools/Validate-ChangeLedger.ps1`：PASS，80 records / 94 governed code files。
- scoped `git diff --check`：PASS（仅行尾转换warning）。
