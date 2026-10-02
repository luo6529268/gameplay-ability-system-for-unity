<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-SHADOW-HIT-SCENE-001
status: SUPERSEDED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 playable GameSession double natural Uj hit sequence and existing Unity read-only ShadowCompare plan
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-SHADOW-HIT-SCENE-001.md
-->

# C053 自然双命中内部观测

脚本前记录。正式源码三人自然链 tick7 两条命中为 attacker51、52→target50，状态 applied/effect2/Uj156；原 Unity Scene 已证 12 tick×11 所选字段132/132同态，两个 attacker rest各10、目标HP450，但尚未直接读取每条 Unity hit 内部 disposition/consume。生产 `BattleEcsHitExecutionPlan` 已有只读 `ShadowCompare` 观察及 `TryGetBattleHitExecutionPlanEntryForDiagnostics`，默认 Disabled。测试可在 World tick0设模式，不应改战斗规则。

本包只扩展既有三人请求式 Editor 探针为第二唯一 RunId，测 World tick0 模式设置及 tick7 hit plan entries，源码/Unity末态复核仍为防止诊断改变行为的 guard。详细不变量、范围、验收、回滚见 Task。预期副作用仅为 Play 副本 diagnostic mode 和新 JSON；所有旧结果保留。任何更改生产代码、DAT、Scene或删除均不属于本包。代码后须登记实测与未验证项。

实际修改：仅扩展 metadata 所列测试 Editor 脚本，新增tick0只读ShadowCompare设置与逐tick hit plan entries、诊断失败原因和差异掩码采集；`RunId`/请求路径/结果目录独立，两次JSON均不覆盖旧结果。生成 Editor 工程两轮 build 0 error，原 Editor 导入并运行两轮12tick；源码/Unity声明132/132各零差，tick7两条实际Damage/consume指纹同预测，但ShadowCompare报两次 `ObservationWriterEffectMismatch`、仅音频bit63。因此目标mismatch0未通过，不能标VERIFIED。两次退出Play clean/四保护SHA稳；未改生产/DAT/Scene/非战斗。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-HIT-SCENE-001/REPORT.md)。仍待Q10正式/Unity逐音效对照；物理键/全World/C053/Q07开放。回滚仅审阅本测试增量，删除须单独记录批准。

2026-10-02 后继更正：上段仍是两轮RED时点事实。Q10/C053正式/Unity实际音频42/42同态，后继 `NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001` 定位投影X897/实际X584；`NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001` 以通用坐标入口修正后同原Scene12tick两自然Damage的ShadowCompare已零差，战斗/实际音效174/174。原本包测试探针请求入口由后继唯一RunId取代，RED JSON保留；本Change状态改`SUPERSEDED`，不把原失败记录改写为当时PASS。后继仍`RUNTIME_PENDING`，C053/Q07整体开放。[后继报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001/REPORT.md)。
