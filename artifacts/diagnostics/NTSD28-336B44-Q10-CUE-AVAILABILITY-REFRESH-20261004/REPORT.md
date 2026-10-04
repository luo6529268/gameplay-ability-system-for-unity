# Q10 正式帧音效路径现存性回访（2026-10-04）

后续同日当前权威回访已取得鸣人后续 **Jump** 的 `data/078.wav` 自然源码事件与正式根 action/MP 对照，因此 Q10 的下一单 cue 优先级已改为 `078`；本报告中的 `043` 仍是只读静态候选。[078 回访](../NTSD28-336B44-Q10-ORASENGAN-JUMP-ROOT-LFR-20261004/REPORT.md)。

状态：`READ_ONLY_INVENTORY / NATURAL_REACHABILITY_PENDING`。本次仅检查磁盘、PCM 和现有播放器查找链；没有复制、替换或删除 WAV，没有修改 DAT、角色图片、Scene 或战斗脚本。Q10、Q12 与总目标继续开放。

当前正式根 `NTSD2.8-Logan.exe` 的 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本次使用既有 `NTSD28-Q10-FRAME-SOUND-ENTRY-READINESS-001/formal-indexed-cue-paths.csv` 作为**历史词法索引**，其 SHA-256 为 `C2331CB3423CEFCCEC78FD8A53BE61E93937FA2423D8F07E3C2593808D79ACCB`；没有重新解析当前全部 DAT，也不能将索引行数当作当前自然可达事件数。原始索引 976 行，合并 6 个大小写别名后为 970 个物理路径。

逐路径现存性结果见 [inventory.csv](inventory.csv)、[summary.json](summary.json) 和 [case-aliases.json](case-aliases.json)：正式 `resources/runtime/vfs` 中 970/970 存在；Unity `Assets/NTSD/Content/LoganRuntime/vfs` 仅 4/970 已暂存，且四份均与正式文件逐字节相同：`c/kim/w/j1.wav`、`c/saku/w/tra.wav`、`data/020.wav`、`data/067.wav`。Unity 旧 `Assets/NTSD/Sound` 有 68/970 同路径，其中 `data/m_ok.wav` 与正式文件逐字节相同，其余 67 份原文件字节不同；900 路径在两个 Unity 查找根均不存在。对这 68 份 WAV 解码容器并比较 PCM，27 份 PCM 相同，41 份不同，见 [pcm-overlap.csv](pcm-overlap.csv)。文件字节不同但 PCM 相同不能直接宣称玩家听到差异。

`NTSDSoundPlayer.GetOrPrepareCue(soundId, true)` 的现行路径优先使用暂存的正式战斗文件，缺失时回退旧 `Sound` 路径；此处只核查静态查找配置，不是实际预热、播放或扬声器证明。当前鸣人正式/Unity 同版 `nar.dat` 的 frame 641 `running2` 声明 `data/043.wav`；它属于上述 PCM 不同的旧 Sound 重叠项：两边均为单声道 22050 Hz、13641 采样帧，但 PCM SHA-256 分别是正式 `D3705296112E6B1715677BF19B5CBE9B473C292EAD5E6CE9F20722EE63F42A11` 与旧文件 `F2CB5625B474AED49A64F77910D24823360C64183960CBDEF944BB92B97A502C`。**frame 声明不证明当前正式 EXE 或 Unity 在自然战斗中发出该事件**；下一步须先取得同输入自然帧事件，再就这一条检查正式文件接入、Unity clip 和播放。

同样，P-08 样本涉及的 `data/007.wav` 虽然两端 WAV 原文件字节不同，PCM 完全相同（SHA-256 `1A4317C153D6DC3652F55BA29167989069178AB9B8A58811390F2A3DBDD74D4F`，单声道 22050 Hz、2099 采样帧）；本次不把它列为已证音色首差。既有螺旋丸后续 Jump 的 `data/078.wav` 旧诊断包含 B1E13 身份口径及当前源码事件，但尚无本轮当前正式根音频输出与原 Unity 自然同态证书，也不据此批量部署。

原 Editor 当前 Scene/编译状态另受未保存内容与后台编辑影响；本次没有启动 Unity 测试或 Play。磁盘现存性和 PCM 结果不关闭 Q10 的自然可达、预热失败、实际播放、声像及设备输出门。
