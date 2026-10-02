# Q10 战斗声像共用出口审计（336B44）

2026-10-02。状态：`PRODUCTION_SEAM_MAPPED / DISPLAY_CAMERA_POLICY_PENDING / RUNTIME_PENDING`。本包只核当前源码和既有运行证据，未修改生产脚本、DAT、WAV、Scene 或相机，也未进行新的 Editor/设备播放。正式根 `NTSD2.8-Logan.exe` 的 SHA-256 本轮复核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

## 当前已闭合的路径

- 正式 playable：`GameSession28::step` 完成后，`main.cpp` 将 `session.camera_x()` 交给 `locked_local_battle_stereo_layout28`；`audio_backend.cpp::native_stereo_mix28` 以 `event.world_x-camera_x` 和 `{wing=333,crossfade=666,silentBefore=-333}` 生成整数左右百分比。单声道 WAV 的 XAudio2 输出矩阵为 `[L/100,R/100]`；双声道 WAV 则是对角矩阵。当前 C053 自然事件用到的正式 `020.wav`、`067.wav` 均为单声道。
- Unity 战斗写入者：`LF2Entity.QueueBattleSound`、该实体内的两个直接写入点、`LF2SpecialAttack` 与 `NTSDBattleTickSystem` 均通过 `NTSDEntityRuntime.ResolveBattleSoundWorldXInt`，优先写已初始化的**源规则 X**；未初始化时才退回物理 X。`BattleResultsWriter` 的多个直接写入点使用固定 X=400，属于另行划定的结果页行为，不能混入角色/命中声像证明。测试与压力探针的直接写入不算生产消费者。数据导向命中计划也调用同一规则坐标解析器。所有战斗写入最终进入 `SimulationWorld.QueueSound` 的 `PendingSoundEvent.WorldX`；该字段已进入快照与 checksum。
- Unity 播放端：`SimulationTickDriver.PublishPendingSoundsAfterChecksum` 复制事件，`DispatchPublishedSounds` 调用 `NTSDSoundPlayer.PresentSounds`。`PresentSound` 虽以 `WorldX` 设置 GameObject 显示位置，但通过 `PlaySfx` 进入共用 `PlayPreparedCue` 后，当前正式单文件 fallback 的 `range=0` 使声源 `spatialBlend=0`，且每次设 `panStereo=0`。播放链还可能使用 `AudioItem` 的随机音量/音高、调用间隔、3D range 和对象池；非战斗 `PlaySfx` 共享该函数，后续修改必须限定 `isBattleEvent`。
- 336B44 C053 正式源码与原 Battle Scene 的 10 条事件在已声明字段上 42/42 一致；相机 X 在所选 12 tick 为 0，正式目标左右矩阵为 64/36、75/25、65/35、63/37、78/22、66/34。因而此样本无需改变音效生产写者或 `PendingSoundEvent` 语义，首差位于播放声像出口。原始证据见 `../NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/REPORT.md` 和 `../NTSD28-336B44-Q10-C053-STEREO-CAMERA-AUDIT-001/REPORT.md`。

## 可以复用但不能越级宣称的校准

既有 `NTSD28-Q10-AUDIORENDERER-MATRIX-CALIBRATION-001` 在同项目 Unity 2022.3.62f3 Editor 上实测了**一个临时单声道样本**的 stereo 输出。其反算式是 `power=L²+R²`、`pan=(R²-L²)/power`、`voiceVolume=sqrt(power)`，其中 L/R 是目标线性矩阵增益；`power=0` 必须单独静音。旧校准的正式目标样本属于 B1E13 版本，不能据此给 336B44 的规则或全体音频贴新版 PASS 标签。`c053-mono-voice-parameter-candidates.csv` 仅将**本版** C053 10 条正式矩阵代入该已测 Unity 单声道反算式，供后续生产 RED/GREEN 与 AudioRenderer 验证；CSV 中的参数不是当前设备实测值。

现有接口在 `PresentSound` 向异步 `PlaySfx` 传递时丢失了事件源规则 X 和选择的声像相机上下文，只保留显示位置与 `isBattleEvent`。生产实现须在 battle-only 参数路径保留所需声像输入，或在该入口先算出左右目标；不能改写 `WorldX` 来承载物理坐标，因为那会改变战斗队列、快照和 checksum。

另一个必须纳入聚焦控制的读口是 `ApplyNativeBattleVolumeHostTick`：它把 `oneShotVoiceBaseVolumes[index] * nativeBattleSfxGain` 重新写给池内 voice。若声像反算的 `voiceVolume` 仅在 `PlayPreparedCue` 临时乘一次，F6/F7 调音量后会丢失矩阵增益。新包应声明池中保存的基准音量语义，并复核当前共享非战斗 voice 的边界；本审计不改变已有音量行为。

## 尚待裁决与验收

用户保留完整背景和固定画面。正式版的战斗相机可移动，因此后续声像需明确选用**Unity 完整画面的可见位置**，或**正式规则 X 加独立音频虚拟相机**。本次所选 C053 相机 X=0，两种策略在按同一视口比例换算时可能给出相同目标；不能凭这一例代替移动相机条件的选择。用户选择正在请求中；生产映射不在本包猜定。

确定策略后，应先登记独立 Task/Change/Ledger/STATE/handoff，再针对战斗写入坐标、0/满声道/交叉段整数边界、mono 与 stereo clip、已配置 `AudioItem` 的音量和 range、重叠/暂停/F5/池复用建立聚焦 RED 及邻近控制；随后只改战斗播放出口，在原 Battle Scene 以自然 cue 核事件、voice 参数、AudioRenderer 左右 PCM 与关闭回收，并单列正式 EXE 实际设备音频不可观测的边界。正式整场、所有可达 cue、Q10 与 Q12 均保持开放。

验证：本轮重新读取正式及 Unity 入口、复核根 EXE SHA，并从既有正式声像 CSV 生成 10 行候选参数。未运行编译、Unity Editor 或声音设备测试；本包无脚本改动，不把旧校准或本轮代数换算写成新版运行时通过。
