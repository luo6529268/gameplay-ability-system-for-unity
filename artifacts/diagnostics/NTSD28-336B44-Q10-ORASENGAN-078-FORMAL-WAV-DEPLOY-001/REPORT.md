# Q10 鸣人螺旋丸后续 Jump：正式 `data/078.wav` 文件暂存

状态：`FORMAL_CONTENT_STAGED / UNITY_IMPORT_AND_NATURAL_VOICE_PENDING`。本包只完成一份正式战斗音效原始文件和对应Unity `.meta` 的新增；Q10、Q12和总目标开放。没有编辑DAT数值、旧Sound、Unity脚本、Scene、相机、模式或非战斗功能。

当前正式根EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。此前[当前根与playable对照](../NTSD28-336B44-Q10-ORASENGAN-JUMP-ROOT-LFR-20261004/REPORT.md)确认：55tick后续Jump输入中根动作/MP对源码110/110同态，源码tick34发一次`data/078.wav`；根公开trace没有audio字段，不声明根设备发声。正式与暂存鸣人DAT逐SHA同版。

执行前[清单](preflight.json)确认目标 `Assets/NTSD/Content/LoganRuntime/vfs/data/078.wav`及`.meta`均不存在，正式源SHA-256 `D8CAB6CE5CCF129FD689F9FD0170D06B21E04B930BE77EF9AAC9AD4B563F3560`、54,148字节；旧`Assets/NTSD/Sound/data/078.wav`另存且SHA `78688CFD046DCA0A33F36A10F589EC2ADDE83301AF58BD99FE0C9A5823A69DF5`。原Editor经MCP只读为非Play、无测试、Battle Scene clean，但编译仍停滞。`NTSDSoundPlayer`通过`File.Exists`在battle-only路径先选正式文件，`NTSD_ResourceLoader`以绝对文件URI加载，所以本轮只先做不依赖Editor程序集的原始文件新增，不调用刷新或Play。

以独占新建方式复制正式WAV，按已暂存相同格式的`data/020.wav.meta`模板新增唯一GUID `.meta`，不改原WAV字节。[执行后复核](postflight.json)显示正式/Unity暂存整文件SHA完全相同，meta GUID仅在这一文件出现一次，旧Sound SHA与四保护文件Battle、Menu、GameConfig、ProjectBattleModeConfig均同前置；随后仅规范化新meta的空字段行尾空格，[最终磁盘复核](postflight-final.json)再次确认WAV同SHA、头部22050Hz/54,104帧、保护文件全不变和新meta零尾空格。MCP再读原Editor：Battle Scene仍clean，非Play、无测试；编译仍`true`且完成时间为空。**未验证Unity导入后的实际AudioClip、自然技能pending→voice、声像或设备输出**，不把文件存在写成音频运行时通过。

下一步等原Editor编译恢复，按[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q10-ORASENGAN-078-FORMAL-WAV-DEPLOY-001.md)做定向自然Battle Scene输入、正式文件SourcePath与voice/旧通用Sound并存验收；此链只验证后续Jump，不代替用户的后续Attack→螺旋手里剑。任何撤销新增文件须按删除审计合同记录并取得需要的授权。
