# Q10 鸣人螺旋丸后续跳跃分支声音资源身份定向核对（只读）

> **2026-09-30 SHA 文字更正（覆盖下文截断值）：** 下文 `nar.dat` 的 SHA-256 少抄 8 位，不是有效的 64 位 SHA。重新直接读取正式 `resources/runtime/decoded_dat/c/nar/nar.dat` 与 Unity `Assets/NTSD/Content/LoganRuntime/decoded_dat/c/nar/nar.dat`，两者实际同为 `6BE721524C8CCA0E293BEB8D6BF1DFEE306CCB948181BDAA94545EDB29418ED9`（64 位）；文件字节未改，原“同内容”结论不变。旧截断串保留在下文供审计，不可用于后续身份校验。

状态：`VERIFIED_STATIC_CONTENT_DIFFERENCE / NATURAL_FRAME326_EVENT_PENDING / NO_AUDIO_DEPLOYMENT`。父项：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q10 / O-05 / R17`。2026-09-29 在原工作树和正式 `NTSD2.8-Logan.exe` 身份 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 下，只读正式 DAT、正式 VFS、Unity 暂存 DAT、Unity 旧 Sound 及生产播放器；未运行新的 EXE/Unity 战斗、未改声音文件或代码。

## 首个明确候选

- 正式 `resources/runtime/decoded_dat/c/nar/nar.dat` 与 Unity `Assets/NTSD/Content/LoganRuntime/decoded_dat/c/nar/nar.dat` 均为 SHA-256 `6BE721524C8CCA0E293BEB8D6BF1DFEE306CCB948181BDAA94545ED9`。两者 `<frame> 326 Orasengan` 均声明 `sound: data\078.wav`；该帧还生成 OID518。**不能把它误作首次防+前+跳进入的普通螺旋丸帧：**同份 DAT 的 241～253 帧写有 `hit_j: 325`，须在螺旋丸持续帧再次触发跳跃分支，325 才按 `next: 326` 进入该声音帧；相邻 `hit_a: 300` 是另一条攻击分支。正式 playable `battle_world.cpp::append_native_frame_sounds` 在进入声明帧时产生音频事件，`main.cpp::audio_roots` 与 `audio_backend.cpp::AudioResourceResolver28::resolve` 从正式 VFS 选择资源。Q06 的 frame-sound producer 已有其独立限定验收，本报告不重做。
- Unity `NTSDSoundPlayer.GetOrPrepareCue` 当前默认把 cue 接到 `Application.dataPath/NTSD/Sound`，当前保存的 Battle Scene 的 `AudioController.AudioList` 为空。`Assets/NTSD/Content/LoganRuntime/vfs` 仍无正式 WAV。因此，若该声明帧在当前 Battle 发出 `data\078.wav` 且无运行时注入覆盖，播放器会选旧 `Assets/NTSD/Sound/data/078.wav`，而非正式 VFS 版本。这里的**实际自然帧 326 声音事件及设备输出尚未运行验证**；静态路径与文件身份差异已证。
- `data/078.wav` 正式文件 SHA-256 `D8CAB6CE5CCF129FD689F9FD0170D06B21E04B930BE77EF9AAC9AD4B563F3560`；Unity 旧文件 `78688CFD046DCA0A33F36A10F589EC2ADDE83301AF58BD99FE0C9A5823A69DF5`。两者均单声道、22050 Hz、54104 帧；正式 PCM 为 8 bit（SHA-256 `B2C9A46EC782A37F0F1AEBEAA749EC8DC2D5DE12DB2FAD9B192C761A83F13F0E`），Unity 旧 PCM 为 16 bit（SHA-256 `3150732A644B8341E0641741C66BA36BFDD3D897E2D0E286E4529852A1D94E39`）。把正式 8-bit 样本按 `(byte-128)*256` 投到 16-bit 后，54104 个样本中 340 个整数相等、平均绝对差约 123.41/32768、最大 261；可能是同一近似波形的不同量化，不能仅凭哈希称为明显可听差异，但也不能称为正式 PCM 相同。

## 正反控制与当前资源计数

已有[Q10 自然鸣人 D 键 Play 验收](../NTSD28-Q10-ENTITY-AUDIO-SOURCE-X-001/ACCEPTANCE.md)在 Battle 中观察到 `data\003.wav` 与 `data\004.wav` 两个实际事件、预热声音与声源 X。这次对两文件的正式/旧 Unity WAV 做 `wave` 解析：整文件 SHA 分别不同，但声道、采样宽、22050 Hz、帧数及**全部 PCM 帧 SHA 完全相同**（003：4857 帧、PCM `9DF1C977C5D312C03368D6FD8F0D7A860CA91E77177AC3E897691F1513E2DC68`；004：5536 帧、PCM `4D55DEB8F1136E02AD4BD9CB0412EB041E3EED989979BD01C972FF41C8FEC274`）。因此不能用“整文件哈希不同”要求机械替换这两条已知样本；声道、音量、声像及最终扬声器表现仍属独立验收。

旧[Q10 入口报告](../NTSD28-Q10-FRAME-SOUND-ENTRY-READINESS-001/REPORT.md)中的“976 不同 cue / 71 旧路径 / 905 缺”是**大小写和分隔符未折叠的 2026-09-22 词法快照**。其[2026-09-25 当前物理路径复核](../NTSD28-Q10-FRAME-SOUND-ENTRY-READINESS-001/CURRENT-OVERLAP-AND-TYPED-CUE-REFRESH-20260925.md)归一化为 970 条正式物理路径、68 条旧 Sound 同路径，其中 1 条整文件相同、67 条不同；现有原 Editor 预热日志的失败警告涉及 902 条正式帧 cue 物理路径。它们都不是 902 个已发生的自然声音失败。本次只核三个已具名 cue 的文件/PCM，未重扫 970 条或改写旧 CSV。

## 下一有界出口

先在原 Battle 的**自然鸣人防+前+跳进入 241～253 后，再按跳跃触发 `hit_j:325`**动作中确认 frame326、`data\078.wav` 音频事件、预热所选路径及实际 clip 身份；取得正式配对/根发行相同输入的声音事件 tick。若它确实到达当前战斗播放器，则单独建 Q10 Task/Change，在不覆盖旧 `Sound`、不改 DAT、Scene 或非战斗音频的前提下，声明通用的**战斗正式 VFS 根选择**和精确已证消费者资源。新增文件须逐路径核 SHA/GUID 与 Player 侧载；路径逃逸、缓存/关闭、旧根回退、原 Editor Play 和正式音频对照为验收。不要为这一帧写 `078` 特例，也不要据本报告复制 977 条候选 WAV；声音整包范围尚无用户确认。若自然动作未发该事件，先找第一可达的非例外 PCM/播放首差，Q10 仍开放。
