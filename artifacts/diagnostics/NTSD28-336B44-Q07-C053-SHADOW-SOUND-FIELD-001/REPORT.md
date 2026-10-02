# Q07/C053 ShadowCompare 音频 bit63 字段首差

状态：`VERIFIED`，仅关闭“定位首次 bit63 分量”的只读诊断包；Q07/C053 整体与 ShadowCompare 零差出口仍开放。

同三人受控初态的原项目 `NTSD_Battle.unity` 运行 12 个生产 Driver tick、自然出生双OID875和目标OID808。相对tick7两次writer `ObservationWriterEffectMismatch`，首次 attacker slot51/candidate0 的七分量期望/实值为：

| 分量 | 投影期望 / 生产实值 |
| --- | --- |
| PendingSoundCount | 1 / 1 |
| PendingSoundFingerprint | 16497791656434152434 / 16674766849052731947 |
| PendingSoundCue | `data\020.wav` / `data\020.wav` |
| PendingSoundWorldX | **897 / 584** |
| PendingSoundTick | 12 / 12 |
| QueuedSoundEventCount | 6 / 6 |
| RejectedSoundEventCount | 0 / 0 |

指纹对 cue/X/tick 逐事件求值；此条期望和实际的新增事件只有 X 不同，故指纹差随 X 差而来。现有[正式与 Unity 实际待播事件审计](../NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/REPORT.md)显示正式tick7前两条世界 X 均584，Unity实际队列也均584。投影 `ProjectStandardHurtCustomSounds` 等仍传比例后 `Runtime.XInt`，实际 `LF2Entity.QueueBattleSound` 通过 `Runtime.ResolveBattleSoundWorldXInt` 选源规则 X。这是**投影预期错误**，不能据此改实际音频或 DAT 数值。第二条 writer 的全部七字段尚未单独导出；后续共用投影修正需独立 Change 和回归。

本次新增的字符串只在已有 bit63 错误的 ShadowCompare 路径构造并保存首次差异，不改变真实 writer、投影期望、音效队列或战斗结果。[原 Scene JSON](ank580-ank580-jira500-shadow-sound-field-01.json) 为 `FIELD_OBSERVED / DONE`，12tick×11选定战斗字段[132/132同正式源](source-unity-selected-comparison.json)，两条 writer bit63 如期复现。生成工程脚本编译0 error（281 warning），原Editor已导入；Play退出、Scene clean，[四保护SHA事前](protected-before.json)/[事后](protected-after.json)完全相同。初始 action 受控、非物理键自然选招；全World、音频实际播放与整场仍待。

Change Ledger 校验 exit0、PASSED，1121 Records/当前diff 3个脚本均COVERED；[原始日志](change-ledger-validation.txt)。
