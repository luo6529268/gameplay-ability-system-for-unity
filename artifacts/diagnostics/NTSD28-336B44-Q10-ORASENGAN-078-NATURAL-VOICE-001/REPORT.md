# Q10 鸣人后续 Jump 的正式 078 自然事件与战斗 voice

状态：`RUNTIME_PENDING / SCOPED_NATURAL_VOICE_PASS`。本包验证一条 55 tick 离散输入的原 Unity Battle Scene 战斗音频链；Q10、Q12 与总目标仍开放。本链是螺旋丸持续帧的后续 **Jump**，不是用户报告的后续 **Attack→螺旋手里剑**。

权威是用户选定的根目录正式 `NTSD2.8-Logan.exe`，SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，及其 playable 构建闭包。沿用[当前根/源码 55 tick 对照](../NTSD28-336B44-Q10-ORASENGAN-JUMP-ROOT-LFR-20261004/REPORT.md)：正式根与源码的动作/MP 110/110 同；源码在 tick34 发出一次 `data/078.wav`。根公开 trace 不导出 audio，故本包不宣称根扬声器波形已直接对照。

原 Editor 使用唯一 `NTSD_Battle.unity`，在 Play clone 中配置 OID2 鸣人 X500/Z650、OID7 远距对手 X1200/Z650、seed `682973786`、mode0，并经生产 `SimulationTickDriver.StepOneTick` 提交与源码相同的防2 tick→右2 tick→跳2 tick→空到33→跳34–35 离散输入。初始全局 tick5，结束全局 tick60，共 55 个测量 tick。[Unity 原始逐 tick 记录](naruto-078-scene-01.json)和[机械对照](comparison.json)显示输入相位、动作、`Health.PP` 对正式 MP、组合键状态及 078 事件共 **275/275 值相同、首差 0**。tick7 的 PP 为 350，tick34 为 250；记录中另一列 `Runtime.MP` 仍为 500，它不是本路径实际扣除的 PP，不能拿来宣称战斗 MP 首差。

生产战斗 cue 的 `SourcePath` 是 `Assets/NTSD/Content/LoganRuntime/vfs/data/078.wav`；Unity 导入的 clip 为 54,104 samples、单声道、22,050 Hz，原 WAV SHA-256 `D8CAB6CE5CCF129FD689F9FD0170D06B21E04B930BE77EF9AAC9AD4B563F3560`。非战斗同名 cue 仍指 `Assets/NTSD/Sound/data/078.wav`，SHA-256 `78688CFD046DCA0A33F36A10F589EC2ADDE83301AF58BD99FE0C9A5823A69DF5`。Unity tick34 自然排出一条 078 待播事件，同 tick `PooledOneShotPlayCount` 增 1，且有一条 `AudioSource` 使用该正式 clip 并报告 `isPlaying=true`。这是生产播放器中的实际 voice 证据；设备 PCM/用户听感尚未测量。

原 Editor 刷新后导入了新增探针及 `.meta`；`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` 在最终脚本版本上退出 0、0 error（299 warnings）。第一次菜单请求与另一轮 Play 重叠，入口安全拒绝；第二轮采集了完整 55 tick，结果文件在后续 Editor 刷新时完成写出。探针随后补了 Play 被外部提前结束时的 `INTERRUPTED` 收尾，未覆盖结果原件。六个受保护文件（Battle/Menu Scene、GameConfig、ProjectBattleModeConfig、正式/旧 078 WAV）在**本次 Play 结果快照内**前后哈希相同，且退出非 Play。

[独立退出残留记录](naruto-078-postplay-01.json)显示原 Scene Driver 1、绑定 World 0、Scene Pool 0，Scene 当时 `isDirty=false`。但残留检查时 Battle Scene 磁盘 SHA 已从本次 Play 结果中的 `1748F4EE…A3F70` 变为 `6D983C01…04C2C`，同期存在其它 UI/Scene 工作区改动，写入者与改动时点未证。因此“跨检查窗口的 Scene 文件稳定”只能记 `INCONCLUSIVE`，不把外部变动归因于本探针，也不回退/保存他人内容。

未验出口：真实物理键→Action→Provider 的端到端路径、后续 Attack 分支、其他 cue、设备 PCM 与声像、Q10 全量可达事件和 Q12 最终集成。本包未修改生产战斗脚本、DAT 数值、角色图、正式或旧音频字节、Scene、Prefab、模式或非战斗逻辑。
