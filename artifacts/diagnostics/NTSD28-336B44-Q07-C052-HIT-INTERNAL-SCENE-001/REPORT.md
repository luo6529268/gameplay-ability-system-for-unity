# Q07/C052 effect21 原 Scene 逐候选内部观测

状态：`VERIFIED_SCOPED_HIT_SEQUENCE / C052_OTHER_EXITS_OPEN`。权威为根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及其 playable `BattleWorld28`；沿用既有正式源码完整 GameSession tick1 的 `source-hits.tsv` 与正式根三实体 LFR 限定证据。本包仅给原 Unity Battle Scene 的现有 OID211/action161、两名 OID2/X500/530 场景增加唯一新 runId 的只读 ShadowCompare 观测；生产战斗代码、DAT、图片、Scene、配置均未改。

正式源码 tick1 的三个候选为：目标1 `effect21/applied`；同目标第二条 `effect21/rejected`，消息为 relation rest 阻挡、`terminates=0`；目标2 `effect21/applied`。原 Unity Battle Scene 在 reset boundary 仅一次启用只读 ShadowCompare，三条 OID211 条目依次指向 Unity 目标 slot0、slot0、slot1；`ConsumeEffectsObserved/WriterEffectObserved` 依次为 `true/true`、`false/false`、`true/true`。候选顺序与实际消费/写者生效模式同正式源码 3/3；hit plan `CurrentTickPlanValid=true`、`FailureCount=0`、`ObservationMismatchCount=0`。Unity 条目的 `ExpectedDisposition/ObservedDisposition=Damage` 表示候选类别，**不表示第二条实际造成伤害**；第二条没有消费或写者，后续第三条仍被处理。

正式源码与 Unity 三个完整 tick 的攻击者动作、两目标动作/HP/rest 共 21/21 字段一致；tick1 两目标均 HP500→420，rest44。原 Scene 报告 `SCOPED_PASS/DONE`、`shadowConfigCount=1`、退出 Play、Scene clean；Battle/Menu Scene、GameConfig 和 ProjectBattleModeConfig 前后四 SHA 全同。原 Editor 刷新后的测试程序集时间晚于探针修改，生成 Editor 工程编译 0 error/251 warning。`Tools/Validate-ChangeLedger.ps1` PASS（1135 Records/11 governed code files），`git -c core.safecrlf=false diff --check` PASS；未重跑完整 SelfCheck，因本包只扩诊断探针。

限制：ShadowCompare 直接观测了第二候选未消费/未写者及其后第三候选继续；其内部未逐条导出“被拒原因等于 rest”的文字或即时 rest 数值，这部分依据正式源码、已实现的共用 rest 门、源逐候选消息与 Unity tick 尾 rest44 联合判断，不能写成 Unity 独立逐字段读出了拒绝原因。正式根 LFR 的既有 trace 只证明所选 tick 尾字段同态，不证明根逐 hit 内部状态；当前一组受控初态也不等于真实玩家键盘、完整 World 或 C052/Q07 全部关闭。

原件：本目录 `comparison.json`、`request-before.json`、`request-submitted.json`、`protected-before.json` 和 `protected-after.json`；Unity 原 Scene 报告为 `../NTSD28-336B44-Q07-C052-UNITY-SCENE-001/x530-hit-internal-v1.json`，正式源码逐候选为 `../NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001/a161-x500-x530-v1/source-hits.tsv`。
