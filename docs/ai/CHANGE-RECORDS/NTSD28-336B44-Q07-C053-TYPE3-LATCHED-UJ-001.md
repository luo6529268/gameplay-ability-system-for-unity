<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-TYPE3-LATCHED-UJ-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3TargetGenericContinuationEditorTests.cs
authority: selected 336B44 playable BattleWorld28 type3 response current hit_Fj versus action-latch hit_Uj
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-TYPE3-LATCHED-UJ-001.md
-->

# C053 type3 hit_Uj 锁存动作帧

脚本前建立。正式 `battle_world.cpp:7030-7044` 按分支读取当前 `hit_Fj`、锁存 `hit_Uj`；Unity普通写者和数据导向计划统一从当前帧读。预期副作用仅 type3 目标的 Uj 响应动作及其后继状态/命中候选；Fj路径保持当前帧。共用解析避免各消费路径再分叉。先加 current/latch 差异测试，后修三声明脚本，生成工程编译、原Editor聚焦及必要原Scene与正式根对照；未运行前不称对齐。保留所有预存脏文件，回滚只审本 ID 行级差量。

2026-10-01 测试先写入既有 `NTSD28B5Type3TargetGenericContinuationEditorTests`：current156/latch153，锁存帧hit_Uj156、当前帧缺字段，旧写者静态将回退20；另以锁存Fj88/当前Fj77验证Fj不被误改。原Editor程序集未导入，未能执行先RED，不声称测试运行结果。

随后仅在 `BattleDamageWriter` 增一个共用响应动作解析，普通写者与 `BattleEcsHitExecutionPlan` 的预检/投影均调用；type3 kind-table transform与pair reset未改。最终 `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 成功，263警告、0错误，生成工程包含新增测试文件。原Editor聚焦、原Battle Scene、正式根同条件C053尚未执行。状态 `CODE_WRITTEN / UNITY_RUNTIME_PENDING`；C053/Q07/总目标均开放。实际文件仅本Record声明的三处，DAT/Scene/GAS/非战斗未改。详[进度](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TYPE3-LATCHED-UJ-001/PROGRESS.md)。

本包审计：`Tools/Validate-ChangeLedger.ps1` 用显式仓库根运行PASS（1084 records、27 diff code files），相关`git diff --check`为0；正式根EXE仍为336B44。Battle/Menu Scene与GameConfig/ProjectBattleModeConfig四保护SHA均与前次一致。原Editor PID11944仍响应，但Editor程序集写入时间仍早于本包，故不能报告Unity脚本编译或Play通过。

2026-10-01 后续追加：原 Editor 编译更新后，Uj锁存帧/Fj当前帧两项完整名称 EditMode job `4c72782ca2874157937b226ec28d9522` 实际2/2 Passed、0 Failed。前一 job `e8d3512ae37846f2b95baae0f6c8f2a2` 过滤器漏`.Editor`、实际0项，即使作业 succeeded 也不计作测试通过。原Battle Scene保持活动、Editor idle非Play，正式OID808自然完整tick、根LFR及Unity Play仍待。状态 `FOCUSED_TEST_PASS / RUNTIME_PENDING`，C053/Q07不关闭。[进度](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-TYPE3-LATCHED-UJ-001/PROGRESS.md)。
