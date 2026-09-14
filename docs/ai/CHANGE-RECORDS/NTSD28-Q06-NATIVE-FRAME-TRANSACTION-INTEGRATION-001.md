<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001
status: IN_PROGRESS
change-kind: NATIVE_FRAME_TRANSACTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeFrameTransactionEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleRecoveryStatusWriter.cs
authority: Formal playable C25 BattleWorld28 step_frame_slot and lifecycle, FrameMachine28, original2676 witness and CALLER-MAP.
evidence: Pre-change original vectors and exact Unity live markers/owners; signed costs and action mirror differences confirmed.
-->

# 正式C25帧事务接线

IN_PROGRESS / TEST_FIRST。准确四脚本，先以原2676输入构造Unity实际core调用RED，再改正式C25入口；direct compatibility仍走原方法，既有C25 marker由真实late slot loop包住optimized/virtual两入口，不改变Mono/GAS架构。

LF2Entity增加原子C25 frame body：Native frame解析、link/hold/terminal14门、type3 drain（kind2仅抑制drain）、源wait/next/latch/counter、212 side effects/正负999/YReference、sound latch+前20声明事件、原destination负MP/HP成本/消费统计及maxHP除3、post-cost terminal。复用BattleCharacterActionWriter.AdjustNativeMpCost，唯一变化改internal static以共用signed调整，正输入支付条件不变。

新增一个与既有renderPhaseTransitionArmed同寿命的private transient terminalPending结果：Begin清零，End(out bool)取出并finally清零，Reset清零；仅在C25g到同一slot后继内由局部bool持有。snapshot中途捕获已有WorldBusy硬门，故不是跨tick状态，不加入runtime/schema；必须用退出后值/重复Begin/exception finally测试证实。旧End()重载保留原语义。不得把尚未绑定raw lifecycle两个字段假标为已绑定。

BattleLateEntityLifecycleModule消费terminal结果，不再用无条件857判存活；编码重置按原动作/latch/collision/previous078区别处理。完整driver OPoint、state18、weapon pieces、mirror/lifecycle顺序仍为本Task必要出口，不能仅靠core矩阵关闭Task；现有broken weapon缺fragment producer须读具体source后明确追加范围，未补齐之前本Task保持IN_PROGRESS。保留十一阶段关闭顺序。

源码frame machine直接读definition wait/next。正式C25不要把旧Trans.SetWait/SetNext覆盖当authority；当前阶段保持兼容Trans储存并只在实际绑定/提交时同步，是否存在需要迁移的live request producer用具体caller检验，不以全局预先重绑掩盖问题。negative wait以source error处理；转场成本gate来自World.FunctionKeys.HitResourceEnabled，不使用旧PpMode别名推断。出生data和sound latch已由前包建立，definition/fusion成功重置仍需按真实owner接通。

验收包括core2676逐字段（action/latch/counter/facing/vitals/cost/motion/sound）及两路径、全late/完整driver terminal+encoded+OPoint/particles、snapshot replay、SelfCheck、真实高位动作完整tick、清理全0和非战斗保护。基线14/22/25/2/2；无持久新字段时不升版本。旧138真实Scene与Logan正式内容fixture分别标记。测试先输出完整差异汇总，不把单项API缺失当完整行为证据。

回滚须批准仅本差量并保留用户工作；不改Scene/InputActions/资源/Gen/Plugins/Server，禁止computer-use。当前只给已查明四路径权限，新增脚本/生产路径必须事前扩展准确Record。

RED实测：9组/2676例全部组失败，逐字段差异共9103（lookup13/next775/counter55/cost6832/modifiers596/gates25/terminal14=140/type3=655/already212=12），red目录保留完整输入/输出。接下来写正式C25 core和terminal handoff；完整driver/weapon pieces/definition/fusion仍是父任务未关闭出口。

CODE_WRITTEN：正式C25 marker分支已接Native数据/core+独立声音采样和完整负成本，direct compatibility保留；瞬态terminal输出End(out)已写/Reset清零，module消费接线进行中。跳跃优先现有NativeMetadata.Bmp double，旧138无metadata保留其内容字段。

关键未完成合同：source materialize_state18_broken_weapon_particles没有统一pending早退；声明999的current frame仍可参与state18分支，所以terminal不能直接提前free。source resolve_pending_lifecycles写runtime_state_code，与render_phase_008为不同字段；现Unity Handle写HitStun不能代替独立载体。先验证已写core，再闭合该持久载体/生命周期结果消费及particles/weapon pieces；当前Module尚未修改，仍旧857退出，故禁止把当前中间代码当完整高位动作运行时交付或运行真实高位Play。Task保持IN_PROGRESS。

## 当前实施证据（任务未关闭）

# C25帧事务实施中检查点

**IN_PROGRESS / CORE_FOCUSED_PASS / FULL_TRANSACTION_INCOMPLETE。** 不能发布为完整frame对齐，不能把当前core通过推广到真实高位动作完整tick。

## 本轮实际实现

LF2Entity的正式nativeC25FrameTickActive分支新增RunNativeC25FrameTransaction。未带marker的direct compatibility继续既有逻辑，真实late loop的optimized/virtual均进入marker。Native getter解析当前/目标帧；原link/hold/terminal14/type3资格；负next翻面/正999二次YReference/212 advanced判断；读取源DAT wait/next且区分latch与counter；独立sound latch/head与destination前20事件；原destination signed MP/HP及fallback/消费统计/maxHP除3。跳跃读取现有NativeMetadata.Bmp binary64，无metadata旧内容用既有字段。BattleCharacterActionWriter.AdjustNativeMpCost仅private→internal复用signed调整，原输入支付算法未改。

private transient terminalPending与End(out)已写，Begin/End/Reset清零，但尚未被Module消费；此中间设计将由下一生命周期持久载体替换，不留两套真值。Module文件本轮尚未修改；它仍用旧857边界并将encoded结果写HitStun，未满足原版合同。因此尚未完成源step/lifecycle两个端点或整条driver串联。

## 新鲜证据

- 编译idle/error CS0。
- RED job3b2e788356d8456296ba2e7448a1023e：9组2676例，14项核对字段总9103差异。红色输入/输出JSON保留red/；所有组失败，非仅API缺失。
- 实施后2070be80d2314702b50cc9ee197a182f：9/9 PASS，2676例×14字段全部相同，5.705秒。字段为action/latch/counter/facing、HP/MP/maxHP/两consumed totals、三速度、sound latch/声音数。没有据此宣称status/terminal/粒子/完整driver全部相同。
- 相关job7bd2b0b4ebb44f5f9347174cac9ac991：19中18PASS/1FAIL，旧夹具假设action202自动写phase20。source唯一phase20赋值在advance_native_revivals且选择212，step_frames_range没有该writer；独立C25-RENDER-PHASE-20-FIXTURE Record将测试改为前置20→19、bare202保持0并保留new15不递减，未恢复错误production writer。
- 最终48136c7e3cc04b47aa7929dab254942c：core9+原相关19=28/28 PASS，6.216秒；9组JSON仍2676例/0差异（core-validation.json）。原19覆盖direct compatibility和两native进入方式。
- 本轮未执行完整SelfCheck、真实高位Play、完整driver trace或新的关闭验证，因为事务尾部仍未接完；之前344/SelfCheck/Play是前包数据基线，不算本轮新生产验收。
- Scene用户HUDBg x30/SHA bcd1047b…保持，无Scene/资源/非战斗/GAS/Server/Gen/Plugins变更。14/22/25/2/2仍当前版本。当前生产两个文件有新差量、一个新测试；第四个声明Module是后续接线范围。current-code-scope/diff文件保留准确差量。

## 必须接续的工作（不要求用户再确认）

下一必要Task NTSD28-Q06-NATIVE-LIFECYCLE-STATE-CARRIER-001 / READY_FOR_EXACT_PRECHANGE_RECORD：闭合独立runtime_state_code/pending/code和snapshot/copy/checksum/raw映射，再立即返回本Task。source runtime_state_code与render_phase_008不同，battle_world.cpp:4963和native_ai_tests明确禁止别名；不能沿用HitStun写入。若新增持久三字段，按新Task实施联合版本15/23/26/2/2，并将临时private结果替换为Runtime真值，Begin/End不得清真实pending。

接线顺序必须完整：普通zero-frame OPoint被pending拒绝；state18 particles没有统一pending早退，声明999可有current frame；然后previous078提交、真正broken weapon的两类fragments、最后lifecycle。code11xx/12xx清current及collision镜像、保留latch，不修改render phase。健康负link由framebody不arm pending而存活；已arm的broken held weapon不能被旧负link保护无条件保活。旧weapon cleanup只sound/flag，fragment producer仍未实现，本Task因此保持IN_PROGRESS。

完整事务还需补判定结果/marker生命周期、source同输入全driver/OPoint/particles/fragment、snapshot replay、SelfCheck和真实高位动作完整tick/关闭全0。definition/fusion成功重置sound latch也须按已确认业务owner接通。保留Q10实际WAV效果回访及Q07资源迁移依赖；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

事前扩展第5路径BattleRecoveryStatusWriter.CanEnterNativeResource加入真实NativeLifecycleResolutionPending拒绝，source明确资源pre跳过pending；父LF2Entity/Module消费由此Record负责。原临时private结果改为三字段Runtime真值，Begin/End不清pending；definition成功清pending/code和sound latch，state18读取Native当前/前一动作。

CODE_WRITTEN：父core现写Runtime.NativeLifecycleResolutionPending/Code；private shadow已移除，Begin/End不清真值。Module终止移到state18/078/weapon cleanup之后，pending拒绝普通OPoint，encoded写独立NativeRuntimeStateCode并清collision镜像保留latch，不再写HitStun；资源资格加入pending拒绝。broken gate限type1/2/4/6、交给统一生命周期，原fragment生成仍未接，父Task未关闭。definition成功重置sound/pending/code；具体完整资格继续回访。

## 生命周期接线检查点（仍IN_PROGRESS）

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
