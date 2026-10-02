# Q10 自然双声道音效播放矩阵首差（336B44）

2026-10-03。状态：`SOURCE_MATRIX_AND_UNITY_VOICE_CONFIG_FIRST_DIFFERENCE / DEVICE_PCM_PENDING`。这是现有正式源码、根回放及原 Battle Scene Play 原件的只读复核；本包没有运行新的 Unity Play、录制设备 PCM，或修改生产脚本、DAT、WAV、Scene、相机及非战斗文件。

权威根 `NTSD2.8-Logan.exe` 的 SHA-256 本轮复核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。对应 playable 的 `main.cpp` 将 `GameSession28::camera_x()` 交给 `locked_local_battle_stereo_layout28`；`audio_backend.cpp::native_stereo_mix28` 使用 `world_x-camera_x`、wing 333 和 crossfade 666。在 `layout` 已提供且 mastering output 为双声道的正式分支，对双声道 WAV 的 `submit` 路径将 `[left,0;0,right]` 交给 XAudio2 的 `SetOutputMatrix`，各整数百分比除以 100。此处只描述源码闭包中的目标矩阵；未采正式 EXE 的实际扬声器输出，也未确认用户设备是否进入该双声道分支。

| 已证自然事件 | 正式源码事件与相机 | 正式双声道目标 | 原 Unity Battle Scene 已观察播放配置 |
| --- | --- | --- | --- |
| 小樱 HP100，防 L 两 tick→上 W 两 tick→攻 J 两 tick→action172 后新跳 K | 相对 tick24，`c/saku/w/tra.wav`，声源规则 X500、相机 X0；[源码逐 tick](../NTSD28-336B44-Q10-SAKURA-STEREO-NATURAL-REACH-001/source-run-05/source-events.csv)，根回放选定动作/MP/相机165/165同态 | `right=(500-333)*100/666=25`，left75；双声道对角矩阵 `[0.75,0;0,0.25]` | [原 Editor Play JSON](../NTSD28-336B44-Q10-SAKURA-NATURAL-VOICE-001/play-20261002-134026-108.json) `PASS`、voice播放中、正式2声道123466采样、`voicePan=0`、`voiceVolume=1` |
| 君麻吕，防 L 两 tick→上 W 两 tick→跳 K 两 tick | 相对 tick6，`c/kim/w/j1.wav`，声源规则 X500、相机 X0；[源码逐 tick](../NTSD28-336B44-Q10-KIM-J1-NATURAL-REACH-001/run-02/source-events.csv)，根回放选定动作/MP/相机165/165同态 | 同上，双声道对角矩阵 `[0.75,0;0,0.25]` | [原 Editor Play JSON](../NTSD28-336B44-Q10-KIM-NATURAL-VOICE-001/play-20261002-142725-116.json) `PASS`、voice播放中、正式2声道80454采样、`voicePan=0`、`voiceVolume=1` |

两份 Unity WAV 本轮原始 SHA 分别仍为 `A6D36A499DBEAB218690BC3FDEE5071E5B97165D5DB34427116A3CDE8660EBC7` 和 `86A7F48012EF4A2C1E3E578CCCE37E95F033D1BEEB7C47A8444B651A3D33BD65`，与各自正式内容部署记录一致。`NTSDSoundPlayer.PresentSound` 当前仅把 `PendingSoundEvent.WorldX` 换为显示位置，`PlayPreparedCue` 每次把 `AudioSource.panStereo=0`；正式单文件 fallback 的 `range=0` 使 `spatialBlend=0`。在这两条自然事件里，相机 X=0，因此无须先决定移动相机与固定完整背景的全局声像策略，就能确定**当前 Unity 播放端没有设置正式版所需的独立左右矩阵**。这不等于已经测量出 Unity 设备上的实际 `[1,1]` 增益：AudioSource、Mixer 和设备输出需要 AudioRenderer/PCM 验证。

已有 C053 校准只覆盖临时单声道信号，不能证明 `panStereo` 对双声道素材可实现对角矩阵。后续独立 Q10 子包应先在当前项目原 Editor 对左声道独占、右声道独占的临时双声道信号测交叉串音及各增益，再按结果选择 battle-only 共用播放出口；使用上述两条真实已达 voice 验证声像、F6/F7 音量更新、池复用和停止回收。全局音频相机策略需另按用户选择处理移动相机条件。Q10、Q12 与总目标保持开放。
