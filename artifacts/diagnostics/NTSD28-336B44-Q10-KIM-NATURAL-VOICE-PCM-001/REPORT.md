# Q10 君麻吕自然战斗音效：原 Editor 软件输出 PCM

状态：`VERIFIED_SCOPED_UNITY_SOFTWARE_PCM`。当前正式根 `NTSD2.8-Logan.exe` SHA-256 再核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式 playable `audio_backend.cpp::native_stereo_mix28` 在已证的君麻吕 `c/kim/w/j1.wav` 自然事件、源规则 X500/音频相机 X0 条件下给出双声道左75%/右25%对角输出目标。既有正式源/根与原 Battle Scene 物理 L/W/K 输入已经证明相对 tick6 的 action311、PP250 和播放中的双声道 voice。本次只测这条生产 voice 经 Unity 项目 Sfx Mixer 后的软件 PCM。

原始结果 [`play-20261003-033009-390.json`](play-20261003-033009-390.json)，SHA-256 `8149FEC24AB7F9163E705CFDDBAE7E6639F61B287C624834430312DC01DA90B0`，为 `PASS`。物理输入经项目 Input System/生产 Driver 于 global tick40 发 `c\kim\w\j1.wav`，同 tick 进入声音回调；末动作311。正式两声道、80,454采样 clip 已赋给播放中的池化 voice，`panStereo=-0.6666666865`、`volume=0.75`、输出组 `Sfx`。cue 当时其他正在播放的 AudioSource 为0。

Unity `AudioRenderer` 捕获该真实战斗 voice 的14,336个立体声输出帧，输出采样率48kHz。输出左右 RMS 为 `0.2590646384 / 0.0861605983`，同一正式 WAV 早期片段本身左右 RMS 为 `0.3413480032 / 0.3405978506`。按源 WAV 左右幅度比校正后，实际输出相对左右增益为 **`3.000156898958169:1`**，与正式75:25目标3:1的相对误差约0.00523%，在事先声明的2.55–3.45区间内。`pcmCaptureStopped=true`。

验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 返回0，270警告、0错误；原Editor脚本刷新后 `Assembly-CSharp-Editor.dll` 时间晚于脚本。原项目唯一Editor在干净Battle Scene执行新opt-in菜单，Play完成后恢复干净Battle Scene，再切回此前的干净Menu，最终 idle/non-Play/no test。Battle/Menu/GameConfig/ProjectBattleModeConfig 磁盘SHA与本包前的已有受保护基线相同，依次为 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、`5D79DBB7F3C6E9FF790413A8D6C0D9942A093F5D9E1B69FC351468C05EC0052D`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`；四文件 Git 状态无新增变化。Unity MCP 并发读取产生两条已知 disposed-client 桥接错误，随后顺序读取、Play与结果均成功；它们不是脚本编译或战斗失败。

本证据只关闭君麻吕 X500/camera0 自然 cue 的 Unity 软件PCM子门。没有录到正式EXE扬声器或Windows设备输出，也没有验证其它位置的动态音频相机基准、其它cue、异步路径或整体Q10/Q12。生产、DAT/WAV、Scene、相机、菜单与非战斗逻辑没有改动。
