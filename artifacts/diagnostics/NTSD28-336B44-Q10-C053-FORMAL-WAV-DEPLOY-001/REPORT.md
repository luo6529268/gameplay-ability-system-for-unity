# Q10/C053 正式战斗 WAV 内容接入与原 Battle Scene 限定验收

状态：`AUDIO_CONTENT_SCOPED_PASS / Q10_OPEN`。当前战斗权威为根目录正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`、对应 playable live path 和 `resources/runtime`。本包只处理既有 C053 自然双命中链实际使用的 `data\020.wav` 与 `data\067.wav`，未修改 DAT 数值、Scene、Prefab、Input Actions 或非战斗逻辑。

正式源 WAV 以新文件放入 `Assets/NTSD/Content/LoganRuntime/vfs/data`，Unity 导入生成各自独立 meta。020 的正式源/部署 SHA-256 均为 `4872410C3601F31931CCD2EB5335093ACC132AC73C0BAE7FB15AEB118A482E73`；067 均为 `D8D7B12EE94AE2EC26AEC9A93A44B65540FC69194E53E21AB44655E89451C757`。旧 `Assets/NTSD/Sound/data` 两文件 SHA 保持 `969BBFD0...B5BF` 与 `B335D602...DDEAE9`。没有删除或覆盖任何文件。播放器通过共用战斗 cue 解析在正式副本存在时使用独立源路径、缓存键和 clip，通用 `PlaySfx` 继续旧路径；未给 020/067 写生产特判。

生成的 `Assembly-CSharp-Editor.csproj` 编译 exit0、0 error。原项目 Unity Editor 导入两个 8-bit WAV 后，具名 EditMode 解码/AudioSource 播放、路径隔离和通用热路径零分配最终新鲜 3/3 PASS：正式 clip 分别为 16,413 与 31,170 采样帧，播放 voice 引用相同 clip。最终测试 job 为 `c7d837525cf84b08a7bcabcdea40de25`；编辑器测试发现数 8779 不等于执行数，实际仅执行 3 项。代码复核时将正式战斗 cue 改为按原 cue 名索引的独立字典，避免每次战斗播放拼接缓存键；以下最终 Scene 结果是在该修订之后取得。

原 `NTSD_Battle.unity` 中复用 C053 三人受控初态、生产 Driver 连续 12 tick 自然生成 OID808/875。最终第三轮独立结果 [formal-wav-03](ank580-ank580-jira500-formal-wav-03.json) 为 `SCOPED_PASS / DONE`，且与修订前第二轮 [formal-wav-02](ank580-ank580-jira500-formal-wav-02.json) 的选定音频数值相同：正式 020/067 在战斗预热目录中，首个事件 tick 有 3 条待播及 3 次实际播放，voice 上分别引用 1 个 020 与 2 个 067；相对 tick4/7/8 又分别播放 2/3/2 次，共 10 条事件、10 次播放。tick7 两条 020 和一条 067 的事件路径/源 X/全局 tick 与先前 [正式事件 42/42 审计](../NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/REPORT.md) 相同。场景正常退出、Scene clean，Battle/Menu/GameConfig/ProjectBattleModeConfig 四份保护 SHA 前后相同；旧两个 WAV SHA 也不变。

首轮 [formal-wav-01](ank580-ank580-jira500-formal-wav-01.json) `FAIL / DONE` 原件保留：探针误要求播放器属于 Battle Scene，而实际 `AppManager` 的播放器在跨场景持久对象上。首轮未进入目标 12 tick，Scene 仍正常退出且 clean。第二轮只修诊断查找条件并使用新 run ID，不覆盖首轮或旧 Q07 结果。

此证据覆盖正式文件字节、Unity 解码、同名非战斗路径隔离以及当前 C053 战斗场景的实际 AudioSource 播放。正式根 EXE 的设备波形/响度、333/666 声像矩阵、其它 cue、Player 打包与全场音画仍待；本探针未记录关闭后的 pool borrower 数。因此 Q10、Q12 和总目标保持开放。

最终保护复核为 Battle/Menu Scene、GameConfig/ProjectBattleModeConfig 和旧020/067 WAV 六项 SHA 6/6 不变。`Tools/Validate-ChangeLedger.ps1` exit0/PASSED（1148 Records、25受管代码diff），原始输出见 [账本校验](change-ledger-validation.txt)；`git diff --check` exit0。未运行全量测试或设备播放验收。
