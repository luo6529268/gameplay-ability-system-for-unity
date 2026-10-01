<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-SHADOW-HIT-SCENE-001
status: PLANNED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs
authority: selected 336B44 playable GameSession double natural Uj hit sequence and existing Unity read-only ShadowCompare plan
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-SHADOW-HIT-SCENE-001.md
-->

# C053 自然双命中内部观测

脚本前记录。正式源码三人自然链 tick7 两条命中为 attacker51、52→target50，状态 applied/effect2/Uj156；原 Unity Scene 已证 12 tick×11 所选字段132/132同态，两个 attacker rest各10、目标HP450，但尚未直接读取每条 Unity hit 内部 disposition/consume。生产 `BattleEcsHitExecutionPlan` 已有只读 `ShadowCompare` 观察及 `TryGetBattleHitExecutionPlanEntryForDiagnostics`，默认 Disabled。测试可在 World tick0设模式，不应改战斗规则。

本包只扩展既有三人请求式 Editor 探针为第二唯一 RunId，测 World tick0 模式设置及 tick7 hit plan entries，源码/Unity末态复核仍为防止诊断改变行为的 guard。详细不变量、范围、验收、回滚见 Task。预期副作用仅为 Play 副本 diagnostic mode 和新 JSON；所有旧结果保留。任何更改生产代码、DAT、Scene或删除均不属于本包。代码后须登记实测与未验证项。
