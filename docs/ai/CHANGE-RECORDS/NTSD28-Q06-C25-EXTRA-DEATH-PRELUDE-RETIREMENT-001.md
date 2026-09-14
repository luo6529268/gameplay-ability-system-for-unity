<!-- CHANGE-RECORD
id: NTSD28-Q06-C25-EXTRA-DEATH-PRELUDE-RETIREMENT-001
status: VERIFIED
change-kind: RETIRE_EXTRA_C25_DEATH_PRELUDE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsLateTailNoOpEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/LateRuntimeSnapshotBoundaryEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06DeadCharacterPreludeEditorTests.cs
authority: Formal C25 step_frame -> timers -> OPoint; source witness6480, physics/hit/WPoint owners read unchanged.
evidence: 3240 frame endpoints never change action/position/link; full driver3240 has1716 full-tick action changes and384 explicit WPoint releases, proving different owners must remain.
-->

# 退休C25额外死亡前置处理

IN_PROGRESS / TEST_FIRST。准确八脚本：LF2Entity退休RunLateDeathOpointPreCleanupPhase与私有DropHeldObjectForCurrentDatDeath/EnterCurrentDatDeathBounceFrame；LF2Character退休无其它调用者的override和ForceDropHeldWeaponForLateDeathInternal包装（通用ForceReleaseHeldObjectReference保留）；Module退休该生产调用及CanSkipExactCharacterDeathOpoint，保留已有snapshot/profile API和诊断阶段数值，原DeathOpointNoOp计数改为退休区段经过槽位数，不注入规则；SimulationWorld仅移除内部self-check probe的obsolete override，保留原数组9/10列为0表示退休事件，禁止改通用World架构。

三个旧测试文件因钩子移除同步迁移：SelfCheck GT07/CheckLateDeathBounceFrame改实际Late pass验证不强制弹起/不额外释放，不能只改数字而继续调用旧helper；LateTail derived probe移除obsolete hook/计数，保留其余virtual职责；SnapshotBoundary旧事件计数改0但snapshot模式及索引不变。新Editor测试原phase0向量的动作/位置/速度/关系在实际C25后保持，计数/render/resource等额外C25职责不拿两端点假装完整driver；完整source phase1用于保留真实physics/WPoint职责。两帧后端、真实Character/共享当前DAT外壳覆盖，原6480 witness不重写expected。

风险：旧death emitter导致HP0帧0/5/212额外186和丢武器；移除后应由正常hit/physics/WPoint/terminal决定动作和关系。上述真实writer不在本Record内，不修改。无新字段/schema/服务/队列或shutdown阶段，仍用既有明确terminal及有序关闭回收。实体不因单纯HP0被伪造动作，不等于禁止正常死亡反应。

验收：现有state9998 type0 RED48差异保留，新向量先RED；编译0、new/core/weapon/late/snapshot focused、完整SelfCheck新首差异、真实Scene HP0帧/绑定关系存活/快照恢复及关闭全0。资源15/23/26/2/2与raw47/3不改；DAT/图片/Scene/非战斗/Unity-GAS框架/Server/Gen/Plugins不动。回滚仅上述差量且须批准，现有用户改动保留。

已修改准确八脚本，旧钩子/私有bounce-drop/无用Character包装及CanSkip helper已全部无源码引用。保留diagnostic旧phase/snapshot编号，经过该历史边界仅累加跳过计数；World self-check数组9/10保留零值，移除其旧probe方法及counter字段。新原3240向量四配置全部RED，已保存；新增独立调用重写后的SelfCheck目标，避免完整SelfCheck后继landing失败使该目标未实际运行。更正：原full tick1716次动作变化尚未逐例归因，不一概称为物理；真实physics/hit/WPoint代码未修改。

新测试已增加两个RNG域前后不变检查及真实Scene HP0 standing+真实kind2持有生成的C25验证probe（原声明文件），只运行Late并显式回收临时child、恢复快照/原materializer模式，不改共享DAT或Scene。目标SelfCheck已被独立NUnit实际执行并通过；完整SelfCheck仍需复验。

VERIFIED / EXTRA_DEATH_PRELUDE_REMOVAL_ONLY，6480原源、四配置各3240/64联合全PASS、独立SelfCheck目标及真实HP0-kind2持有C25/4→4 checksum/关闭全0两帧Stopped。完整SelfCheck仍type2落地方向FAIL，整体Q06/总目标不关闭。
