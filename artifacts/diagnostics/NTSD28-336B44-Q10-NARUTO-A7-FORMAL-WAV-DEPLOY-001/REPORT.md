# Q10 鸣人持续方向＋跳跃：正式 `c/nar/w/a7.wav` 文件暂存

状态：`FORMAL_CONTENT_STAGED / UNITY_IMPORT_AND_NATURAL_VOICE_PENDING`。只新增正式战斗WAV和Unity文件夹/音频 `.meta`，未改DAT、旧Sound、脚本、Scene、相机、地图、模式或非战斗功能。Q10/Q12/总目标开放。

当前336B44正式源码在鸣人先右4tick、之后右＋跳持续80tick的tick9发`c/nar/w/a7.wav`，双源码运行CSV/LFR逐字节同版；正式根同LFR进程exit0/PASS，动作/当前DAT状态/X/Y/Z/MP/输入相位560/560零差。根公开trace没有audio字段，不声明其设备实际声音。[自然入口与范围](../NTSD28-336B44-Q10-NARUTO-HELD-JUMP-043-REACH-001/REPORT.md)。正式/Unity staged `nar.dat`逐SHA同版。

[前置清单](preflight.json)确认`Assets/NTSD/Content/LoganRuntime/vfs/c/nar/w/`、目录meta、`a7.wav`及其meta均不存在；正式源`resources/runtime/vfs/c/nar/w/a7.wav` SHA-256 `62DABF2DD6EEA06A042D806D3991625CA30822C096C3BE72B723C935ABEA4CBB`、3096字节、mono/8-bit/22050Hz/3052帧，旧`Assets/NTSD/Sound`没有同路径文件。原Editor只读仍为非Play、无测试、Battle Scene clean、脚本编译停滞。

以独占新建方式复制正式WAV，参照现有`c/kim/w.meta`及同格式`data/020.wav.meta`新增两个独立GUID，不更改正式WAV字节。[执行后复核](postflight.json)显示正式/暂存WAV逐SHA相同，两个GUID在全Assets `.meta` 各仅出现一次；随后仅规范化新meta的空字段行尾空格，[最终磁盘复核](postflight-final.json)再次确认WAV同SHA、头部22050Hz/3052帧、所有保护文件不变、新meta零尾空格。四保护文件与先前新增`data/078.wav`均同前置SHA。原Editor再次只读仍Battle Scene clean、非Play、无测试、编译未完成。按当前播放器共用battle-only路径，这份正式文件应在后续预热时优先于旧Sound被选取；**真实Unity AudioClip导入、自然tick9 pending→voice、Sfx混音及设备声音尚未验收**。

下一步等原Editor编译恢复后执行[定向Task](../../../docs/ai/TASKS/NTSD28-336B44-Q10-NARUTO-A7-FORMAL-WAV-DEPLOY-001.md)的自然输入和voice/SourcePath检查。两个新文件及文件夹不因运行时待验而自动撤销；若需删除，按项目文件操作审计合同逐文件记录。
