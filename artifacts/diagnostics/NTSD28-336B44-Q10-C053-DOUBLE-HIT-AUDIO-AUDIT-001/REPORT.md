# Q10/C053 自然双命中音频事件审计

结论：`VERIFIED` 仅限这组三人受控初态的**正式事件→Unity 待播队列**。336B44 playable `GameSession28::last_tick().audio_events` 与原项目 `NTSD_Battle.unity` 的生产 Driver `PendingSounds` 在相对 tick 1～12 共 10 个事件的顺序、路径和世界 X 相同；12 个计数加 10×3 个事件字段共 42/42，连同 11 个战斗字段×12 tick 共 **174/174，首差为空**。[逐字段机器结果](source-unity-audio-comparison.json)、[正式原始事件](source-audio-run-01.csv)、[Unity 原 Scene 结果](ank580-ank580-jira500-audio-scene-01.json)。

同初态：两名 OID65 安科初始 action511、X580，OID702 自来也初始 action553、X500；Y0/Z400、HP/MP500、seed682973786、mode0，之后中性输入。所有 OID875/808 由正式资源与生产 OPoint 自然生成。Unity 在全局 tick5 暂停后测相对 12 tick，因此事件 tick 对照使用 `UnityGlobalTick = SourceTick + 5`。初始 action 由探针受控指定，不能称玩家物理键自然选招。

| 相对 tick | 正式版和 Unity 共同观测到的待播事件（按顺序，路径 @ 世界 X） |
| --- | --- |
| 1 | `data\067.wav@578`、`data\067.wav@578`、`data\020.wav@503` |
| 4 | `data\067.wav@572`、`data\067.wav@572` |
| 7 | `data\020.wav@584`、`data\020.wav@584`、`data\067.wav@485` |
| 8 | `data\067.wav@564`、`data\067.wav@564` |

正式 tick7 前两条事件的 source 为 `definition_weapon_broken`（原始枚举值4），第三条为 `frame_sound`（值1），channel 均为-1；Unity `PendingSoundEvent` 没有一一对应的 source/channel 字段，因此不把这两列算入相同字段，也不从此推断播放声道、音量、左右声像或设备可听结果。其余 tick 无事件。

正式根 EXE SHA-256 再核为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。新增离线诊断以正式 playable 闭包编译 exit0、stderr0；两次源码运行各18 CSV 行、exit0、相同 SHA `3E4CA682EA6939B5FBE48E9EECD86370F90E80F149BDC7B8237AE873778AEF42`。测试采集脚本的生成 Editor 工程编译 0 error；原 Editor 刷新后编入 `Assembly-CSharp-Editor.dll`，真实 Battle Scene Play `SCOPED_PASS / DONE`、12 tick、退出 Play、Scene clean。Battle/Menu/GameConfig/ProjectBattleModeConfig 四份保护 SHA 前后相同，见[前](protected-before.json)/[后](protected-after.json)。

Q07 ShadowCompare 两轮在 tick7 报 `ObservationWriterEffectMismatch`，掩码只有音频 bit63；本次实际队列与正式音频完全相同，故不能把旧报错解释为生产音效首差。静态代码显示投影 `ProjectStandardHurtCustomSounds` 等仍向 `ProjectQueuedSound` 传 `Runtime.XInt`（比例后物理 X），而真实 `LF2Entity.QueueBattleSound` 使用 `Runtime.ResolveBattleSoundWorldXInt`（源规则 X）；**投影坐标是诊断首差候选原因，尚未逐字段证明 bit63 的具体分量**。Q07 内部 hit-plan 观测仍开放，若修诊断投影须另立 Change。Q10 的实际 clip/voice/声像/停止/扬声器，以及其它角色和声音也仍开放。

本包只新增 Tools 诊断脚本并修改测试探针；正式生产脚本、DAT、图片、Scene 和非战斗流程未改。独立结果文件保留；后续撤销或删除须留痕并按仓库规则处理。

Change Ledger 校验 exit0、PASSED（1120 Records，当前 diff 两个脚本均 COVERED）；`git diff --check` exit0、无空白错误。[校验原始输出](change-ledger-validation.txt)。
