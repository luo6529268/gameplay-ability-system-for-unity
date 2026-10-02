# NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001

状态：`VERIFIED`（只关闭 bit63 首次分量定位）。父项：新版336B44 G1/BATCH-04/Q07/C053 与 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

目标：在正式336B44与原Battle Scene已证同初态自然双命中/待播事件174/174零差的基础上，将 Unity 只读 `BattleHitExecutionPlan` writer 音频 bit63 的首次期望/实际分量直接导出，确定是 Count、Fingerprint、Cue、WorldX、Tick、Queued、Rejected 中的哪一项产生首差。原 Q07 ShadowCompare 两轮失败与 Q10 无 Shadow 的实际队列对照原件均保留。

权威与前置：正式根 `NTSD2.8-Logan.exe` SHA `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable `GameSession28::last_tick().audio_events`；Unity `BattleEcsHitExecutionPlan.ObserveLegacyWriterEffect`/`DifferenceMask` 与生产 `LF2Entity.QueueBattleSound`。三人安科65/action511/X580×2、自来也702/action553/X500、seed682973786、mode0，中性12tick。Q10实际待播事件10条42/42、战斗132/132。

脚本范围：`Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs` 只在已出现音频 bit63 的只读观测路径保存首次字段对照并经既有 `BattleHitExecutionPlanDiagnostics` 发布；`Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 使用新唯一 RunId 请求原 Scene 并写 JSON。诊断不改变 writer、投影判定、战斗字段或正式音效。生产 DAT/资源、Scene/Prefab、ProjectSettings及非战斗不改。

出口：先离线及原Editor脚本编译0 error，再原 Battle Scene clean/非Play 进入，12个完整生产 Driver tick 与正式源码所选132字段零差；ShadowCompare 仍应复现 bit63 并给出首次具体字段期望/实际值。退出 clean、四保护SHA稳，Change Ledger校验通过。该包只定位诊断首差，不宣称修复；后续若修投影须另立 Change。回滚须审阅本包两脚本，保护既有JSON与用户工作，不做未经批准的删除或Git恢复。

出口结果：[字段报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001/REPORT.md)。首次投影X897/生产实际X584，正式原始事件也是X584；其它六字段中仅依赖X的指纹不同。12tick战斗132/132，原Scene clean/四SHA稳。此诊断闭合，投影修正另包。
