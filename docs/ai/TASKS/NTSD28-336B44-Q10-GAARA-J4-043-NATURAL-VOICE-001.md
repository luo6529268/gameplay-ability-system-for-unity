# NTSD28-336B44-Q10-GAARA-J4-043-NATURAL-VOICE-001

状态：`VERIFIED / SCOPED_NATURAL_VOICE`。归属当前336B44总表 Q10 的单次新增场景出口；既有Q12同版重进已限定通过，未重复运行。

权威与触发：[正式可达性报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-GAARA-043-NATURAL-REACH-001/REPORT.md)中，正式OID16/action0/seed682973786/mode0、X800/Z650 vs 远距OID7 的40tick自然防2→右2→攻2，于tick13/15发j4/043。正式两WAV已仅在LoganRuntime暂存并由原Editor导入为AudioClip；声音生产链尚无同条件Play证据。

精确脚本范围：仅新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q10GaaraJ4043NaturalVoiceProbeEditor.cs` 与其 `.meta`。参考现有078探针的Play clone安全前置和有序收尾，但仅录本案40tick的action、PP、phase、两pending cue、对应正式clip、池化voice计数与`AudioSource.isPlaying`；不得修改现有078探针或生产播放器，不使用旧Sound回退充当正式通过。输入按当前共用映射采用逻辑包Attack/Right/Jump承载正式Defend/Right/Attack；若动作链不同先查共用映射，不加我爱罗特判。原始输出只新建，不能覆盖。

安全前置与出口：用户已确认原Editor保存且空闲，执行时仍须重新只读核对它是唯一原项目、非Play/非编译/无测试且仅加载clean Battle Scene，并冻结Battle/Menu/两配置/两WAV哈希。仅一次40tick Play，逐tick对当前源CSV的phase/action/PP/位置等可比字段定位首差；至少自然tick13/15 pending与正式clip、池化voice同tick增长且对应AudioSource播放方可标限定通过。退出确认非Play、Scene clean、World解绑/活动池零及保护哈希不变；外部并发修改则标`INCONCLUSIVE`并保留现场。只运行生成C#工程编译/原Editor导入及这个定向Play，不跑全套。若发现共用真实首差，另开最小生产修复包。回滚仅前向更正并保留原始记录；任何删除/覆盖另按文件操作审计及授权。

执行结果：[原Scene报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-GAARA-J4-043-NATURAL-VOICE-001/REPORT.md)。原Editor导入、生成工程0错；40tick正式动作/phase/PP/X/Y 200/200同。tick13/15对应正式clip各同tick待播、池化play+1且AudioSource播放中；退出clean、World解绑/Pool0、保护哈希稳定。Z因项目地图边界不同，tick27对手动作/音频队列不可同态裁决，详内建cue诊断。此Task只关闭j4/043自然声音链；设备PCM/正式EXE扬声器及其它cue未知。
