<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q10Orasengan078NaturalVoiceProbeEditor.cs
authority: selected 336B44 root EXE and corresponding playable natural Naruto followup Jump data/078.wav event
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001.md
-->

# Q10 正式078自然事件到战斗voice原场景探针

实际脚本已写：新增所声明的独立 `NTSD28Q10Orasengan078NaturalVoiceProbeEditor.cs`，只有原Editor菜单、Play clone OID2/7预配置、正式源坐标/seed/mode初态、55tick离散输入与动作/PP/待播/voice采样、正式和非战斗同名cue的 SourcePath/clip诊断、退出四SHA与独立EditMode残留菜单。输出使用`FileMode.CreateNew`，无请求文件，不改生产、DAT、图、旧Sound或Scene。Unity `.meta` 待原Editor自动生成；编译、真实Play、正式CSV逐tick配对与清理证据仍待，不预报通过。

2026-10-04 结果追加：原Editor已自动生成该脚本 `.meta` 并导入程序集；最终脚本 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0/0 error/299 warnings。第一次菜单请求与另一Play重叠被入口拒绝，无结果；第二次原Battle Scene跑完55个生产Driver tick，结果在后续刷新时写为[原始记录](../../../artifacts/diagnostics/NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001/naruto-078-scene-01.json)。为外部提前退出补 `OnPlayMode`/`Poll` 的 `INTERRUPTED` 留痕分支，未改生产。正式源码CSV与Unity输入相位、动作、Health.PP、combo1、078待播共275/275同，tick34正式clip的生产pooled voice及AudioSource正在播放阳性；战斗正式SourcePath/旧非战斗SourcePath和正式clip54,104 samples/1 channel/22050 Hz阳性。Play结果六SHA同/非Play，独立残留World解绑/Pool0；其后Scene磁盘SHA并发变化，跨检查窗口Scene稳定性`INCONCLUSIVE`。详细比较与风险见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001/REPORT.md)。真实物理键、设备PCM、后续Attack及Q10/Q12仍待，状态不超过`RUNTIME_PENDING`。

任何脚本修改前登记。现有正式文件已按单cue Task暂存，Unity导入/自然voice未证；当前正式root/source限定动作/MP110/110同、源码tick34发078，根trace不导出audio。拟仅新增独立Editor-only原Battle Scene探针，复用正式内容、源坐标投影、离散输入、生产Driver、共用播放器及已有安全退出模式；不改生产、DAT数值、图、旧Sound、Scene或非战斗。计划符号：菜单/SessionState生命周期、OID2/7初态、55tick输入/voice记录、双SourcePath/clip检测、EditMode退出哈希与残留检查。实际文件和验证在写后追加，未运行前状态不超过`CODE_WRITTEN`。范围、验收、风险和回滚见[Task](../TASKS/NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001.md)。
