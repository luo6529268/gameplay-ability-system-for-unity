<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-LIFECYCLE-STATE-CARRIER-001
status: VERIFIED
change-kind: NATIVE_LIFECYCLE_CARRIERS
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Tools/NTSD28Parity/TraceContentIdentity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4StatusMotionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitResourceSuppressionCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4SourceCountCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5SpecialHitLatchCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FJStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25ResourceDisplayCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28CollisionYReferenceCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28EnvironmentStateCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeActionDataCarrierEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeySessionStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05JointSnapshotVersionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeLifecycleCarrierEditorTests.cs
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
authority: Formal EntityState28 independent runtime_state_code/lifecycle_resolution_pending/lifecycle_code and frame/lifecycle/definition/break writers; not render_phase_008.
evidence: Original2676 frame/lifecycle outputs; source4956/8319 and exact raw missing entries.
-->

# 生命周期状态与raw映射前置

IN_PROGRESS / TEST_FIRST。准确路径见元数据。三独立Runtime字段NativeRuntimeStateCode(int0)、NativeLifecycleResolutionPending(boolfalse)、NativeLifecycleCode(int0)，Reset/canonical copy/ECS fingerprint/full checksum/full parity/claimed+raw snapshot。新增payload需要entity15/aggregate23/checksum26，shell2/2不变，native capture与工具meta同步。Raw50已有三项null改真实字段，verified47/missing3；不把PendingFlushDestroy或HitStop当对应字段。仅保留platformSourceSlot/environmentState/environmentSourceSlot三缺失。

来源已在父Task逐项追踪：runtime_state_code是独立模型scripted/diagnostic state，不能写render_phase_008/HitStun；pending/code来自frame和weapon break，生命周期消费清除或despawn，definition成功清零。字段没有独立manager或queue，沿实体Reset/既有十一阶段关闭。先数据单元RED/default/copy/raw alias拒绝/两profile恢复/前版14及22拒绝，再实际父frame生产接线。

旧测试仅更新准确schema14/22/25与binding44/6到15/23/26和47/3，raw旧MISSING断言改为已映射但与既有错误alias不同。历史snapshot13/21、checksum23/24拒绝保留，新增上一版25拒绝；其他数值断言不改。新测试覆盖新字段的独立fingerprint/checksum/raw/restore，不能把data通过当完整规则通过。工具88、native/Unity新fresh capture头及剩余3MISSING、父frame2676、相关schema/回放、SelfCheck、实际场景恢复与关闭按父集成进度执行。所有修改前preimage记录。

本Record不占父LF2Entity/Module路径；这些consumer在既有FRAME-TRANSACTION-INTEGRATION Record下修改，以Runtime真值替换private临时shadow，Begin/End不清真实pending。不能留两套真值。当前中间窗口未验完不发布baseline。禁止computer-use/非战斗/Unity-GAS/Scene/资源/Server/Gen/Plugins，回滚须批准仅本差量。

RED7/7失败已留证：5缺独立字段/2旧header仍可接受。25准确路径已CODE_WRITTEN；三字段及15/23/26/2/2和raw47/3同步，尚待父core改用真实字段和验证。

初轮35项33PASS/2FAIL，原2676两端点22字段全部通过；两失败在新fixture聚合capture，因为未推进World却请求tick1。已按World实际tick0捕获/恢复/checksum，raw字段投影用其API要求的正样例编号1，仅检查数据映射。未改生产capture校验。

事前补充第26路径EntityFieldContract.cs：真实native/Unity比较拒绝unity-binding-status-mismatch，工具字段清单仍把三项标MISSING。只同步三项真实字段路径及VERIFIED映射，形状50不变；之前工具合成self-test不能单独证明Unity新capture接入，拒绝报告留证。

## 当前验证追加

# 生命周期状态与帧尾部当前验收

FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / SELF_CHECK_PENDING_WEAPON_PIECES。父FRAME-TRANSACTION-INTEGRATION仍IN_PROGRESS。不能宣称完整frame或完整战斗对齐。

## 实际变更

本数据Record准确26脚本：NativeRuntimeStateCode、NativeLifecycleResolutionPending、NativeLifecycleCode独立存储，默认0/false/0、Reset/canonical copy/ECS fingerprint/full checksum/parity/claimed+raw snapshot；联合版本15/23/26/2/2。Raw50已有三项从null改真实字段，47已绑定/3MISSING；工具EntityFieldContract最后补同步，未别名到HitStop或PendingFlushDestroy。旧schema/旧checksum拒绝与既有字段数值断言保留。

父Record实际生产接线：private临时terminal shadow移除，Begin/End不清Runtime真值；core arm pending/code、pending入口不重入。C25资源拒绝pending，definition成功重置pending/code/sound。普通OPoint在pending时跳过；state18读取Native前一/当前frame后提交078；终止移到cleanup之后，encoded写独立state code、清current/collision、保留action_latch及078，不改HitStun；负Link不再阻止已arm的生命周期消费。broken gate限定type1/2/4/6、标code1000后由统一生命周期移除，但两类fragment生产仍缺失。

## 证据与范围

- 初始7/7 RED：三字段缺失和上一版14/22仍接受。初轮35中33PASS/2FAIL为新fixture未推进World却请求snapshot tick1；改为当前tick0，保留raw映射正样例编号1，未修改生产capture门。
- fe9a9025ed794c68a8a409d2f6f5d4c8：41/41 PASS。原2676向量已扩到22字段（含pending/code及lifecycle后survives/current/latch/collision/078/state），仍零差异；9组调用原frame及lifecycle两个端点，不冒称其中未执行的driver particle/fragment。6个实际Late测试各覆盖Legacy/DataOriented两路径，验证857/998存活、999解析、1000删除、1101重置及成本fallback latch1101/原声音顺序/原render phase4保留。
- fd4e0a10b7174cbfad2a443ab8fcac22：386/386 PASS，395.532秒；39请求selector均实际执行，包含snapshot/回放/schema/raw/资源/两profile及上述frame测试。related-386-pass.xml。
- 工具5组88/88；真实capture第一次被unity-binding-status-mismatch拒绝，原因工具EntityFieldContract仍标三项Missing。事前扩展第26路径后修正，最终14 raw+其余74全通过。fresh native/Unity内容及15/23/26/2/2头完全相同，3tick/6实体/300字段出现：47字段相等，3MISSING共18差异；first combat.platformSourceSlot。不是无剩余差异，且sound latch仍不在raw50表。
- 完整SelfCheck本次实际运行FAIL（20:31:39Z）：CheckQueuedObjectPointPassBoundaries:20349仍期待PendingFlushDestroy；其OID100无fragment假设又与当前source内置5片冲突，当前producer确实未实现，不能仅改断言变绿。失败文件保留SelfCheck-initial-fail.result。本次请求写入成功，但请求时间归档命令漏Value且原请求已消费，未保存精确请求UTC；未因归档失败重复发请求。
- 真实旧内容Scene tick5暂停边界：注入并恢复Native state=-99/pending/code1101的完整snapshot，随后实际World.LateEntityUpdateAll执行编码重置为state=-1/current0/0781101/collision0/pendingfalse/code0，原始World checksum恢复，实体4→4。play-lifecycle-pass.json。属于实际C25 late入口和状态恢复，不是输入/物理全tick或碎片表现验收。
- 既有Q05恢复/Renderer保留与有序关闭PASS，World/slot/logic/render borrower全0，两帧Stopped，正常退出Play；结果mtime晚于cleanup请求。最终CS0/Editor idle、Scene dirtyfalse/root14且用户HUDBg x30哈希保持。源码与正式EXE未改，未使用computer-use/未开第二Editor。

## 下一唯一执行

NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001 / READY_SOURCE_WITNESS_AND_EXACT_RECORD。必须同时闭合内置和DAT两阶段生成、真实spawn初始化/RNG/slot/时序及SelfCheck fixture的native合同，再回父Frame事务剩余全面driver/资源回放与场景验收。当前不是用户授权阻塞，不再重复做已通过的carrier单元测试作为替代进展。

当前schema处于frame campaign未完成窗口，不发布最终baseline。剩余3MISSING/其它frame reader/definition与fusion完整准入、display其余出生/post、Q07资源及Q10播放仍由总表继续跟踪。总目标ACTIVE。

后续更正：WEAPON-PIECE-TRANSACTION已有两阶段producer、49联合及真实Late四向量/关闭通过；旧“producer未实现”不再是当前阻塞。完整SelfCheck已越过武器，现GT08旧encoded/HitStun fixture待独立核验；carrier仍保持原限定FOCUSED/SCOPED Play状态，完整总目标未关闭。

2026-09-21 final reconciliation VERIFIED / declared carrier+consumer scope. Existing386 XML/actual lifecycle Play/full-driver Play224 and zero-close inspected; successor weapon fragments and stableSelfCheck close original blockers. See EXIT-RECONCILIATION.md and pinned evidence JSON. No rerun/production changes; populated cross-World epoch, raw3 and Q07-Q12 boundaries remain explicit.
