# Q10 battle-only 左右声像矩阵：生产出口与原 Battle Scene 见证

状态：`SCOPED_NATURAL_STEREO_VOICE_PASS / PCM_AND_CAMERA_POLICY_PENDING`；Q10、Q12 及总目标开放。

当前正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。playable `audio_backend.cpp::native_stereo_mix28` 对 `event.world_x-camera_x` 按 wing333/crossfade666 求整数 L/R，并对单声道用1×2、双声道用对角2×2输出矩阵。已证正式自然小樱 `c/saku/w/tra.wav` 的源规则 X500/正式相机X0目标为75/25；旧原 Battle Scene voice 是pan0/volume1。用户保留 Unity 完整背景和固定相机；本包按当前画面位置声像处理，使用**已在队列中的源规则 X、音频相机X0**，使声像位置与 D-024 的同一画面比例相符。若用户改选原版音频虚拟相机，生产输入策略仍需调整；不得把固定画面例外称为正式移动相机声像等价。

仅改 `NTSDSoundPlayer.cs` 与既有 `SoundPresentationDispatchEditorTests.cs`。`PresentSound` 的源规则 X 在 battle-only 的已加载/异步路径传到播放出口；正式整数左右比例和 Unity mono/stereo 的不同反算在共用出口执行。battle voice 为2D，矩阵增益进入现有池的 base-volume 槽，确保 F6/F7 重写在播音量时保留左右矩阵；非战斗 `PlaySfx` 仍按既有 pan0/配置 range 与音量运行。事件 `WorldX`、逻辑/快照/checksum、Unity相机、DAT、WAV、Scene、Prefab、AudioItem 资产和非战斗脚本均未修改。

测试先行：原Editor RED job `b19ffd48a73c43d5aa68ce45b578ec43` 的12项均按预期失败（11项缺正式整数矩阵方法，1项X500 mono旧pan0）。生产修正后首轮 GREEN job `449bd8c00220463899c8af71c32e6260` 整数边界11/11通过，voice项在测试自身误把99%音量整数dB预期写成−37处失败；生产实际应用−38，与原 `((99−100)*0xED8)/100` 一致。仅校正夹具后，job `6e63989004214a3bb9b98b46db37f68e` **12/12 PASS**：包括X500 mono pan−0.8/volume√0.625、双声道pan−2/3/volume.75、99%在播调音量以及池切回非战斗pan0控制。相邻四项 job `6d99d6f1f5a04d1e9c1c4fad37ce6b92` **4/4 PASS**，覆盖正式 WAV 解码、池上限/复用、steady-state分配与原生音量入口。生成 Editor 工程最终编译0 error/270 warning，原Editor完成脚本导入。

原项目原 `NTSD_Battle.unity` 通过既有小樱自然输入探针运行一次，结果 [`play-20261003-024350-823.json`](../NTSD28-336B44-Q10-SAKURA-NATURAL-VOICE-001/play-20261003-024350-823.json) `PASS`：正式暂存 `LoganRuntime`、防→上→攻→action172后跳的原生输入链，relative tick24/global tick58 将 `c/saku/w/tra.wav` 以源X500排队并同tick交给播放器；实际 clip 2声道/123466采样，voice正在播放、`panStereo=-0.6666666865`、`volume=0.75`，与正式75/25的 Unity校准参数一致。pool播放数3→4。此次验证的是**生产 voice 参数和事件交接**，不是正式根 EXE 或当前 Unity 设备 PCM 的直接波形比较。

Play 已退出；Editor 切回原 `NTSD_Menu.unity`，非Play且Scene clean。Menu、Battle、GameConfig、ProjectBattleModeConfig 四保护文件的 [`before`](../NTSD28-336B44-Q10-STEREO-MATRIX-PREFLIGHT-20261003/protected-before.json)/[`after`](../NTSD28-336B44-Q10-STEREO-MATRIX-PREFLIGHT-20261003/protected-after.json) SHA-256均不变。没有覆盖旧Sakura结果，新的 runId 文件独立保存。Change Ledger 验证与最终 diff 检查见本包后续结果，不把本句当作已执行记录。

仍需用实际战斗 voice 经过项目 Mixer 的 `AudioRenderer` 或设备输出验证左右 PCM，且需覆盖当前内容可达的单声道自然音效、配置 `AudioItem` 的额外音量/range、异步尚未预热路径以及移动相机时用户声像策略。已有 mono/stereo临时信号校准证明参数可行，但不能取代这些生产出口。正式 EXE 扬声器 PCM 未采，Q10与Q12不关闭。

交付检查：`Tools/Validate-ChangeLedger.ps1` 返回0、`Change ledger validation PASSED`（全仓1190个Record/18个当前脚本diff；旧Record的非当前diff警告不属于本包失败）；`git -c core.safecrlf=false diff --check` 返回0。此次未运行全角色/全场景回归，现有证据只覆盖上文目标出口。
