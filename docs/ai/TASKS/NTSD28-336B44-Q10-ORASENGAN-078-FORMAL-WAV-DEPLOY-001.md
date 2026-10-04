# Q10 鸣人螺旋丸后续 Jump：正式 `data/078.wav` 战斗接入

状态：`FORMAL_CONTENT_STAGED / UNITY_IMPORT_AND_RUNTIME_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`；新版总表 BATCH-05/Q10。此文件准备独立子包，不代表资源已复制、Unity clip 已导入或战斗播放已通过。2026-10-04 原 Editor 已独立确认测试停止、Battle Scene clean，但编译长期停在进行中，刷新两次未恢复；见[现场报告](../../../artifacts/diagnostics/NTSD28-336B44-ORIGINAL-EDITOR-COMPILE-RECOVERY-20261004/REPORT.md)。

## 权威与触发

当前正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。同一 55 tick LFR 在正式根的 action/MP 与当前 playable 源码 110/110 相同；tick34 自然输入 `hit_j` 经过瞬态 325 到 326/MP250，源码 `last_tick.audio_events` 同 tick 发一次 `data/078.wav`。正式根公开 trace 不导出音频，限制见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-ORASENGAN-JUMP-ROOT-LFR-20261004/REPORT.md)。正式/Unity 暂存鸣人 DAT 同 SHA，不改其数值。当前 Unity `LoganRuntime/vfs/data/078.wav` 不存在，旧 `Assets/NTSD/Sound/data/078.wav` 存在；正式与旧文件格式分别为 8-bit 与 16-bit、逐样本不同但波形高度相关，不能推定玩家可闻首差。

## 精确范围与边界

- 只新增 `Assets/NTSD/Content/LoganRuntime/vfs/data/078.wav` 及对应 `.meta`，前者逐 SHA 等于正式 `resources/runtime/vfs/data/078.wav`。沿用已有 `NTSDSoundPlayer.GetOrPrepareCue(soundId, true)` 的共用正式文件优先路径；生产脚本没有证据要求逐 cue 特例。
- 保留 `Assets/NTSD/Sound/data/078.wav` 和所有非战斗 `PlaySfx` 路径；不覆盖、不删除、不移动旧资源，不改 DAT、角色图片、Scene、Prefab、Input Actions、相机或模式 Asset。
- 若实际导入/播放暴露共用解析器问题，在修改任何脚本前另建 Change Record、登记精确代码路径和首次差异；不得为 `078` 写硬编码生产分支。
- 原 Editor 最新只读状态为 Battle Scene clean、非 Play、无测试，但编译仍在进行中。2026-10-04 执行口径更正：`NTSDSoundPlayer` 的正式单文件选择是 `File.Exists`，`NTSD_ResourceLoader` 使用绝对 `file://` 路径 `UnityWebRequestMultimedia.GetAudioClip`；因此可先**仅新增**已核SHA的正式原始 WAV/.meta，不要求旧程序集运行，也不调用 Unity Refresh。新增后立即复核目标、旧 WAV 与四保护文件；这一步最多标 `CONTENT_STAGED / IMPORT_PENDING`，不得写成导入/播放通过。任何 Unity 导入、测试、Play 仍必须等 Editor 编译完成、Scene clean、无测试。现有 Unity-MCP 明确禁止执行 `File/Quit`，不得绕过；已请用户手动重开原实例。不得保存或丢弃别人未确认的内容。

## 验收与回滚

先记录正式/旧 WAV、Battle/Menu Scene、GameConfig 和 ProjectBattleModeConfig 的前置 SHA，确认目标 WAV/meta 均不存在。新增后确认正式/暂存 SHA 相同、旧 WAV 与四保护文件原 SHA 不变；生成工程零编译错误，原 Editor 导入后的 battle clip 格式/54,104 采样帧正确、同名通用 cue 仍解析旧 Sound。原 Battle Scene 复现本 LFR 的**防2 tick→前2 tick→跳2 tick，后续第34～35 tick再按跳**，记录真实输入、完整 Driver tick、action 253→325→326、MP、待播 `data/078.wav`、实际 battle voice clip 和 SourcePath。此链只验证后续 **Jump**，不能代替用户此前报告的后续 **Attack→螺旋手里剑** 分支。退出时 Scene clean、声音借用归零；必要时用近似输入阴性控制确认不是旧 Sound 或测试直接注入。声像/设备 PCM 另证；本子包最多报告内容与自然 voice 限定通过，Q10/Q12 不自动关闭。

如导入或播放失败，保留失败原件与新资源供分析，不擅自删除；任何撤销新资源亦按 `docs/ai/file-removal-audit-contract.md` 建逐文件操作记录并取得所需授权。运行 `Tools/Validate-ChangeLedger.ps1` 与 `git diff --check`，保存实际测试命令和结果。

2026-10-04 执行结果：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-ORASENGAN-078-FORMAL-WAV-DEPLOY-001/REPORT.md)。正式WAV与Unity暂存逐SHA相同，新增meta GUID唯一、旧文件与四保护SHA稳；原Editor仍clean/非Play/无测试但编译未完成，未做导入、AudioClip或自然voice验证。
