<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C048-BDEFEND-EARLY-RETURN-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs
authority: selected 336B44 playable BattleWorld28::resolve_confirmed_unarmored_standard_hit victim-rest and first-BDY order
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C048-BDEFEND-EARLY-RETURN-001.md
-->

# C048 特殊 BDY 提前返回前防御累计值

脚本修改前建立。正式源 `battle_world.cpp` 6574～6582 先检查 victim-rest，再写目标防御累计值45，然后处理首个当前 BDY；Unity 共用 `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 已有 rest 门与特殊响应，但在响应提前返回前未写45。影响仅战斗命中后的目标累计值与后继反应；不改 DAT、Scene、模式或非战斗行为。

先在既有 `BattleCollisionHitDamagePlayModeProbeEditor` 的首 BDY 早退断言中纳入 Bdefend45，再在共用 runner 的门后写45。对既有未提交内容保留并只审阅声明路径的行级差量。验证生成工程编译、原 Editor 定向 Play、需要时正式OID301同态。Editor 未导入时不得称运行时通过；回滚仅逐行撤销本 ID 的最小改动，保留他人改动。

2026-10-01 实际改动：现有 R8 战斗 Play 探针的 OID300/frame30 首 BDY kind1033 早退断言新增 `Runtime.Bdefend == 45`，并将该值写入探针 JSON；共用 runner 在有效未命中间隔门后、调用首 BDY 响应前针对 native ordinary unarmored continuation 写入45。只改上述两处战斗脚本。最终 `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 成功，232 个警告、0 错；`Tools/Validate-ChangeLedger.ps1` 显式仓库根并临时禁用 Git 自动CRLF提示后PASS，1083 records/24 diff code files；相关 `git diff --check` 为0。原 Editor 程序集仍早于本轮脚本写入，尚未运行新断言与真实 Play；正式 OID301 源/根/Unity 同态也待。状态 `CODE_WRITTEN / UNITY_RUNTIME_PENDING`；不关闭 C048、Q07。差量 `git diff --` 两声明脚本仅 4 行生产代码及 3 行探针增量；未碰 DAT/Scene/其它战斗或非战斗脚本。

2026-10-01 脚本前补充：原 Editor 已编译本包，定向Play于tick449调用既有探针，建夹具检查提示旧OID300/frame30不再携带kind1033/respond0；矩阵未执行，结果FAIL仅属夹具内容漂移。探针清理后object/slot/pool基线、统计、RNG、待播音效与rest均恢复；已退出Play，Battle Scene磁盘SHA未变。正式OID301→s/1/1.dat的frame29含kind1033，formal/staged SHA同为FB2517651A071550E23B53D667321F4273175DC56CC9CC8E909ECA3540C8FF6D。现计划仅修本Record已声明的探针代码中OID/帧与证据字段300/30→301/29，不改生产/DAT/Scene或原预期响应33/Bdefend45；修后原Editor编译及同探针Play再验。回滚只审本次几处夹具行。状态保持`CODE_WRITTEN / RUNTIME_PENDING`，不得把夹具FAIL记为战斗FAIL。

2026-10-01 实际夹具修正：仅改本Record已覆盖的 `BattleCollisionHitDamagePlayModeProbeEditor` 中正式配置读取与校验OID300/frame30→OID301/frame29、`ProbeCriminal.ObjectId`、构造初始帧、报告`objectId/sourceFrame`和相应诊断文字；保留响应action33及Bdefend45等原断言。`BattleHitCandidateSequenceRunner`本次未再编辑。生成Editor工程0错/232警告，相关脚本diff-check通过，ChangeLedger validator通过1085 records/29 diff code files；原Editor DLL更新至2026-10-01 00:15:06 UTC，正在域重载，真实Play未重跑。状态保持`CODE_WRITTEN / RUNTIME_PENDING`。

2026-10-01 脚本前第二次补充：修OID301夹具后的原Editor全矩阵Play于tick1089通过夹具前置，但在首BDY断言之前因独立角色击倒统计归属断言失败，`cleanupCompleted=true`；原Scene哈希仍为3A089236...235ED，已退出Play。失败JSON保存在`artifacts/diagnostics/NTSD28-336B44-Q07-C048-BDEFEND-EARLY-RETURN-001/full-matrix-unrelated-stat-failure.json`，此前OID300失败也单独保存。为隔离C048，计划仅在本Record已声明的探针脚本加C048专用菜单+独立Temp结果路径，复用现有等待与清理，只建立正式OID301/frame29首BDY一对并运行生产PostInteraction，断言Bdefend45和动作/队伍/hold/wait/HP/vrest；不改原R8矩阵、不修改生产/DAT/Scene。完成后编译、原Editor导入/定向Play/退出及四SHA。状态仍`CODE_WRITTEN / RUNTIME_PENDING`。

2026-10-01 实际专用探针代码：在已声明的 `BattleCollisionHitDamagePlayModeProbeEditor` 加C048菜单入口/专用Temp结果路径/单对`ExecuteC048Only`，从正式OID301/frame29创建攻击者目标，在生产World收集一个候选并消费，断言action33/group1/hold3,-3/wait77/HP100/vrest0/Bdefend45，复用原清理与baseline检查；原全矩阵仍保留，生产runner本次未再改。`dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 0错/232警告。原Editor尚未导入该专用入口，Play待，不关闭C048/Q07。

2026-10-01 原Editor真实Play后续：新增C048专用单对入口在原Battle Scene正式暂存OID301/frame29下产出`PASS`，一个候选，Bdefend45、动作33/队伍1/停顿3,-3/等待77/HP100/vrest0；`cleanupCompleted=true`，对象4→4、槽2→2、两池2→2，统计/RNG/待播音效/rest/plan全部恢复。退出Play后Editor idle非Play、Scene isDirty=false、四保护SHA稳定。原始PASS与此前两次前置FAIL均保留，见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C048-BDEFEND-EARLY-RETURN-001/REPORT.md)。状态`RUNTIME_PENDING / CONTROLLED_PLAY_PASS`：正式OID301自然入口与根同态尚未证，不宣称C048或Q07完成。回滚仍仅审阅本ID的生产共用写入与探针增量；未改DAT/Scene/非战斗。
