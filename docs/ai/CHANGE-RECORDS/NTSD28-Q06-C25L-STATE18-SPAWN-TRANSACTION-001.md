<!-- CHANGE-RECORD
id: NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001
status: IN_PROGRESS
change-kind: C25L_NATIVE_PARTICLE_SPAWN
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeState18ParticleWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State18SpawnEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25LState18ParticleOwnerEditorTests.cs
authority: Formal materialize_state18_broken_weapon_particles/spawn_at; 1550 original source/full-driver vectors and F08.
evidence: Legacy Match.Rng and ordinary birth in current C25L remain wrong despite correct owner placement.
-->

# State18/19粒子生产生成事务

事前准确五脚本。LF2Entity.C25L委托新writer，旧state13/200 SpawnTransitionEffectBranch1/共用旧tail保持；只移除不再被引用的Branch2方法。新writer复用decision kernel及NativeRandom同步callsite，source整数/精确位置和velocity分离，generic HP/MP500/default owner/group/right/action140。nativeWeaponPieceSpawn现有flag在这里同样表示原broken-weapon generic spawn：借用已有IsInitialActionAdmitted/InitializeBirth以及两个factory的目标World提前绑定，SpawnSemantic仍TransitionEffect，避免重复出生/复位字段。flag已有Clear/copy所属任务合同，不加持久字段或schema。

slot读实际World.FindEntityByRuntimeSlotForNativeDisplay语义：包含仍在slot的pending destroy，排除fusion OidMergeDormant与pendingUnregister，等价native live entity存在性。选首次null而非first free bit；原融合保留slot在raw entity不存在但仍claimed时会在required-slot register失败，tuple已经消费并break，不能跳到下一空位。slot50..当前批准profile容量，不全局释放/放宽registry或改变capacity。

生产default delay0保留原B8/Q08动态producer边界；writer可显式接收delay用于独立原函数向量，正delay只抑制持续态选roll，非离开态。Stopping在任何RNG/spawn前拒绝。每粒同步structural create后下一次扫描，不新增queue/manager；task try/finally归还原pool，pool不足break。Module去掉该immediate producer后的旧queued flush，保留活动检查/其他owner flush和previous078→fragment→lifecycle顺序。

验证先旧实际入口RED（不是仅缺API）：原同输入/775出生及完整driver适用场景、两profile/two backend、actual raw47/3和RNG；delay非零单独helper测，当前host无动态producer。新增reserved dormant slot失败、任务/逻辑池不足、Stopping禁止写入，普通spawn/fragment回归。旧owner tests保留位置/镜像保证，仅改Native RNG证据。真实Play两factory/正式999与隔离fixtures/完整SelfCheck/Scene恢复/有序关闭全0。失败原文保留，生成功能通过不代表B8动态gate或整个frame事务完结。

关闭：现有阶段3结构停止、6任务归还、7Renderer、8logic清理，复用现有关闭顺序。无非战斗/Scene/正式资源/Unity-GAS架构/Server/Gen/Plugins变更。用户HUDBg x30/hash bcd1047b保持。回滚只本Record新增差量且先批准，不覆盖用户工作。
