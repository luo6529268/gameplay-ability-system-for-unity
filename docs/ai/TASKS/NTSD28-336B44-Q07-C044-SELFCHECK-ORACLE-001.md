# NTSD28-336B44-Q07-C044-SELFCHECK-ORACLE-001

状态：`VERIFIED`（仅SelfCheck合成口径；C044父出口开放）；父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`；BATCH-04/Q07/C044。

原 Editor 完整 SelfCheck 在 C042 旧断言修正后执行到 `CheckCpointDecreaseEscape`，首差是旧“负 decrease 同轮将双方动作计数设1、命中计数维持0”预期。失败原件为 `artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/selfcheck-result-02.txt`。当前336B44正式 `BattleWorld28::advance_catch_relations` 跨零分支写双方待结算 hit contribution count=1、目标冲量X±4/Y-3，不写动作计数或即时速度；Unity C044共用 `BattleCpointWriter.RunKind1` 已相同，`SimulationWorld.FramePostProcessAll` 后才结算并清计数。

只改 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs::CheckCpointDecreaseEscape` 中对应三处旧预期：跨零 pass 后动作计数/命中计数、即时速度、帧后处理速度及计数。保留动作0/181、等待计数、关系和原有位置/冲量断言；不改生产代码、DAT/图片、Scene、菜单。改脚本前创建同名 Change Record、Ledger、STATE/handoff；改后生成工程编译、原 Editor完整SelfCheck，记录下一首差或真实通过；校验账本、差异与四保护SHA。

预期仅修正诊断口径，不改变战斗行为。完整SelfCheck即使通过，也只关闭这条测试门，不自动关闭C044自然物理键或Q07整组。回滚仅审阅本次测试断言的精确差量，不使用整体Git恢复。

2026-10-02 范围增补（再次修改脚本之前）：第二轮修正后 SelfCheck 已通过 `CheckCpointDecreaseEscape`，下一首差在紧邻的 `CheckCpointEscapeAndMismatchControlFlow`，同属 C044 负 decrease 跨零分支。该方法内正例两组仍期待动作计数改1、HitCount0、即时速度；阴性缺失/互指不一致分支保持原断言。现把同文件该方法两个正例断言纳入本 Change，先静态核对再修改，不扩到其它 SelfCheck。
