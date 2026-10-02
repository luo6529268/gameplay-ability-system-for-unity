<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-P13-INPUT-PHASE-PAIR-001
status: VERIFIED
change-kind: BATTLE_Q09_EDITOR_DIAGNOSTIC_INPUT_PHASE_PAIR
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs
authority: formal 336B44 playable 2tu human input sampling and the natural Han earthquake Scene first-difference
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-P13-INPUT-PHASE-PAIR-001.md
-->

# Q09/P-13 韩自然地震探针输入相位配对

脚本修改前记录。当前 Unity 原Battle Scene探针在生产世界已到 tick5 后添加韩和李，下一直接完整 Driver tick6 处于可采样相位；正式336B44根LFR从 phase0 起，tick1 phase1 不消费新 Attack，tick2 phase0 才选韩 action60。正式根与Unity已有50tick的韩动作/源X错开一tick，而李在抓取前的被动动作按相同相对tick 8/8一致。将韩整体平移一tick再比较李，会人为制造正式tick4/8两处李动作差；不能据此修改生产战斗规则。

本 Change 仅让已有请求式 Editor 探针在两名诊断角色出生前设置正式 fresh-session 的 2tu input phase0，并在报告中写入原相位与配对相位，复用现有完整 Driver 输入、PNG捕获、唯一输出和清理。原脚本SHA、范围、前置条件、副作用、保护边界、验收与回滚在同ID Task。修改后立即记录准确符号、编译/运行原件与未验项；当前尚未改脚本或运行。

实际代码已写，仅修改 `NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs`：`Report` 增加 `inputPhaseBeforePair/inputPhasePaired`，`Run` 先检查 2tu，再于两诊断角色创建前把此测试 World 的 `Runtime.Flow.InputPhase` 设为0；`TickRow` 逐tick记录 `inputPhase` 并断言奇偶相位。原角色创建、50tick输入、地图相机捕获、请求与关闭职责保持。未改生产、DAT、Scene、背景或音频。生成项目编译、原Editor Play、像素/正式根同tick对照及Ledger结果待写。

原本的请求文件在上一轮已由探针写为 `requested:false`，保留不覆盖。本轮同一脚本还新增 `PhasePairRequestPath` 的独立唯一请求入口，限定只接受映射 X520/Z400 的自然捕获；仍由既有 `WriteConsumedRequest` 写回这一新路径，输出沿用唯一runId保护。第一版相位字段的生成工程曾编译0错/251既有warning；新增请求入口后的最终编译及原Editor Play待验。

2026-10-02 最终证据：新增请求入口后的生成 Editor 工程 `dotnet build` 0 error/251 existing warning，日志 `artifacts/diagnostics/NTSD28-336B44-Q09-P13-INPUT-PHASE-PAIR-001/generated-editor-build.log`；原 Editor 导入后对保存的原 Battle Scene 完整 Play 50 tick，唯一 JSON 为 `PASS_SCOPED_PLAY`，正式初相位配对前1/后0、报告逐tick相位交替；根336B44与Unity同一相对tick1～50八字段400/400，韩145/149/背景偏移/归零时点tick4/10/16/21一致。新三张Map相机PNG的天空/地面区域偏移+1输出像素及归零复原；仅Map背景，非正式EXE或合成画面。Play已停止、池借用0、材质/相机恢复；旧JSON/三PNG的SHA未变，四保护资产SHA与 `LoganRuntime` 限定状态稳。完整原件、比对口径、哈希及剩余出口见 [报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-P13-INPUT-PHASE-PAIR-001/REPORT.md)。本ID仅诊断探针限定关闭，Q09/P-13/Q07及整场仍开放；当前正式背景Z钳位、可见像素与全World未验证。

最终审计：`git diff --check` exit0，只有现有 CRLF 提示；首次通过子 `powershell -File` 调 validator 因其默认参数中的空 `$PSScriptRoot` exit1，是启动方式错误，未得出代码失败结论。随后直接调用 `& Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exit0，输出首行 `Change ledger validation PASSED.`，原始日志见 `artifacts/diagnostics/NTSD28-336B44-Q09-P13-INPUT-PHASE-PAIR-001/change-ledger-validation.txt`；后续 warning 为大量历史 Record 路径不在当前 diff 的提示。
