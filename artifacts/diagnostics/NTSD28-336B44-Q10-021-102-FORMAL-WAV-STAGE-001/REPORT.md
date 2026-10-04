# Q10 正式 `data/021.wav`、`data/102.wav` 战斗内容暂存

状态：`FORMAL_CONTENT_STAGED / UNITY_IMPORT_AND_NATURAL_VOICE_PENDING`。只新增两份当前正式战斗 WAV 及各自 Unity `.meta`；没有修改 DAT、旧 Sound、生产脚本、Scene 或非战斗模块。当前根 EXE 实测 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

已存在的自然事件依据有明确边界：君麻吕防→纵深上→跳的当前 playable 完整 `GameSession28` 在相对 tick9 起多次发 `data/021.wav`，对应 LFR 的正式根动作/MP/相机 165/165 同态；多由也命中鸣人的正式源码 128 tick 链在 tick1、23 发 `data/102.wav`，原 Battle Scene 生产待播 6 条事件与源码 30/30 字段相同，同一 LFR 已在正式根回放通过。根公开 trace 均无逐条音频字段，不能把源码音频事件写作正式 EXE 扬声器见证。原始事件见 [君麻吕 CSV](../NTSD28-336B44-Q10-KIM-J1-NATURAL-REACH-001/run-03/source-events.csv) 和 [多由也声源对照](../NTSD28-336B44-Q10-C032-NATURAL-HIT-AUDIO-001/source-unity-audio-comparison-v1.json)。

[写入前清单](preflight.json)确认两目标 WAV/meta 均不存在，旧 Sound 两条与正式 PCM 不同。正式 021 和旧 021 均为 mono/8-bit/38400 Hz/34310 帧但 PCM SHA 不同；正式 102 是 mono/8-bit/16000 Hz/35777 帧，旧 102 是 mono/16-bit/16000 Hz/35777 帧。依照既有 battle-only `NTSDSoundPlayer` 单文件正式 VFS 优先解析，仅以独占新建方式复制正式原始 WAV 和新增唯一 GUID `.meta`，未覆盖旧文件。

[写入后复核](postflight.json)：`021.wav` 正式/Unity SHA-256 同为 `AE2ED083B8A94665266AC32522E8A1648B9918CAB12062D838F1CDACC605637E`，`102.wav` 同为 `F0BF315E2CF8C0402D26C9928810F0F6E12233C851C61FE4B23C5CD1BA7E5603`；两个 GUID 各自在 Assets 中仅出现一次。WAV 头的声道、采样位宽、采样率和帧数与正式源一致。旧 021/102 以及 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四保护文件的 SHA 全部与前置相同。两个新增文件没有覆盖/删除审计对象。

原 Editor 经本地 Unity-MCP 再读为非 Play、无测试、Battle Scene 内存 `isDirty=false`，但编译仍 `is_compiling=true` 且无完成时间。本包没有调用 Refresh、Test Runner 或 Play；没有 Unity AudioClip 导入、生产 battle voice、项目 Mixer PCM、声像或实际设备验证。此前源/Unity事件同态也不能代替新增正式音频文件的实播证明。Q10、Q12 和总目标仍开放；Editor 恢复后按 [Task](../../../docs/ai/TASKS/NTSD28-336B44-Q10-021-102-FORMAL-WAV-STAGE-001.md) 在原 Battle Scene 逐条验正式 SourcePath→clip→voice 与有序退出。

交付核对：8 份 Unity 正式暂存 WAV 全部与正式 VFS 原文件逐 SHA 一致；本包相关 `git diff --check` 退出0，`Tools/Validate-ChangeLedger.ps1` 退出0/PASSED（1227 Records、28 个受治理代码差异文件）。这两项不代替 Unity 导入或战斗实播。新增 Task/报告及四个 WAV/meta 均为未跟踪文件，待保留供后续验收，不作删除或清理。
