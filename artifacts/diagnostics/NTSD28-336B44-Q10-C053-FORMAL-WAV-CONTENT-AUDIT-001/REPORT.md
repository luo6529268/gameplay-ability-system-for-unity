# Q10/C053 自然战斗音效 WAV 内容首差（2026-10-02）

状态：`READ_ONLY_AUDIO_CONTENT_FIRST_DIFFERENCE / PLAYBACK_PENDING / Q10_OPEN`。正式根 `NTSD2.8-Logan.exe` SHA-256 本轮重核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本轮没有修改、复制或删除 WAV、DAT、图片、脚本、Scene 或配置。

已完成的 [C053 自然事件对照](../NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/REPORT.md) 在三人受控初始动作、原 Battle Scene 完整生产 Driver 12 tick 中，正式 playable 的 10 条 `audio_events` 与 Unity `PendingSounds` 的 tick、顺序、路径、源规则 X 为 42/42 同态。其中直接消费 `data\020.wav` 与 `data\067.wav`。事件同态只到**待播队列**，此前没有验证音频文件数据。

| cue | 正式 `resources/runtime/vfs/data` | Unity `Assets/NTSD/Sound/data` | PCM 对照 |
| --- | --- | --- | --- |
| `020.wav` | 16,458 字节；单声道 8-bit PCM、22,050 Hz、16,413 帧；原文件 SHA `4872410C3601F31931CCD2EB5335093ACC132AC73C0BAE7FB15AEB118A482E73` | 33,688 字节；单声道 16-bit PCM、22,050 Hz、16,776 帧；原文件 SHA `969BBFD0E0AD2043902366DA0EEC9DB5C1450255E9F552220130A46A5364B5BF` | 正式 8-bit 按 `(byte-128)*256` 转为 signed 16-bit 后，重叠 16,413 帧仅 35 帧整数精确同；Unity 另有 363 帧，约 16.46 ms。 |
| `067.wav` | 31,214 字节；单声道 8-bit PCM、22,050 Hz、31,170 帧；原文件 SHA `D8D7B12EE94AE2EC26AEC9A93A44B65540FC69194E53E21AB44655E89451C757` | 62,476 字节；单声道 16-bit PCM、22,050 Hz、31,170 帧；原文件 SHA `B335D60273C64B14D003DF7EA99F8BD80BC2958EA4B4128FE4B09E13F6DDEAE9` | 相同帧数但量化整数仅 74/31,170 精确同。 |

两文件均用 Python 3.12.7 标准库 `wave` 读取 RIFF/PCM，并独立对原始文件做 SHA-256；上述结果是**磁盘音频数据差异**，尚未量到 Unity `AudioClip` 输出 PCM 或扬声器声音，不能推断人耳能否察觉。

播放路径也有一个独立待证候选：正式 `source/ntsd28_playable/src/main.cpp:2544-2549` 把 `GameSession28::camera_x()` 交给 `locked_local_battle_stereo_layout28`；`audio_backend.cpp:57-98,583-593` 以事件源 X 和 333/666 的窗口分段计算左右矩阵。Unity `NTSDSoundPlayer.PresentSound` 位于 `Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs:246-254`，将源 X 投为世界位置；`PlayPreparedCue:393-400` 依据 `AudioItem.range` 设 2D/3D 并将 `panStereo=0`。未在当前序列化 `.asset/.prefab/.unity` 中找到这两个精确 WAV cue 的 `AudioItem` 配置；`CreateFallbackAudioItem:656-668` 默认 `range=0`、随机音量/音高0。**不能仅凭源码比较宣称实际双声道输出首差**，仍需同条件 camera X、准备后的 cue、voice 参数与音频输出验证。固定全背景相机和 D-024 比例例外需在该条件下保留。

下一独立实施包应只为已证自然战斗 cue 提供正式 WAV 内容，保留现有 `Assets/NTSD/Sound/data` 文件和非战斗 `PlaySfx` 消费；使用战斗事件的通用资源解析入口，按正式 cue 路径选择已部署文件，不写 cue 专属分支、不批量替换 970 条 WAV。修改任何脚本前建 Task/Change/Ledger/STATE/handoff；然后验证文件 SHA、Unity 8-bit WAV 导入、Battle Scene 真实播放 cue/帧数以及现有非战斗路径不变。声像、voice cap、音量、停止、设备出口另按证据推进。Q10 和总目标仍开放。
