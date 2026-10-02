<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C043-INVALID-HELD-TAIL-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Passes/Interaction/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationQueryAndLinkModuleEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldNegativeFrameLifecycleGuardProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: selected 336B44 BattleWorld28::settle_held_refill_objects invalid reciprocal tail clears only child interaction_state
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C043-INVALID-HELD-TAIL-001.md
-->

# C043 当前权威失效持有关系尾

脚本前记录：正式 336B44 battle_world.cpp 的三个失效分支均只写子体 interaction_state=0 后 continue；当前 battle_world_tests.cpp 有对应断言。Unity 旧 B6 生产与测试保持负 LinkState，构成版本首差。受影响路径和符号、验收/风险/回滚见 Task。先 RED 再共用入口修复，旧 B1E13 的 Change Record 原样保留为历史。当前不改 DAT/Scene/非战斗。

2026-10-01 RED 阶段：已先在 `SimulationQueryAndLinkModuleEditorTests.cs` 的越界父槽和互指不匹配两例写入新版断言，要求只清子体 `LinkState`、其它关系字段保留，第二次扫描不重复报错。生产入口尚未修改；待原 Editor 聚焦测试证明旧实现失败。

RED 实测：原项目 Editor job `4ca68d728543402ab94797ad876f417b` 仅执行上述两例，2/2 按预期失败：越界预期0/实际-1，互指不匹配预期0/实际-2；该结果是旧生产行为证据，不记作功能验收失败。

实际代码：`SimulationQueryAndLinkModule.HeldObjectProcessAll` 的失效负关系共同分支仅将子体 `Runtime.LinkState` 清零并刷新快照，保留父槽和其它实体字段；诊断事件捕获原负值并记录 `Before`→`After`、`Outcome=cleared`。`SimulationQueryAndLinkModuleEditorTests` 覆盖越界、不活动、互指不匹配、slot0、高槽、生命周期、事件与无sink分配；`NTSD28B6HeldNegativeFrameLifecycleGuardProductionEditorTests` 的完整tick期望改为首扫一次失败；`BattleRuntimeSelfCheck` 更新两次扫描及互指断言。旧 B1E13 Record 不变。生成 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly`：0错误、266警告；原Editor GREEN/Play尚待。

更正：按“此尾只改状态”合同删除额外 `RefreshRuntimeSnapshot()`；最终生产分支仅写 `Runtime.LinkState=0`，避免重写其它派生字段。最终生成工程 0 错误；原 Editor 最终 DLL 晚于脚本，完整tick失效三例 job `5f8a272fb7244fd7b4c3143beb3e70e6` 3/3 PASS。前一版聚焦七例 `b8ecaa395ddc47f5a3d5c577c15bb125` 7/7、完整tick三例 `fc2fe7b087c04432974fb0d48f163c76` 3/3、正常持有六例 `4f5d6dede24b4b5f825c1ef44f989fb2` 6/6；最终版原 Battle Scene Play 请求结果七例 PASS，退出后 Editor idle，四保护资产 SHA 与请求前相同。报告见 `artifacts/diagnostics/NTSD28-336B44-Q07-C043-INVALID-HELD-TAIL-001/REPORT.md`。完整SelfCheck、正式根EXE失效关系自然链和相同场景可观察结果未验，故限定 `SCOPED_PLAY_PASS / RUNTIME_PENDING`；Q07仍开放。`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <project>` PASS（1091 Record/38 代码文件），`git diff --check` exit0。

2026-10-02 增量更正：正式根自然链与原Scene同条件已分别通过选定840/840和语义化1020/1020字段；在原Scene后续tick42还暴露共用槽释放时旧AI行失效顺序问题，独立Change ID `NTSD28-336B44-Q07-C043-SLOT-RELEASE-PUBLISHER-001`修正，第三轮60tick无异常且退出池0/Scene clean/四SHA稳。先前“根/Scene待”仅为历史快照；完整SelfCheck/其它失效路径/全World与物理键仍待，父项保持`RUNTIME_PENDING`。[自然Scene报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/REPORT.md)。
