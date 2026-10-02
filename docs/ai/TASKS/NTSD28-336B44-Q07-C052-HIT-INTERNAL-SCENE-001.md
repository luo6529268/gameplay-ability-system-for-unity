# Q07/C052 effect21 逐候选原 Scene 内部观测

状态：`VERIFIED / SCOPED_HIT_SEQUENCE_PASS`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-04/Q07/C052；父 C052/Q07 均开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-HIT-INTERNAL-SCENE-001/REPORT.md)。

正式权威：336B44 playable `BattleWorld28::classify_ordinary_hit_eligibility` 与正式 OID211/action161 的完整 `GameSession28::step`；既有正式源码 `source-hits.tsv` 在 tick1 依次记录 slot1 applied、slot1 因正 rest rejected/不终止、slot2 applied，正式根三实体 LFR 与原 Scene 的 tick 尾动作/HP 已限定同态。Unity 现有 `BattleHitCandidateSequenceRunner` 已补 rest==0 共用门，但原 Scene 探针未导出逐候选内部结果，不能仅从 tick 尾两目标 HP420 倒推第二候选拒绝原因。

范围：只扩展现有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C052PositiveRestBattlePlayProbeEditor.cs`，为唯一新 runId `x530-hit-internal-v1` 在原 Battle Scene Play 副本的 World reset boundary 开启只读 `BattleHitExecutionPlanMode.ShadowCompare`，记录 OID211 计划条目的目标 slot/候选序号、预计与观测处置、预处理/消费/写者标志和计划失败计数。旧运行 ID 行为保持。若已有 ShadowCompare 观测无法区分 rest 拒绝，明确报告其证据边界，不借诊断改变生产规则或增加旁路战斗逻辑。

验收：脚本修改前建 Change Record、Ledger、STATE/handoff；生成 Editor 工程 0 error，原 Editor 刷新编译 0 error；单次独立请求完成 3 tick、退出 Play、Scene clean、四保护资产 SHA 稳定；比较正式源码逐候选顺序和 Unity 实际观测，并保留任何首差/失败；运行 ChangeLedger validator 和 diff check。只验证本样本内部观测，不宣称完整 World、物理键、C052/Q07 全部关闭。

风险与回滚：ShadowCompare 本应只读，但可能暴露其它尚未对齐的投影差异；先留原件并分清生产结果与诊断首差。原 Editor 若不空闲或已有请求，暂停 Play。回滚仅审阅本 ID 的探针增量；不对工作树执行 restore、删除或清理。无生产代码、DAT 数值、图片、Scene、配置或非战斗修改。

实施与验收：新 runId 才在原 Scene World tick0 启用只读 ShadowCompare；每 tick 导出 OID211 的计划条目和诊断计数。旧 runId 不启用。生成 Editor 工程 `dotnet build --no-restore` 0 error、251 warning；原 Editor 刷新后测试程序集更新，原 Battle Scene 3tick完成、退出clean/四SHA稳。tick1三候选顺序与实际消费/写者 `true,false,true` 对正式源码 applied/rejected/applied 3/3，三tick尾21/21字段零差、计划失败/观测差异0。Unity 未直接逐候选导出拒因文字或即时rest；根逐hit内部、全World、真实玩家键盘仍待。
