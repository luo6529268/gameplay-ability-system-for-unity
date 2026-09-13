# Q03-B 联合字段矩阵（进行中）

状态：CONTRACT_FROZEN / IMPLEMENTATION_PENDING。日期2026-09-13。此表记录实际静态路径，不是schema迁移交付证据。默认仓库路径以Assets/NTSD/Scripts为根，native路径以权威source/ntsd28_core为根。

## 当前载体与后继窗口

| 项目 | 目前已确认路径 | Q04/Q05/Q06责任与未决项 |
|---|---|---|
| Native +2F8 | include/ntsd28/battle_world.h:366，int默认-1；battle_world.cpp:1248构造EntityState28，:1294发布slot，despawn:1519清optional；:8032 held-release parent WPoint DVX非零及type1/4/6写parent slot；native_ai.cpp:290读取该slot实体的battle_group作为common target排除组 | 拟用独立runtime字段承载，默认/reset为-1；Q05加copy/快照/hash/derived projection，Q06精确写/读。不是owner_slot，也不是固定缓存的group值；源slot重用会读当前实体group，不能擅自改成stable id |
| Unity SpawnerSlotIndex | Core/NTSDEntityRuntime.cs:115默认-1，:814复制，:1052重置；LF2Entity.SpawnerEntityIndex公开包装；held resolver:133写holder，weapon frame resolver:230及LF2Entity:2388读取；RespawnModule:261、LateEntityLifecycleModule:780也写 | 不能整体改名当作+2F8。先将held/AI两端绑定新字段，其他Spawner语义需逐项判定保留；当前整体不得删除 |
| Spawner派生/验证 | Ecs/Core/BattleEcsWorld.cs:608复制Identity.SpawnerSlot，:862验证，:1101hash；Checksum/BattleLockstepChecksumModule.cs:563；BattleParitySnapshot.cs:1086 | 新+2F8独立进入同链；不得覆盖Spawner导致其他现有行为变化。哪些AI sensing投影需新字段仍待完整审计 |
| mass | CharacterMechanics.cs:374 startedGrounded &&ctx.mass>0；LF2Character _mass/查询/restore与MassForFrameAdvance；ECS CharacterFrameAdvancePass:123；CharacterShellSnapshot:14/22/35，schema1 | native physics_integrator.cpp:102起grounded摩擦无mass gate。Q04移除行为gate；Q05协调清载体、context、shell capture/restore并将shell1→2。复用既有mass owner审计，不删NTSDGlobal等无关API；正式type0旧内容mass1不能证明额外gate合理，也不能声称已测出正式角色bug |
| CPoint内容 | DataContracts/CatchPoint Value19int；Catalog ScalarsPerEntry19，CopyCanonicalScalars按固定顺序输出；Converter有hurt→injury/cover alias；selfcheck及FormalKernelCatchPointValueSeamEditorTests消费canonical数组 | 新27项含3float32；canonical输出须明确bit编码、顺序、identity/fingerprint影响及测试。内容字段不是直接新增27个runtime字段；当前catalog复制值结构，不另建可变双份真相 |
| OPoint内容 | DataContracts/ObjectPoint八项；Runtime/BattleLogicObjectPointRuntime -> ToLegacyTask -> task -> factory，以及独立presentation路径 | 24项值对象、legacy task复制/reset、候选content fingerprint与materializer分别声明。24项不等于新增24项entity快照；只将生成后需保留的运行值映射现有/新runtime载体 |

## 五类reserved目前仍在的存储（不是已删除）

| 字段 | runtime默认 / copy / reset行 | ECS派生或hash | checksum / parity | 当前解释 |
|---|---|---|---|---|
| GrabbedBy | 116零 /815/1053 | Links数组356/368，727复制，776清，958比对；hash1102 | 当前两模块未找到同名项 | LF2Entity包装及同步自赋值仍在；先验Goal20退休断言，Q05连同ECS删除，不能伪称checksum本来有此字段 |
| ReleaseTick | 123为-1 /822/1060 | hash1105 | checksum542 / parity1058 | stampReleaseTick参数仍存在不等于在写字段；需逐项核验兼容参数实际无行为，然后在准确窗口清理 |
| HolderCopySlotIndex | 231为99 /933/1113 | Links.HolderCopySlot:721、952；hash1121 | checksum537 / parity1053 | LF2Entity包装及HitExecutionPlan诊断TargetHolderCopySlot仍引用；旧kind5/type3行为已退休，不重做；snapshot/hash删除与diagnostic同步 |
| TrackerFlag | 238零 /935/1115 | Links数组357/369，728复制，776清，959比对；hash1122 | 当前两模块未找到同名项 | 角色/其他/武器初始化仍写0，LF2Entity同步自赋值；不得与真实TrackerParent生命周期混同删除 |
| WeaponState | 384零 /1011/1188 | hash1150 | checksum634 / parity1207 | 武器初始化置0仍在。GetResolvedWeaponStateForExternalUse的方法名称不能证明读此字段，必须按实现区分真实frame state |

全表runtime路径为Simulation/Core/NTSDEntityRuntime.cs；ECS行为为Simulation/Ecs/Core/BattleEcsWorld.cs；checksum/parity在Simulation/Lockstep/Checksum。行号是本次读取位置，后续以符号检索为准。

## capture/restore与版本约束

`BattleWorldEntityRuntimeSnapshotBuffer`（当前12）分别保存claimed entity和materialized raw slot，各通过`TryCopyCanonicalStateTo`复制。raw slot与entity runtime要求独立对象；新字段必须在两者copy/reset链覆盖，不能只改公开entity属性。

`BattleStateSnapshotBuffer`（当前20）核对子buffer schema、tick和session identity。`BattleStateSnapshotRestoreModule`先做schema/identity/world状态与依赖验证，再恢复runtime及各shell，最后重建derived state。character shell保存Mass并通过`LF2Character.TryRestoreCharacterShellForSnapshot`恢复。新版本不允许将旧midbattle snapshot填默认后接收。

D-022已决定entity12→13、aggregate20→21、checksum23→24；实际必要shell同步迁移（已确认character1→2），内容版本/identity和trace descriptor另作明确映射，不凭字段数量隐式推出版本。当前全部保持原版本。

尚需闭合：完整shell名单；+2F8在AI快照/SoA/fast与fallback链；CPoint/OPoint及新增BDY字段的content identity入口；所有reserved在test/diagnostic/compat API引用与真正reader零计数；canonical float规范；版本拒绝、双端同版本trace、同seed输入重放和pool复用测试清单。完成之前不能将Q03-B标为FROZEN或启动Q05。

## 几何见证后的合同增量

`NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001`已测得14普通type0用例中9个两端不同，Unity三模式一致；详细42项在该包comparison.json。`DataContracts/BodyBox/BattleBodyBoxValue.cs`/Adapter与Converter需要保留native实际消费的zwidth及缺失几何有效性；显式w0仍可生成候选，不得用w>0替代presence。BDY z在native不影响中心，不盲加同名效果。ITR raw zwidth默认0与投影effective15分开，精确query与broadphase/cached矩形同步消费ITR z和body depth。以上是待冻结的联合内容合同增量；规则实施与全fast/fallback验收归Q06，不提前改schema。

已追加核实两项reserved误报边界：`LF2WeaponReleaseFlowResolver.ReleaseHeldWeaponRuntime`的stampReleaseTick参数未被使用，只调用ClearReleasedLinks；`LF2WeaponBase.GetResolvedWeaponStateForExternalUse`返回CurrentFrameState，没有读取Runtime.WeaponState。因此方法名/参数名不能当作仍有行为reader的证据；其兼容API最终删留仍在Q05准确路径清单中处理。

## OPoint/held-depth审计后的合同增量

同目录 `OPOINT-AND-HELD-DEPTH-CONTRACT.md` 已逐项记录OPoint24全部字段的source reader、两条实际factory/initializer/PostInit链、task value复制/reset及Q05/Q06出口。显式team和hp/mp不能仅加producer，后处理有再次覆写；native缺definition/无slot会终止整个OPoint循环；frame查询有0..998未声明零帧，不能用DAT声明存在性替代实际frame查找。

WeaponStrengthEntry当前index+8项对native index+19项，缺zwidth/z等实际candidate/伤害字段；`CharacterAnimtorManager.ExtractWeaponParameters`与data/weapon list复制需要进入同一Q05内容窗口。held candidate读取holder当前WPoint的attacking并查武器自己的strength行，缺行是0/0，零宽度随后回退15；不等同于消费阶段kind5替换。旧ProcessAttackInternal没有找到生产caller，不能为方便而把新规则接入该兼容方法。

新需关闭的身份边界：LoganDefinitionFingerprint V1只hash raw catalog/DAT，同原文的decoder语义变化不会改变它；解码合同版本与Lockstep ulong catalogFingerprint的生产绑定要在Q03-B明确。PendingEventSnapshot只存sound，不代表OPoint pending queue已被capture；应追真实capture边界，而不是机械增加24个task snapshot字段。

## 联合版本与identity/capture合同补齐

同目录 `VERSION-IDENTITY-AND-CAPTURE-CONTRACT.md` 已记录准确版本范围、原始/语义身份编码及ulong投影、snapshot已存在入口与OPoint前置条件、五类reserved及任务别名、+2F8两个object-AI reader和CPoint27 canonical顺序/float32 bit规范。`semantic-identity-test-vectors.json`提供三组精确编码向量（规范数据，尚非生产结果）。`reserved-alias-reference-matrix.json`列339处命中：271测试/diagnostic fixture、15runtime存储复制重置、19ECS、6checksum/parity、1hit-plan诊断投影、27alias/初始化/task配置；这些类别不冒充新增行为测试证据。

Q05目前准确版本集合是12→13、20→21、23→24、character shell1→2；其他shell维持原版本。Core11和SessionIdentity/packet1布局保持。本规范实施前仍须完成native数值语法/float32位模式见证与Q03整体出口核对；本表未标FROZEN，代码未改。

最终更正及冻结：以Q03-EXIT-REPORT和VERSION-IDENTITY-AND-CAPTURE-CONTRACT最终表为准，base-shell因旧Oscillate两项同升1→2。37 native数值位模式见证已交付。上文早期NOT_FROZEN/仅character升版/待numeric等过程状态均被本条取代，代码和版本尚未实施。
