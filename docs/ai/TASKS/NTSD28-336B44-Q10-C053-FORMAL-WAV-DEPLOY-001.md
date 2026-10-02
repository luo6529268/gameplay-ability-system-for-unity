# Q10/C053 已证自然战斗 cue 的正式 WAV 内容接入

状态：`RUNTIME_PENDING / AUDIO_CONTENT_SCOPED_PASS`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，新版总表 BATCH-05/Q10。战斗权威是根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的 playable live path 与其 `resources/runtime`。触发证据为[Q10/C053 WAV 内容首差](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-FORMAL-WAV-CONTENT-AUDIT-001/REPORT.md)：同条件待播事件42/42一致，但 `data\020.wav`、`data\067.wav` 正式与现有 Unity 文件的 SHA、PCM 不同。

原状：`NTSDSoundPlayer.PresentSound` 调 `PlaySfx`，`GetOrPrepareCue` 以 cue 名从 `Assets/NTSD/Sound` 构造唯一 `PreparedSoundCue`；战斗与通用调用共享该 cue 缓存。当前两个旧 WAV 必须保留，直接覆盖会改变非战斗播放，也违反文件保护边界。现有正式 `Assets/NTSD/Content/LoganRuntime/vfs/data` 尚无这两个 WAV。正常音频事件必须继续在 64 voice 上限、预热/封存及有序关闭合同内工作。

精确实施范围：

- 只新增 `Assets/NTSD/Content/LoganRuntime/vfs/data/020.wav`、`067.wav` 及各自 Unity `.meta`，逐 SHA 等于正式 `resources/runtime/vfs/data`。不覆盖或删除现有 `Assets/NTSD/Sound/data`、不改 DAT 数值、Scene、Prefab、Input Actions 或非战斗脚本。
- 只改 `Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs` 的战斗 cue 预热/解析：当已部署的正式**单文件**音频路径在 `LoganRuntime/vfs` 下存在时，以通用路径规则为战斗事件准备单独资源键与 clip；缺少正式副本时沿用现有路径。通用 `PlaySfx` 和它的原资源键继续用 `Sound`，同名 cue 的两种用途不得共享错 clip。保持封存后零动态新 cue、已有事件顺序和声音参数，声像矩阵另包处理。
- 如验证需要，仅在 `Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs` 增一个非镜像的聚焦断言：同名战斗/通用 cue 确实取得不同源、正式音频帧数正确、旧通用路径仍是原文件；不得引入 cue 名硬编码生产分支。
- 原 Editor 聚焦解码与 AudioSource 测试通过后，可精确扩展既有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 的独立 Q10 请求/结果出口，复用其受控三人自然 OPoint 链，只附加播放器中正式 clip 与实际 voice 见证；既有 Q07 请求、结果和原证据不得覆盖。

实施前保护：记录旧两个 WAV SHA、Battle/Menu Scene 和 GameConfig/ProjectBattleModeConfig SHA；核对原 Editor idle、非 Play 且 Scene clean。先核对目标不存在再复制。新资源和代码均可按精确清单回滚，但任何实际删除/覆盖都需用户另行明确批准并按删除审计留痕；本 Task 不执行删除。

验收：生成 C# 工程编译零错误；原 Editor 导入正式 WAV 并聚焦检验 8-bit PCM 的 clip 帧数（020=16,413；067=31,170）与正式/旧源路径隔离；原 Battle Scene 当前 C053 的自然 OPoint 双命中链至少证已准备的正式 clip 参与战斗 PresentSound，事件/tick/源 X 不回退、退出 clean、借用0与保护 SHA 不变。保存原始失败及成功结果。运行 `Tools/Validate-ChangeLedger.ps1` 和 `git diff --check`。未取得真实声道输出/扬声器证据前只报告 `AUDIO_CONTENT_SCOPED_PASS`，Q10及总目标开放。

风险：Unity 对 8-bit WAV 的导入/运行时解码可能失败，或封存后的同名双资源缓存可能让通用调用被拒。若任一发生，保留现有旧路径和失败证据，先修复通用解析与测试；不得通过覆盖旧 WAV 或按 cue 逐条特殊处理绕行。音频量化/声像/设备未由本包自动关闭。
