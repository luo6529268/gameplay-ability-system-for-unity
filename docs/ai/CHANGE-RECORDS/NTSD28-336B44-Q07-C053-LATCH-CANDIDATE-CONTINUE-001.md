<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs
authority: selected 336B44 root EXE and playable BattleWorld28 ordinary hit eligibility plus SimulationTickDriver28 candidate loop
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001.md
-->

# Q07/C053 特殊锁存只拒当前角色候选

脚本修改前登记。Unity原状：`BattleHitCandidateSequenceRunner.TryConsumeCandidate`在锁存且目标为角色时返回`true`，`TryConsumeCaptured`据此停止该攻击者所有后续候选；现有BattleCollisionHitDamagePlayModeProbe的断言文字也称“abort entire attacker”。当前336B44正式源的同分支仅返回`rejected`、未设置`terminates_attacker_invocation`，正式根tick7先拒slot50→角色slot0候选0，再命中slot50→type3 slot2候选1。原Scene RED在默认和ShadowCompare均出现slot2少90伤、动作10/HP458而正式20/HP368，候选列表两项均在。

2026-10-04 实际改动：仅所列共享`BattleHitCandidateSequenceRunner.TryConsumeCandidate`锁存角色条件从返回`true`改为`false`，使`TryConsumeCaptured`继续后继候选；更新就地注释和既有两角色探针断言文案，未加OID或角色特判。`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0、0 error、330 warning，仅证明生成工程编译；原Editor当前Battle Scene dirty且磁盘SHA在先前Play后外变，为保留用户内容未请求Unity刷新、未Play。GREEN、相邻锁存聚焦、SelfCheck、原Scene 12tick与正式同态、退出清理均**未验证**。状态`RUNTIME_PENDING`，生产不能声称已对齐；待Scene由所有者保存且Editor空闲后只跑相关目标。回滚只审本ID两处脚本局部diff，不动Scene/DAT/用户内容。

2026-10-04 后续证据（覆盖上段当时的“未Play”状态）：用户确认保存且Editor空闲后，原Editor通过Unity MCP刷新并编译生产/测试程序集；新独立默认命中场景12tick `SCOPED_PASS`，正式源码/Unity 261/261 可比字段零差，第7tick辅助对象HP368/action20、目标HP440/action156。该结果验证本共享修复在一个受控非角色后继候选路径生效；未运行相邻两角色锁存聚焦与全量SelfCheck，Tobi自然键链未验。运行内Battle Scene clean且磁盘SHA未变，退出后独立查询Scene又dirty、磁盘仍同，来源未证，保留。生成Editor工程最终0错/299警告。保持`RUNTIME_PENDING / SCOPED_GREEN`，不把一个受控通过称为Q07整体完成；见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/REPORT.md)。

拟改唯一生产符号`TryConsumeCandidate`该分支的返回值与精确注释，使它只跳过当前候选；拟更正唯一既有测试探针的断言文案，不改变其两角色目标的期望。副作用是后继合法非角色候选可以消费；不能用OID251/808特判。不可回退边界为当前用户/其他任务的Scene、DAT、资源和非战斗脏改；本ID不得改它们。验收与回滚见[Task](../TASKS/NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001.md)。先保留RED原件，修改后编译和窄聚焦，再在Scene重新clean后原Editor Play GREEN；在Play之前只能报告`COMPILE_PASS/RUNTIME_PENDING`，不得称已对齐。
