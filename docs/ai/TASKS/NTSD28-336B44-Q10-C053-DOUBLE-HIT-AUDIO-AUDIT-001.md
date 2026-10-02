# NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001

状态：`VERIFIED`（仅正式事件→原 Battle Scene 待播队列的三人受控样本；Q10 整阶段开放）。父项：新版336B44 G1 / BATCH-05 / Q10，与Q07/C053的只读ShadowCompare音频bit63首差相连；总目标`NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

目标：用同一三人自然双命中初态（OID65/action511/X580、OID702/action553/X500、OID65/action511/X580；Y0/Z400、HP/MP500、seed682973786、mode0）逐tick导出正式 playable `GameSession28::last_tick().audio_events` 的source/channel/path/world_x，与原Unity Battle Scene生产Driver的待播音效 cue/worldX/tick/顺序核对，解释Q07只读hit plan的bit63差异。初始动作是受控设置；不称玩家物理键自然选招、扬声器输出或完整Q10对齐。

脚本范围：仅新增`Tools/NTSD28Q10Diagnostics/c053_natural_double_audio_probe.cpp`离线诊断并扩展`Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs`的唯一新RunId音频采集；脚本前已建本Task/Change/Ledger/STATE/handoff。编译产物及日志写独立诊断目录；生成Unity工程目标仅Temp。正式根EXE与源、DAT、生产C#、Scene、Prefab、ProjectSettings、菜单/结果页、旧JSON一概不改。

验收：正式EXE SHA复核，诊断编译确属正式playable闭包且运行两次逐SHA重复；Unity新探针编译后原Editor单场景clean/非Play进入，12完整tick仍与正式源11字段×12零差，音效事件逐tick另记；准确报告可比和不可比事件及首差，勿强行等同SFX ID与原生channel/path；两次退出保护SHA/Scene clean；Change Ledger校验。若发现实际音频差，仅记候选，生产修复另建Task/Change与聚焦测试。回滚只审阅本包新增诊断与测试采集，删除须单独留痕批准。

实施结果：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/REPORT.md)。正式与 Unity 10 条事件的路径/X/顺序及每 tick 计数 42/42，战斗 132/132，合计 174/174 首差0。只读 ShadowCompare bit63 是诊断投影待修问题候选，非实际音频已证差异；播放/声像/设备和Q10整阶段不在本包验收范围。
