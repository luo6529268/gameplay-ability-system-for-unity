<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C052-POSITIVE-REST-CONSUMER-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
authority: selected 336B44 playable battle_world.cpp effect21 current-state and zero victim_rest termination gate
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C052-POSITIVE-REST-CONSUMER-001.md
-->

# C052 effect21 正间隔共用候选修复

脚本前登记。正式 `battle_world.cpp:5007-5015` 对 effect21 当前 state18/19 的整次攻击终止要求 `victim_rest(target,attacker)==0`；`:6574` 后续普通 rest 门只拒绝当前候选、不终止。Unity 原 `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 在普通 prelude 前只检查 effect21/current state18/19，不查 rest。正式源三候选受控 tick1 双目标HP420/第二候选不终止；原 Unity Battle Scene tick1首目标HP420/rest44、次目标HP500/rest0，远距控制次目标保持500。这是当前生产运行首差，不只是静态猜测。

唯一生产脚本范围为本 Record 的共用 runner，预计仅追加 relation rest 零条件。前置运行路径、风险、验收及回滚见对应 Task。不可改变普通 rest prelude、候选冻结字段、命中结果写者或其他行为；不改 DAT/Scene/Unity框架。修复后必须记录编译、聚焦正反例、原Scene同条件与未验根/自然链；只在证据覆盖范围内标记状态。

2026-10-01 实际代码：`BattleHitCandidateSequenceRunner.TryConsumeCandidate` 在原kind0/effect21/current state18/19提前终止条件增加 `world.GetRawRestVrest(targetSlot, attackerSlot)==0`，并将旧R4注释更正为当前336B44合同；其余候选/预处理/写者未改。原Editor Refresh后二个程序集更新，最近导入日志未见新CS错误。修复后原Battle Scene近X530、远X650各3完整Driver tick与当前正式源码选定7字段×3＝每例21、合计42/42零差，两个Play均SCOPED_PASS/退出/Scene clean/四保护SHA稳。零rest第一目标action203反例v2第一目标HP500/rest0、第二目标对OID211的rest0，SCOPED_PASS/exit/clean；首轮v1把第二目标HP不变写进断言，实际它被action203自身effect20路径影响至470，未逐hit证明因果，v1保留且不归因本生产门。完整证据见 artifacts/diagnostics/NTSD28-336B44-Q07-C052-UNITY-SCENE-001/ACCEPTANCE.md。`git diff --check`目标代码/Record通过；`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>`通过1107 Records/50 governed code files。正式根EXE三实体/自然OPoint/物理键/全World待，状态RUNTIME_PENDING，父C052/Q07/目标开放。

2026-10-01 根正式 EXE 后补：根当前SHA336B44；直接接受同三实体LFR，近远各passed/failureCode0，源/根选定动作/HP合计30/30零差。原Scene近远源/Unity42/42仍有效，受控根→源→原Unity链成立。未补自然角色→OID211出生、物理键、全World或逐hit内部字段；本Record保持RUNTIME_PENDING。
