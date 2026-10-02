<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-C053-FORMAL-WAV-DEPLOY-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: 336B44 C053 natural battle cue event parity and formal WAV content first difference
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-C053-FORMAL-WAV-DEPLOY-001.md
-->

# Q10/C053 正式战斗 WAV 内容接入

**脚本修改前建立。** 当前目标、原实现、精确代码/音频路径、可观察首差、用户例外、预期副作用、不回退边界、验收与回滚见同 ID [Task](../TASKS/NTSD28-336B44-Q10-C053-FORMAL-WAV-DEPLOY-001.md)。仅为正式可达 `data\020.wav`、`data\067.wav` 部署新版音频副本，并在战斗事件的共用准备入口选择已部署正式文件；非战斗同名调用继续旧根。实际代码和导入、clip、Play 及校验结果待填，不将 Task 意图写成已验证事实。

2026-10-02 扩展诊断脚本前修订：原 Editor 的正式 WAV 解码、AudioSource 播放聚焦 1/1 与资源路径/通用零分配相邻 2/2 已过。为满足原 Task 的真实 Scene 出口，追加既有 C053 Play 探针为第三个 `code-path`；只加独立 Q10 请求和结果路径及声音观察字段，不修改既有 Q07 输出或生产战斗规则。若播放器尚未预热、实际 voice 不使用正式 clip，按首差留证，不静默跳过。

首轮真实 Scene Q10 探针 `ank580-ank580-jira500-formal-wav-01.json` 为 FAIL、正常退出且 Scene clean：诊断错误地限定播放器必须属于 Battle Scene，而 `AppManager.EnsureRuntimeModules` 把播放器挂在跨 Scene 持久对象上。未进入目标 12 tick；这不是播放规则首差。随后仅更正诊断查找为现有项目惯用的活跃 `FindObjectOfType<NTSDSoundPlayer>()`，新 runId `-02`；旧失败结果保留。

实际改动：新增两份正式 WAV 与 Unity meta；`NTSDSoundPlayer.GetOrPrepareCue` 在战斗单文件 cue 已有正式副本时建立独立源路径/缓存键，`PrepareBattleCuesAsync` 同时准备旧通用副本，`PlaySfx` 以 battle 标识取对应 cue。原 `Sound` WAV 不动。聚焦测试在 `SoundPresentationDispatchEditorTests` 覆盖源隔离、正式采样帧和实际 AudioSource 引用。既有 C053 诊断探针只加 Q10 独立请求/结果及逐 tick voice 见证；旧 Q07 请求和结果路径不变。

验证：正式源/部署两个 WAV SHA 各自一致，旧两个 WAV 与四份 Scene/配置保护 SHA 均不变；生成 Editor 工程编译 0 error，原 Editor 导入并运行具名解码/播放 1/1、路径隔离及通用零分配 2/2 PASS。真实 Battle Scene 第二轮12个生产 Driver tick `SCOPED_PASS / DONE`，首 tick 3事件/3播放、相对 tick4/7/8 合计另7播放，voice 分别引用正式020/067，样本帧16413/31170、Play退出clean。原始结果、首轮探针自身查找错误和作用边界见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-FORMAL-WAV-DEPLOY-001/REPORT.md)。声像矩阵、设备输出、Player文件部署、其它 cue 与本轮未记录的 shutdown pool borrower 数仍待；只报本 C053 音频内容限定通过，Q10/总目标开放。

最终复核：为避免战斗事件每次拼接正式缓存键，改为独立字典按原 cue 名读取，原通用字典与播放参数不变。该修订后生成 Editor 工程编译0错、原Editor具名3/3 PASS（job `c7d837525cf84b08a7bcabcdea40de25`）；重新加载原Scene后的第三轮 [formal-wav-03](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-FORMAL-WAV-DEPLOY-001/ank580-ank580-jira500-formal-wav-03.json) 仍为12tick/10事件/10实际播放、正式clip帧16413/31170、`SCOPED_PASS / DONE`。前两轮结果保留。四Scene/配置及旧双WAV SHA6/6不变；`Tools/Validate-ChangeLedger.ps1` exit0/PASSED（1148 Records、25受管代码diff），`git diff --check` exit0。pool borrower、声像与设备等出口仍待，Record 保持 `RUNTIME_PENDING`，不把 Q10 写成完成。
