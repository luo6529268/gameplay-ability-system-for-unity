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

2026-10-04 v2聚焦验证：生成Editor工程0 error/299 warning、原Editor脚本程序集新于脚本；在MCP复核Scene clean且空闲后只运行C053 latch-only。原件`artifacts/diagnostics/NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001/latch-only-original-editor-20261004-v2.json`为PASS：两角色候选HP100/100、vrest0/0、锁存true、HitConfirm2=0，清理对象4→4、槽2→2、两个池2→2；通用矩阵未执行字段的默认值不作证据。退出非Play、Scene clean，但磁盘SHA在运行前后`2BF4047C…D67C1`→`8CC56145…269047E`，当前Git差异含并行UI `RippleRing`，时点/写入者未证，本Change没有Scene写操作。保留`RUNTIME_PENDING / SCOPED_GREEN`：共享修复在混合候选完整Driver与相邻纯角色探针均定向通过；Scene文件保护、自然键链、其它C053/Q07及独立R8统计归属失败未关闭。旧RED、旧格式PASS和R8 FAIL均不覆盖。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001/REPORT.md)。

脚本修改前登记。Unity原状：`BattleHitCandidateSequenceRunner.TryConsumeCandidate`在锁存且目标为角色时返回`true`，`TryConsumeCaptured`据此停止该攻击者所有后续候选；现有BattleCollisionHitDamagePlayModeProbe的断言文字也称“abort entire attacker”。当前336B44正式源的同分支仅返回`rejected`、未设置`terminates_attacker_invocation`，正式根tick7先拒slot50→角色slot0候选0，再命中slot50→type3 slot2候选1。原Scene RED在默认和ShadowCompare均出现slot2少90伤、动作10/HP458而正式20/HP368，候选列表两项均在。

2026-10-04 实际改动：仅所列共享`BattleHitCandidateSequenceRunner.TryConsumeCandidate`锁存角色条件从返回`true`改为`false`，使`TryConsumeCaptured`继续后继候选；更新就地注释和既有两角色探针断言文案，未加OID或角色特判。`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0、0 error、330 warning，仅证明生成工程编译；原Editor当前Battle Scene dirty且磁盘SHA在先前Play后外变，为保留用户内容未请求Unity刷新、未Play。GREEN、相邻锁存聚焦、SelfCheck、原Scene 12tick与正式同态、退出清理均**未验证**。状态`RUNTIME_PENDING`，生产不能声称已对齐；待Scene由所有者保存且Editor空闲后只跑相关目标。回滚只审本ID两处脚本局部diff，不动Scene/DAT/用户内容。

2026-10-04 后续证据（覆盖上段当时的“未Play”状态）：用户确认保存且Editor空闲后，原Editor通过Unity MCP刷新并编译生产/测试程序集；新独立默认命中场景12tick `SCOPED_PASS`，正式源码/Unity 261/261 可比字段零差，第7tick辅助对象HP368/action20、目标HP440/action156。该结果验证本共享修复在一个受控非角色后继候选路径生效；未运行相邻两角色锁存聚焦与全量SelfCheck，Tobi自然键链未验。运行内Battle Scene clean且磁盘SHA未变，退出后独立查询Scene又dirty、磁盘仍同，来源未证，保留。生成Editor工程最终0错/299警告。保持`RUNTIME_PENDING / SCOPED_GREEN`，不把一个受控通过称为Q07整体完成；见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-SCENE-001/REPORT.md)。

2026-10-04 再次修改 Editor 测试脚本前登记：既有R8矩阵虽在原Editor当前clean Battle Scene被精确调用，但先于锁存断言，在角色伤害统计归属断言失败；原件保留，`cleanupCompleted=true`且对象数回基线，退出场景SHA稳定。唯一追加代码路径仍为本Record头部已声明的 `BattleCollisionHitDamagePlayModeProbeEditor.cs`：增加独立菜单入口、唯一结果文件及只含锁存攻击者+两个角色目标的 `ExecuteLatchOnly`，沿用基线捕获、候选收集、生产 `PostInteractionTickAll` 和现有清理；旧R8/C048入口/结果不变。新出口只验证两目标HP/vrest、锁存和HitConfirm2，不能宣称其它矩阵或自然技能通过；结果拒绝覆盖。回滚只审本脚本局部增量，Scene/按钮预览/DAT/非战斗不动。脚本修改后须重新编译，再安全运行原Editor聚焦Play及Ledger校验。

2026-10-04 证据结构二次修改前登记：初次 latch-only 原Editor测试PASS，结果文件已只复制到独立诊断目录；但新分支把未测量的“整个攻击者未中止”序列化为旧 `GateEvidence.attackerAborted=false`，不能让默认值冒充事实。仅在同一测试脚本为新分支加入独立 `latchCharacterCandidates` 证据字段及 DTO，填入实际读取的候选序号/槽、两目标HP、两对vrest、锁存及HitConfirm2，并用新的唯一结果路径重新运行一次。旧R8/C048路径、矩阵 DTO、生产行为和其他文件不改。两份旧原件保留，新结果若不通过则不宣称相邻回归已验收。

拟改唯一生产符号`TryConsumeCandidate`该分支的返回值与精确注释，使它只跳过当前候选；拟更正唯一既有测试探针的断言文案，不改变其两角色目标的期望。副作用是后继合法非角色候选可以消费；不能用OID251/808特判。不可回退边界为当前用户/其他任务的Scene、DAT、资源和非战斗脏改；本ID不得改它们。验收与回滚见[Task](../TASKS/NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001.md)。先保留RED原件，修改后编译和窄聚焦，再在Scene重新clean后原Editor Play GREEN；在Play之前只能报告`COMPILE_PASS/RUNTIME_PENDING`，不得称已对齐。
