<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C052-HIT-INTERNAL-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C052PositiveRestBattlePlayProbeEditor.cs
authority: selected 336B44 playable C052 effect21 eligibility and source hit sequence, existing formal-root and original Battle Scene scoped evidence
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C052-HIT-INTERNAL-SCENE-001.md
-->

# C052 逐候选诊断留痕

脚本前状态：原 Scene 探针已证 OID211/action161 对两目标 X500/530 的 3 tick 末态与正式源码 21/21 字段相同；正式源码 tick1 内部 hit 顺序为 applied/rejected/applied，Unity 原 Scene 未导出候选逐项处置。现有 `SimulationWorld.ConfigureBattleHitExecutionPlanForDiagnostics` 允许 reset boundary 开启只读 ShadowCompare，`TryGetBattleHitExecutionPlanEntryForDiagnostics` 可读取条目。

预期仅增加唯一新请求 ID 的定向诊断字段和结果 JSON；旧 ID 不启用 ShadowCompare。影响符号限 `NTSD28Q07C052PositiveRestBattlePlayProbeEditor` 请求、启动、reset-boundary 配置、tick 抓取及报告类型。不得改生产战斗逻辑、DAT/图片、Scene/Prefab/配置/非战斗。验收、保护边界和回滚方式见同 ID Task；实际差量、编译、Play、逐候选比较与未验项须脚本修改后追加。

实际脚本改动：增加 `NTSD.Simulation.Ecs` 类型引用；`Poll/ConfigureShadowAtResetBoundary` 仅对新 runId 在 World tick0 开启只读 ShadowCompare；`TickRow` 新增候选条目/计划诊断字段，`MeasureOneTick` 仅读取 OID211 条目。旧请求路径与生产战斗写者不变。生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo '-clp:ErrorsOnly;Summary'` 0 error、251 warning；Unity 原 Editor 导入及 Play 待验。当前状态 `CODE_WRITTEN`。

原 Editor 验证：新增探针被导入，测试程序集修改时间晚于源脚本；原Battle Scene `x530-hit-internal-v1` 3tick报告 `SCOPED_PASS/DONE`，ShadowCompare在reset boundary配置1次，tick1 OID211三候选目标slot0/0/1，消费与写者true/false/true，计划有效、失败0、观测差异0。正式源码逐hit applied/rejected/nonterminate/applied 按槽映射3/3；正式源码/Unity三tick动作/HP/rest共21/21同态。退出Play、Scene clean，Battle/Menu/GameConfig/ModeAsset四SHA稳。旧请求原文留档且新请求最终`requested:false`，未删除。根正式EXE SHA本轮复核336B44；根逐hit内部、Unity逐候选即时rest/拒因、完整World与真实玩家键盘未验。定向探针 `VERIFIED / SCOPED_HIT_SEQUENCE_PASS`，C052/Q07继续开放。原件见 `artifacts/diagnostics/NTSD28-336B44-Q07-C052-HIT-INTERNAL-SCENE-001/REPORT.md`。

审计：`Tools/Validate-ChangeLedger.ps1` PASS（1135 Records、11 governed code files），`git -c core.safecrlf=false diff --check` PASS。本包未重跑全量SelfCheck，近期其它Change的自检结果不当成本包新证据。
