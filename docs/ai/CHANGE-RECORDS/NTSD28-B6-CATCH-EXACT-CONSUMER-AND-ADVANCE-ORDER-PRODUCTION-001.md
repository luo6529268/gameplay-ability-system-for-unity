# NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_MIXED_CATCH_ADVANCE_EXACT_CONSUMER
code-path: Assets/NTSD/Scripts/Simulation/Passes/Interaction/BattleInteractionPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CatchExactConsumerAndAdvanceOrderProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/PreInteractionNoOpProofEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointThrowAtomicProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28::advance_catch_relations single ascending mixed pass, exact +0x90 consumers and terminal continue branches; battle_world.cpp EB37E8EC; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-1-PASS-7-FAIL-OF-8-JOB-B09CA0 / FOCUSED-16-OF-16 / PREINTERACTION-15-OF-15 / B6-CATEGORY-57-OF-57 / HITPLAN-185-OF-185 / NTSD28-183-OF-183 / TARGETED-PLAY-16-CASES / BUILDS-0-ERROR / SELFCHECK-BLOCKED-UNRELATED-HELD-ACCOUNTING / CONSOLE-0-ERROR / SCENE-UNCHANGED / LEDGER-PASS-425-RECORDS-361-CODE-FILES / SINGLE-MIXED-ADVANCE-EXACT-CONSUMERS
-->

> 状态：`VERIFIED / RED_1_PASS_7_FAIL_OF_8 / FOCUSED_16_OF_16 / PREINTERACTION_15_OF_15 / B6_CATEGORY_57_OF_57 / HITPLAN_185_OF_185 / NTSD28_183_OF_183 / TARGETED_PLAY_16_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_UNCHANGED / SINGLE_MIXED_ADVANCE_EXACT_CONSUMERS`

## 改前事实

- `BattleInteractionPipeline.RunPreInteraction()`先对所有participant运行kind1，再对所有participant运行kind2，
  最后live scan settlement；低槽kind2因此错误观察到高槽catcher已经变更后的action。
- `BattleCpointWriter`的kind1 reciprocal、kind2 source、settlement reciprocal均读compat
  `CatcherSlotIndex`；前置producer已同时发布exact字段，但consumer尚未迁移。
- mismatch仅设skip flags后仍可能fallback throw/dircontrol；negative release写`HitCount=1`且仍可能继续
  throw/dircontrol，均与Authority terminal `continue`相反。
- no-op diagnostics仍按三个sweep计数；既有SelfCheck明确保留旧fallback throw/dircontrol尾，需先变RED。

## 预定实现

- 增加每实体mixed advance入口，在slot live ascending traversal中按snapshot-kind1-eligible / else-current-kind2
  二选一调用既有virtual hook；advance结束后再独立live ascending settlement。
- 三个正式关系读取改为plain exact `CatchSourceSlot90`；compat只保留镜像。
- mismatch立即return；negative release写双方`AttackingCounter=1`并立即return，保留正常路径。
- no-op/participant-filter诊断和夹具由三sweep重基线为两pass，不隐藏derived hook变化。

## 验证记录

- Unity EditMode RED job `b09ca065d926466f8d7e628dc0173377`：`1 pass / 7 fail / 8 total`。
  唯一通过的是低catcher/高caught（旧两sweep偶然同结果）；失败精确覆盖低caught/高catcher错误212、
  kind1/kind2/settlement三个compat读取、tagged exact source未按plain slot拒绝、mismatch仍继续throw尾、
  negative release仍被throw覆盖且frame-counter carrier错误。
- RED夹具编译0 error；尚未实施或执行GREEN、SelfCheck、Play、Console、Scene与Ledger final validation。

## 实际实现

- `BattleInteractionPipeline.RunPreInteraction()`现以runtime slot `0..capacity-1`执行一个live ascending
  mixed advance；每个slot按advance开始时的kind1且`FrameDelay >= 0`条件选择kind1，否则仅在当前frame
  为kind2时选择kind2。advance结束后仍执行独立的live ascending settlement pass。
- `LF2Entity.RunCpointAdvanceStep10()`封装二选一入口，并继续调用既有virtual kind1/kind2 hook，保留derived
  测试观察点；no-op证明从三sweep重基线为两个正式pass。
- kind1 reciprocal、kind2 validation与settlement reciprocal统一读取plain exact
  `Runtime.CatchSourceSlot90`；compat `CatcherSlotIndex`仍由producer/lifecycle镜像但不再裁决这三个consumer。
- kind1 reciprocal mismatch现写catcher action 0后立即return；negative decrease跨到负值时写双方
  `AttackingCounter=1`、caught action 181、水平抛速`-4/+4`与`Vy=-3`后立即return，保留relation、负timeout
  与`HitCount` sentinel；normal action/throw/dircontrol路径保持。
- `PreInteractionNoOpProofEditorTests`的runtime comparator改为比较`NTSD28InputProxyBlock`序列化字节，修正
  测试夹具把等值proxy对象按引用判不等的旧缺陷。B6类别初跑暴露四个旧throw fixture只写compat relation；夹具
  补写已由前置producer定义的exact source后，57项全绿，生产代码未为旧夹具回退。

## 最终验证

- 精确RED：Unity EditMode job `b09ca065d926466f8d7e628dc0173377`，`1/8 pass`、`7/8 fail`，
  七项失败均为计划内旧差异。
- focused GREEN：job `2aa6fc9536b54731876f79dde709801c`，`16/16`；覆盖slot双极性、三个exact
  consumer、tag拒绝、两个terminal fence、nonterminal/normal continuation、slot reuse与warmed 0B。
- no-op/fast-vs-legacy回归：job `2f0103...`，`15/15`；初跑的proxy引用比较失败已按上节修正为
  序列化值比较。
- B6 category：job `7fe3c9619cd54038baf8c0eedcf40a42`，`57/57`。
- `NTSD28` category：job `8edaf8cfab6b40a6a2af1d40e8fbb87f`，`183/183`。
- `BattleHitExecutionPlanEditorTests`：job `58045e36a41b4fb5bd92055f0ad833dd`，`185/185`。
- targeted Play request `NTSD28-B6-CatchExactConsumerAdvance-Play-v1`：`Passed / 16 cases / exactConsumers=3 /
  terminalFences=2 / warmedAllocationBytes=0 / sceneMutation=none`。这是代码路径定向Play，不冒充当前content
  中存在自然mixed slot witness。
- 最终 `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal` 与
  `Assembly-CSharp-Editor.csproj`均为`22 warnings / 0 errors`；Unity测试运行亦完成脚本编译且无编译错误。
- fresh full SelfCheck通过本包新增检查后，仍在独立旧项
  `CheckSharedDatCpointStep10StatsAndInputOrder()`的held injury accounting断言停止
  （`BattleRuntimeSelfCheck.cs:11547`）；不得将其计为本包失败或完整SelfCheck通过。
- 清理后Unity Console `0 error`；active scene `NTSD_Battle / isDirty=false / rootCount=16`；scene SHA-256
  `89AA621640A3818F2C7CD307836C50D72CAB84B6D4F2B6DB333FF7CBF525A673`、length `216762`，与基线一致。
- `git diff --check`通过；`Tools/Validate-ChangeLedger.ps1`通过（425 records、361 governed code files；
  输出中的历史路径告警不构成失败）。settlement vaction preflight与held accounting仍明确后置，未扩大本包。

## 回滚

反向恢复本Change的mixed traversal/exact reads/fences/tests；不得回退前置Change或用户文件。
