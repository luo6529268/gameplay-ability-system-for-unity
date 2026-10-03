# Q10 战斗声像矩阵生产修正前置

状态：`READ_ONLY_FORMAL_BOUNDARY_TABLE / CAMERA_POLICY_PENDING`。这份前置只给当前正式规则的整数边界和已测 Unity 音频反算候选，不宣称生产 voice 或设备 PCM 已对齐。

本轮重新核对根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；对应 playable `source/ntsd28_playable/src/audio_backend.cpp` SHA-256 `2B8C9AFE49EF8711693D3F4F510C1490CC975DD33B834626C24898AA644096CE`。`main.cpp` 在成功战斗 tick 后把 `GameSession28::camera_x()` 交给 `locked_local_battle_stereo_layout28`；`native_stereo_mix28` 使用 `world_x-camera_x`、wing333、crossfade666、silentBefore−333，逐分支整数截断，再将单声道 `[L/100,R/100]` 或双声道对角 `[L/100,0;0,R/100]` 送到 XAudio2 `SetOutputMatrix`。正式版设备上的实际输出尚未捕获。

[整数边界与 Unity 参数候选表](formal-matrix-unity-parameter-candidates.csv) 在 camera X0 下列出 −334/−333/−1/0、332/333、499/500、998/999、1331/1332、1664/1665 等边界。代表值：正式 X500 为75/25，X999 为0/100，X−333 与X1664均为0/0。X1664在最终衰减分支虽未越出几何范围，但整数截断已使右声道为0；测试不得把“范围内”误写成“可听”。该表由本轮直接抄录正式源码分支的只读计算生成，不是正式 EXE 扬声器实测。

Unity 当前 `PendingSoundEvent.WorldX` 优先来自已初始化的**源规则 X**，并进入快照/checksum；`NTSDSoundPlayer.PresentSound` 只将它用于显示位置，`PlayPreparedCue` 对 battle voice 仍设 `panStereo=0`。本轮已证的两条正式自然双声道事件（小樱 `tra.wav`、君麻吕 `j1.wav`）均为声源规则X500/相机X0，原Battle Scene实际 voice 也均为pan0/volume1，因此其播放出口与正式75/25目标确有配置首差。已有原Editor双声道 `AudioRenderer` 校准证明 `panStereo=-2/3, volume=.75` 对独占双声道输入测得 `[.750269,0;0,.249930]`；单声道校准证明反算 `power=L²+R²`、`pan=(R²−L²)/power`、`volume=√power` 可近似正式线性增益，但该单声道校准属于旧版本测试，须在当前生产链复核。

表中 mono/stereo 参数只是当前校准所支持的**候选**。正式范围内所有值、已配置 `AudioItem` 的音量/随机音高/3D range、异步加载、F6/F7 在播音量更新、声源池复用与项目 Mixer，都要经 battle-only 聚焦及原场景 PCM 检查。尤其 `ApplyNativeBattleVolumeHostTick` 会用池内 `oneShotVoiceBaseVolumes` 重写音量；后续不能只在 `PlayPreparedCue` 临时乘矩阵增益。非战斗 `PlaySfx` 与战斗共用播放器，生产改动必须用 `isBattleEvent` 收束。

固定完整背景下的声音定位仍有一个用户表现选择：按角色在当前完整画面中的位置定位，或另模拟正式移动的音频相机。两者在相机移动后会出现不同左右声像；当前已证的X500/camera0样本不能替这项选择。选择前不接生产相机读口、不改战斗队列的 `WorldX`，也不据本表关闭 Q10。脚本、DAT、WAV、Scene、相机与非战斗文件均未在本项修改。
