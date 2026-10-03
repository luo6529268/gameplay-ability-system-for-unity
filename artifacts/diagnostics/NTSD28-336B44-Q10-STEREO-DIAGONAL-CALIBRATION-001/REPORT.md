# Q10 双声道对角矩阵 Unity 校准

状态：`VERIFIED_SCOPED_UNITY_SOFTWARE_MIXER`。权威目标是当前 336B44 playable `audio_backend.cpp::native_stereo_mix28` 在源规则 X500、camera X0 的两声道矩阵 `[0.75,0;0,0.25]`；小樱 `c/saku/w/tra.wav` 与君麻吕 `c/kim/w/j1.wav` 的原 Battle Scene 自然事件和正式双声道 clip 已在先前独立证据中确认。此包只测 Unity 软件混音器传递函数。

原项目 Editor 2026-10-03 在单一干净 Menu Scene Play，以临时 48 kHz 双声道正弦 PCM 分别只激励左、右输入；`AudioRenderer` 捕获两个输出，相对各自居中参考求 RMS 增益。第一轮 [`calibration-20261003-005250-610.json`](calibration-20261003-005250-610.json) 完成14组 pan 扫描；第二轮 [`calibration-20261003-005720-214.json`](calibration-20261003-005720-214.json) 追加候选参数直接验收，原始文件 SHA-256 `359A2EFE6477EB6CDE0739737B3ED3F23A7D553FD6DFC3ACB7D2827C4FE3D957`。

| 输入 | Unity `panStereo` | Unity `volume` | 左输出/居中参考 | 右输出/居中参考 | 样本帧 |
|---|---:|---:|---:|---:|---:|
| 仅左声道 | -2/3 | 0.75 | 0.7502690452 | 0 | 21,504 |
| 仅右声道 | -2/3 | 0.75 | 0 | 0.2499297554 | 21,504 |

目标非零系数最大绝对误差 `0.000270`，小于本包验收容差 `0.002`；两个非对角系数在本次捕获中为0。生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q` 为0错误/270警告，原 Editor 刷新/脚本导入并执行菜单捕获。两轮 JSON 均 `CAPTURE_COMPLETE`；第二轮记录停止捕获、销毁临时对象、恢复时间设置、四保护文件哈希不变。Editor 已退出 Play，Menu Scene `isDirty=false`。

此测量确认单个 Unity `AudioSource` 在这个双声道目标下能产生近似正式源码矩阵，**不确认**正式根 EXE 的扬声器 PCM、Unity 实际战斗 voice 经过项目 Mixer 后的设备输出，也不决定移动镜头与用户保留固定完整背景时的声像基准。当前生产 `NTSDSoundPlayer` 仍设置 `panStereo=0`、未用源规则 X 算矩阵；后续须另立战斗专用生产 Change，并用自然小樱/君麻吕事件复验。Q10/Q12及总目标保持开放。
