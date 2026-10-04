# Q10 鸣人持续方向＋跳跃 `c/nar/w/a7.wav` 正式战斗文件接入

状态：`FORMAL_CONTENT_STAGED / UNITY_AUDIOCLIP_IMPORT_PASS / NATURAL_VOICE_TRIGGER_ONLY`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q10`；当前336B44自然事件证据见[043回访](../../../artifacts/diagnostics/NTSD28-336B44-Q10-NARUTO-HELD-JUMP-043-REACH-001/REPORT.md)。

权威与触发：正式根 EXE SHA `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。鸣人OID2/action0/X800、先右4tick后右＋跳持续，当前playable源在tick9发`c/nar/w/a7.wav`，同输入LFR正式根exit0/PASS，动作/状态/X/Y/Z/MP/输入相位560/560一致；根公开trace无audio列，不声称根扬声器已被记录。正式/Unity staged `nar.dat`逐SHA同版，正式a7为mono/8-bit/22050Hz/3052帧、SHA `62DABF2DD6EEA06A042D806D3991625CA30822C096C3BE72B723C935ABEA4CBB`；Unity正式暂存和旧`Sound`均无此文件。

精确范围：只新增`Assets/NTSD/Content/LoganRuntime/vfs/c/nar/w/`目录及其Unity folder `.meta`、目录内`a7.wav`及`.meta`。正式WAV原样复制，不改其采样、DAT、角色图片、脚本、Scene、Prefab、相机、地图、模式或非战斗Sound。`GameConfig.BattleContentRuntimeRoot`和`NTSDSoundPlayer.GetOrPrepareCue(soundId,true)`现有共用正式文件优先路径承担加载；不加鸣人专用分支。原 Editor 当前Battle Scene clean/非Play/无测试但脚本编译停滞；只允许新增原始文件与meta并做磁盘SHA，**不调用Unity刷新、测试或Play**，也不把文件存在写成已导入或已发声。

验收：新增前记录正式源/目标不存在、四保护文件及相关旧Sound SHA；生成独立GUID并查全Assets无碰撞。新增后逐SHA证明正式/暂存WAV同字节、GUID唯一、四保护文件不变；若Editor状态安全再只读检查Scene。等原Editor编译恢复后，定向验证正式单cue加载（22050Hz/3052帧）、原Battle Scene上述自然输入tick9 pending事件→battle voice clip/SourcePath及当前Sfx路由，退出clean/零借用；必要时用方向单按控制。根正式EXE设备音频和Q10其它cue仍另证。若失败，保留原件，不擅自删除；任何撤销按`docs/ai/file-removal-audit-contract.md`记录并取得所需授权。

2026-10-04 执行结果：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-NARUTO-A7-FORMAL-WAV-DEPLOY-001/REPORT.md)。正式WAV与Unity暂存逐SHA相同，新增meta GUID唯一、旧文件与四保护SHA稳；原Editor仍clean/非Play/无测试但编译未完成，未做导入、AudioClip或自然voice验证。
2026-10-04 后续原Editor资产查询已证正式a7路径为`UnityEngine.AudioClip`并保留同GUID；自然tick9→battle pooled voice/设备输出未直接验。共用voice链已有078自然原Scene正式clip实际播放阳性，已作为Q12声音代表样本，故本Task不再单开重复音频Scene；仅在a7可复现缺声或共用播放器/路径写者改变时回访a7自身实播。见报告原始资产回执。旧“导入未验”和“Q12必跑a7”均为当时快照。
