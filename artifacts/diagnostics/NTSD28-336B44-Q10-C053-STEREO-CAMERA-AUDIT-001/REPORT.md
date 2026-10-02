# Q10/C053 正式战斗声像相机坐标首差

2026-10-02；状态：`SOURCE_CAMERA_SCOPED_PASS / UNITY_STEREO_FIRST_DIFFERENCE_STATIC`。这是同一三人受控初态、12 tick、10 条自然产生音频事件的声像审计，非生产修复或实际扬声器波形证书。根正式 EXE SHA-256 实测 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

正式 playable 构建闭包包含 `ntsd28_playable/src/main.cpp`、`audio_backend.cpp` 和 `game_session.cpp`（`scripts/build.ps1 -Target playable` 的源清单）。`main.cpp` 在每个战斗逻辑 tick 完成后，以 `session.camera_x()` 建立 `locked_local_battle_stereo_layout28`；`audio_backend.cpp::native_stereo_mix28` 按事件 `world_x-camera_x` 与布局 `{cameraX, wing333, crossfade666, silentBefore-333}` 计算左右整数百分比，XAudio2 对 mono WAV 写 `[left/100,right/100]` 输出矩阵。当前正式 020/067 WAV 均为 mono、22050 Hz、8-bit。前一包已证正式源码与原Unity Battle Scene生产队列10事件的路径、世界X、tick和顺序42/42一致，本包不重复运行Unity场景。

在既有离线 C053 诊断中新增**仅编译时启用** `NTSD_Q10_STEREO_CAMERA_AUDIT` 的 `camera_x` 最后一列。使用之前保存的28 Core+4 host编译参数与正式 source/runtime，新宏编译 exit0、stderr0；两次12 tick输出各18行且原始 SHA 同为 `9DFB159A9CF65A289FCA181E4C6D873B5A438CCFCCD886372CAB6C9750FE9772`。剥掉新列后，与原 `source-audio-run-01.csv` 逐字节 SHA 同为 `3E4CA682EA6939B5FBE48E9EECD86370F90E80F149BDC7B8237AE873778AEF42`；关闭宏重新编译/运行的[默认控制CSV](source-default-control.csv)亦为该 SHA，旧默认诊断格式与战斗样本未变。[原始相机CSV](source-stereo-run-01.csv)、[逐事件声像矩阵](formal-stereo-mix.csv)、[机器摘要](comparison-summary.json)保留。

当前受控链相机X在12/12 tick均为0，10条事件按正式源码整数分支得到：

| 相对 tick | 声音世界 X | 正式左/右百分比 | 条数 |
| --- | ---: | ---: | ---: |
| 1 | 578 | 64/36 | 2 |
| 1 | 503 | 75/25 | 1 |
| 4 | 572 | 65/35 | 2 |
| 7 | 584 | 63/37 | 2 |
| 7 | 485 | 78/22 | 1 |
| 8 | 564 | 66/34 | 2 |

Unity 生产 `NTSDSoundPlayer.PresentSound` 将队列世界X转成显示位置，但 `CreateFallbackAudioItem` 对当前正式单文件 cue 设 `range=0`；`PlayPreparedCue` 因此设 `AudioSource.spatialBlend=0`、`panStereo=0`，没有使用相机X或写出正式左右矩阵。前一包原Scene/Player证明 clip 确实播放，故这是**已执行声音链的配置首差**；Unity设备最终L/R PCM未采，不能把 `panStereo=0`直接写成设备上精确的50/50增益，也不能从源码诊断声称已录到正式EXE实际扬声器波形。默认音量100时正式SFX衰减为0百dB、Unity fallback音量1，此样本未证音量首差。

下一生产包必须是通用战斗音频表现入口，不能对020/067或安科/自来也特判。先确认固定完整背景/D-024比例例外下使用何种显示相机/坐标，再验证Unity可实现的mono与stereo左右矩阵、叠音、F5/暂停、声音停止和真实设备输出；不能直接给 `panStereo` 填一个猜测数值，或把原版跟随镜头接回项目相机。本包未改Unity生产、DAT、角色图、Scene、Prefab、非战斗声音或正式源，Q10/Q11/Q12和总目标开放。

`Tools/Validate-ChangeLedger.ps1` exit0、[完整结果](change-ledger-validation.txt)首行 `Change ledger validation PASSED`（1150 Records）；`git diff --check` exit0。未启动Unity Editor/Player，也未运行无需此只读源码诊断的全量场景案例。
