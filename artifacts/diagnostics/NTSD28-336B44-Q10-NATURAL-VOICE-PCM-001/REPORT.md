# Q10 小樱自然双声道战斗音效：原 Editor 软件输出 PCM

状态：`VERIFIED_SCOPED_UNITY_SOFTWARE_PCM`；Q10、Q12 与总目标仍开放。

当前正式 336B44 playable `audio_backend.cpp::native_stereo_mix28` 对小樱 `c/saku/w/tra.wav` 的源规则 X500/相机 X0 输出左75%/右25%的双声道对角矩阵。先前原 Battle Scene 已实见同一自然物理键链的 cue 与生产 voice，但仅记录了 voice 参数；临时合成信号的 `AudioRenderer` 校准也不能代替真实战斗 voice。本包在既有小樱探针加独立 opt-in PCM 菜单，只读生产输出，不改音频生产或资源。

原始结果 [`play-20261003-030255-256.json`](play-20261003-030255-256.json) 为 `PASS`。原 Battle Scene 使用 LoganRuntime，实际物理 L/W/J/K 输入使小樱在 global tick58/action340 发 `c\saku\w\tra.wav`；同tick进入播放器。正式两声道/123466采样 clip 的生产 voice 为 `panStereo=-0.6666666865`、`volume=.75`，输出组为 `Sfx`。cue 当时其他正在播放的 `AudioSource` 数为0。

`AudioRenderer` 从该真实战斗 voice 经 Unity 软件混音捕获 **14,336 个立体声输出帧、48 kHz**。输出左右 RMS 分别为 `0.1994152094 / 0.0655320796`；同一正式 WAV 早期片段自身左右 RMS 为 `0.2762528489 / 0.2723886918`。除去原 WAV 两声道本来的幅度差后，输出相对左右增益比为 **`3.000451681:1`**，与正式75:25目标3:1相差约0.0151%。这是当前小樱自然 cue 经项目 Sfx 组的 Unity 软件输出证据，不是 Windows 设备采样或正式 EXE 扬声器波形。

生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`：0 error/270 warning。原 Editor 运行时唯一 opt-in 菜单成功；旧小樱/君麻吕菜单未重跑，避免重复既有自然链。采样结束 `pcmCaptureStopped=true`，Play退出，先还原原 Battle Scene，随后切回原 `NTSD_Menu.unity` 且 `isDirty=false`。Menu、Battle、GameConfig、ProjectBattleModeConfig 的本包前后 SHA-256 逐项相同：`5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D`、`93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。

仍需按新版总表补其他正式可达单/双声道 cue、未预热异步路径、固定完整画面下的动态声像基准，以及必要的实际设备/正式 EXE 听感对照。本包只关闭小樱 X500/camera0 自然战斗 voice 的 Unity 软件 PCM 子门；不把它外推成所有音频或完整 Q10 已对齐。最终脚本状态下重新执行生成 Editor 工程：0 error/270 warning；`Tools/Validate-ChangeLedger.ps1` 返回0并报 `Change ledger validation PASSED`（1191 Records、19个当前脚本diff均有覆盖）；`git -c core.safecrlf=false diff --check` 返回0。
