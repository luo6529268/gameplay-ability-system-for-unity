# Q10 当前 336B44 双声道战斗 cue 内容与帧声明边界

2026-10-02。状态：`CURRENT_CONTENT_STEREO_FRAME_DECLARATIONS_CONFIRMED / NATURAL_EVENT_PENDING / NO_RESOURCE_EDIT`。这是当前正式内容和 Unity 暂存内容的只读审计，不是“13 个声音已在战斗中播放”或 stereo 输出矩阵验收。

本轮重验正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；正式 `resources/runtime/catalog.csv` SHA-256 `0AAB4A0FFEE70F17D31DDF952254C798DFEF185FEDD55CACEC28C7A66181E6FE`。扫描当前正式 `vfs` 的 981 个 `.wav`，Python `wave` 均能解析，其中 13 个为 2 声道 PCM。逐文件格式、采样帧和 SHA 见[正式 WAV 清单](stereo-wav-sha256.csv)，该 CSV 自身 SHA-256 `D0BB1A00143BE2371D68D3C273FB03261E6848F3365DB4454485A9D7BBE9A6D4`。

以**当前**正式 catalog 的 330 个 object 路径读取 Unity 生产根 `Assets/NTSD/Content/LoganRuntime/decoded_dat`，检查每个 DAT 的 `sound:` 与 `<frame>` 所属关系：13 个双声道 cue 全部由对象帧声明，共 17 条帧声明、12 份不同 owner DAT；这 12 份暂存 DAT 的原始 SHA 均等于当前正式 catalog 的 `plain_dat_sha256`，17/17 行均有同版 owner。无缺失的暂存对象 DAT。[逐帧 CSV](stereo-frame-declarations.csv) SHA-256 `92674054A9845CD7E0B70A5BCAD6FC36267F19A73C05D9E6F627667B32011A32` 保留 cue、OID/type、DAT、帧号/行号、正式明文 SHA 和 WAV 元数据。

| 当前内容观察 | 结果 |
| --- | ---: |
| 正式双声道 WAV | 13 |
| 含这些 cue 的对象帧声明 | 17 |
| 声明它们的不同对象 DAT | 12 |
| 对应暂存 DAT 与当前正式明文 SHA 相同 | 12/12 |
| 当前生产根 `LoganRuntime/vfs` 中这些相对路径在位 | 0/13 |
| 旧 `Assets/NTSD/Sound` 中这些相对路径在位 | 0/13 |

这使 stereo 分支成为真实的**当前 DAT 内容需求**，不应把 Q10 的 mono C053 校准当成全体声道覆盖。当前 playable 闭包中的 `battle_world.cpp::append_native_frame_sounds` 在新进入有效动作（action 0～998、未被同 action 声音锁存拦住）时按 DAT 声明顺序把 frame `sound` 放入 `WorldAudioEvent28`；此源码链只证明条件性 producer，不能证明表内17个动作已在正式根实际到达。正式 `audio_backend.cpp` 对双声道 WAV 用 `[left,0;0,right]` 对角输出矩阵，而当前 Unity 播放路径还未测该类型的解码、左右声道保真和音量/池复用。上表 0/13 仅是 Unity 当前两个生产查找根的磁盘路径状态；因尚未取得任何一条当前 336B44 正式根和 Unity 同输入下的自然 stereo 事件，不能直接宣称玩家已经听到这 13 条静音，也不能据此批量搬 13 个文件或修改 DAT。

下一定向候选可先取正式 Sakura OID1：当前 `c/saku/saku.dat` 的 frame172 静态含 `hit_j:340`，frame340 `c/saku/w/tra.wav`；对应正式 WAV 为 stereo。这个前驱只是 DAT 帧连接，不是实测玩家按键触发。应先证当前正式根/对应 playable 的自然输入确实发出该 cue，再在保留旧 `Sound` 与非战斗功能的范围内为已证 cue 做正式文件接入、Unity clip 双声道/实际 battle voice 与 AudioRenderer L/R 定向对照。若没有自然正例，改选 CSV 中别的可达帧，不把声明当运行时事件。固定完整背景的左右定位策略仍须独立裁决；Q10/Q11/Q12 和总目标开放。

本包未运行 Unity、未改生产脚本、DAT、WAV、Scene 或任何资源；仅生成上述两份只读 CSV 与本报告。上轮 C053 当前版 mono 临时声源测量保持原限定状态，未重复跑全量场景。
