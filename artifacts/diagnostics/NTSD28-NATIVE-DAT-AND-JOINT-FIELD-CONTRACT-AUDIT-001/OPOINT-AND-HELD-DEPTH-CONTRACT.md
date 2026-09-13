# Q03 OPoint与持有武器深度消费合同

2026-09-13。状态：CONSUMER_PATHS_RECORDED / PRODUCTION_UNCHANGED。本合同补齐Q03内容字段去向，不能代替Q06行为/Play验收；父Q03联合版本总表仍未冻结。

Authority路径以正式Logan的source/ntsd28_core为根；Unity路径以Assets/NTSD/Scripts为根。依据为本轮实际读取的当前源码，不以旧地址注释推断规则。所引simulation源均参与当前playable build。

## OPoint 24字段的统一内容定义

`ObjectSpawnPlanner28::decode`在src/simulation/object_spawning.cpp:15逐项读取；全部是有符号int32，默认0，同名字段采用精确大小写最后一项，非法或越界整数按FieldBag.integer归零。source_line是诊断元数据，不列入24项。字段本身没有可变生命周期；值对象防御复制，任务复用时整个OPoint值归default。

| 字段 | 权威消费 | 当前Unity传递/缺口 |
|---|---|---|
| kind | materializer仅1/2；1启用baseDVZ、random、positive effect；2建持有关系 | Value/DTO有；两个late producer只拒绝<=0，不能用此代替native仅1/2准入 |
| x | parent整数X±(pointX-frameCenterX) | Value→legacy→ConfigureLateOpointPosition已传递 |
| y | parent整数Y+pointY-frameCenterY | 同上 |
| z | parent整数Z+pointZ+1 | Value缺失，两个producer固定Z+1 |
| action | 使用DatDocument::frame解析后作为初始action；显式声明优先，未声明0..998返回native零帧，其他未声明ID才返回null | 有字段；必须复核Unity同一lookup合同，不能将未声明0..998当作缺失动作拒绝 |
| dvx | 由child facing决定符号；随后multi spread改变X | 字段存在；spread和初始化顺序需与native联合验证 |
| dvy | 初始Y速度 | 字段存在 |
| dvz | 只有kind1作base Z，system表修正后再加spread | Value缺失；ToLegacyTask强制dvz0，late task.dvz0，其他对象initializer也将late VZ清0 |
| oid | <=0不产生intent；找不到registered definition终止本frame整个OPoint循环 | 字段存在；Unity找不到对象返回null后当前循环继续，须独立终止结果，而非只统计错误 |
| facing | >10为count=facing/10、mode=facing%10；mode0继承，1取反，其余native false | 字段存在；两个producer拆count/mode，initializer CalculateDirection需按本合同核验 |
| hp | 正值优先，否则OID5/52为10，其他500；stats.ohp>0按int64乘除；kind2另写weapon_hp_31c | Value缺失；两个factory先默认Initialize，随后OID5/52又固定写10，不能仅加入口参数 |
| mp | 正值优先，否则OID5/52为5，其他500；stats.omp>0按int64乘除 | Value缺失；同上默认及后处理覆盖 |
| team | 0继承parent battle_group，否则显式值 | Value缺失；两个producer固定parent.Team，两条PostInitLiving无条件再次继承Team/RelationTeam，kind2再写一次Team |
| reserve | child type0/5且parent credit gate!=2时写revive_lives_30c | 缺失；不是永久默认lives，也不是所有子对象类型都写 |
| effect | kind1且>0覆盖render_phase_008，发生在type0继承parent phase之后 | 缺失；不得把它解释为任意hit effect |
| pic | decoder保存；当前playable未找到OPoint规则reader | 保留内容/identity，不新增视觉效果假设 |
| centerx | kind1正幅度时消费两次同步RNG，给X有符号随机偏移 | 缺失；不是改child frame center |
| centery | 同上，Y；顺序在X后 | 缺失 |
| centerz | 同上，Z；顺序在Y后 | 缺失 |
| framea | kind1随机action偏移；kind2等于1时parent interaction_state=101，否则1 | 缺失；不能只有随机含义 |
| attacking | decoder保存；当前playable未找到OPoint规则reader | 保留内容/identity，不套用WPoint同名字段语义 |
| join | 同reserve条件写revive_next_hp_314 | 缺失 |
| join_reserve | 同reserve条件写revive_next_lives_310 | 缺失 |
| join_pic | 同reserve条件写revive_visual_id_184 | 缺失 |

reserve四项条件直接读battle_world.cpp:7766代码：type0或5且parent ordinary_credit_gate_2f4!=2，无额外kind1条件。附近写着kind1的注释与实际分支不同，以代码及正式行为为准。

## 正式入口、时序和两条Unity路径

native `SimulationTickDriver28`以live slot升序在单实体tail调用`materialize_native_frame_zero_entry`，要求当前frame_counter==0且非lifecycle pending。opoint_action_latch只是trace provenance，不抑制同action零计数重复生成。新生成更高slot可在同tick进入自己的tail，已经越过的低slot留后tick。

`BattleWorld28::materialize_supported_spawns`先组装parent整数位置/frame center/facing/group/literal owner，再按source block顺序规划。kind1/2之外跳过；OID<=0在planner跳过；缺definition或无空闲slot **break整个intent循环**；DatDocument::frame返回null的action为rejected并继续后续intent。该lookup先取首个显式同ID frame；若无声明，0..998取零初始化frame，其他ID才null（不能简单把所有>=999视为无效，显式声明优先）。先分配成功，随后才运行kind1随机，避免容量不足时错误消费RNG。

随机helper `native_opoint_random_delta`（battle_world.cpp:71）：amplitude<=0不消费；正值先`synchronized_next(0x0044D3AB, amplitude)`，再`synchronized_next(0x0044D3B5,50)`，后者>=25则取负。每child严格X/Y/Z/action顺序。

创建后顺序：随机并同步precise位置 → 继承+1A0 → parent suppression==1传播、type0强制suppression1/credit/phase → effect及revival四项 → stats.defend形成+340 → facing/motion → system Z规则处理base，再加multi spread → kind2 link/parent state/weapon HP → 发布本次事件。已有literal owner/credit producer验证保留，不重做或搬到错误的后注册补写位置。

Unity正式owner边界：`BattleStructuralWriter.ProcessLateOpointSegment`调用`IBattleObjectPointStructuralMaterializer.ProcessOpointSpawnCoreForStructuralWriter`，设置CurrentEntityImmediate事务边界。

| 路径 | producer | materializer | 初始化与后处理 |
|---|---|---|---|
| logic-only | Simulation/Runtime/BattleLogicObjectPointRuntime.ProcessOneLateOpoint | BattleLogicEntityFactory.Create | entity.Init(task,null)；角色ModuleBind/Initialize；PostInitLiving；ApplyReleaseOpointDirectionalVz；ApplyDirectVelocity |
| 带renderer | Animation/Character/LF2ObjectPointFactory.ProcessOneLateOpoint | 同类MaterializeObjectForStructuralWriter；logic-only world会转交LogicEntityFactory | pool取renderer与logic，SetLogicObject触发Init；角色ModuleBind/Initialize；PostInitLiving；方向Z与direct velocity |

上述两条producer当前在每个OPoint处理完后各自执行multi exemption/vrest；Q06需保持已确认规则副作用，不能因新字段一次重写对象池。旧`LF2ObjectPointModule.ProcessFrame`只有定义未找到当前生产调用，不作为这次正式入口；其旧HitStun/OwnerId/ShotCount gate不能无证据移入新路径。

内容到任务复制链：`BattleObjectPointValue`8项 → `BattleObjectPointValueAdapter.ToLegacyTask` → `ObjectPoint` → `OPointCreateTask.opoint`。多发复制在`BattleLogicObjectPointRuntime.CopyMultipleTaskToSingle`、`LF2ObjectPointFactory.MaterializeMultipleObjectsForStructuralWriter`；两种task.Clear均将opoint置default。Q05应使24项在这些边界完整值复制，Q06再消费；不得让pos.z、task.dvz和point.z/dvz成为互相覆盖且未声明优先级的双份真相。不同ReleaseSpawnSemantic的非OPoint创建保持现有边界，不对普通factory全局套用OPoint规则。

## 持有武器：深度选择与伤害替换分开

native candidate阶段battle_world.cpp:4300～4356：攻击者type不为0/3/5、linked_parent_slot在合法范围、holder存在且interaction_state非0时，读取 **holder当前frame** 的首个WPoint；缺WPoint的first_weapon_point是全零record。按WPoint.attacking查 **攻击武器definition的weapon_strength行**，缺行或缺zwidth/z字段采用0。传给HitCandidateBuilder的attack_depth_override在几何投影前替换ITR深度；zwidth==0再回退15，Z中心为attackerZ+selectedZ。X/Y仍是武器自身ITR。

这与消费阶段kind5替换不同。Unity `BruteForceSceneQuery.ResolveRuntimeItrForPair`中的holder collision-frame ITR替换发生在候选之后；不能拿它证明候选深度已正确，也不能把新深度选择挪到命中后。

当前Unity `WeaponStrengthEntry`只保存index及dvx/dvy/fall/vrest/arest/bdefend/injury/effect八个数值；`CharacterAnimtorManager.ExtractWeaponParameters`从top-level weapon_strength_list读取同八项。factory把这个list传给weapon，并没有补齐缺失项。`GetStrengthEntry`拒绝<=0且按index首个匹配，缺行返回null；native DatDocument::weapon_strength_entry按index首个匹配且没有<=0拒绝。新formal reader必须按native查行/default规则，不能盲复用该旧方法。

native `CombatRecordDecoder28::weapon_strength_interaction`还定义19项替换：dvx,dvy,fall,arest,vrest,respond,effect,drain,spark,recover,dbdefend,bdefend,injury,zwidth,z,dvz,sound,cover,caughtact。caughtact是有效最后字段的首整数，其余strict int32/default0。Q03联合内容范围纳入19项+行index；candidate只消费其中zwidth/z，伤害consumer按其已确认时点消费完整19项。

`LF2Weapon.ProcessAttack`和`ProcessAttackInternal`是保留的兼容代码，本轮搜索只找到后者定义、未找到生产调用；不得把其中旧WPoint→ITR拼接当作正式深度reader或顺手扩展该无调用路径。正式修复应接到candidate产生前的实际三模式/缓存路径。

## identity、snapshot与实施出口

当前LoganObjectCatalog DefinitionFingerprint V1是catalog hash+registry order/DAT path/hash，不是normalized字段hash；同一DAT改了decoder语义，raw fingerprint不会变化。Q05必须另行明确解码合同版本纳入发布/cache/会话可比较身份的映射，不能仅依赖源文件hash宣称两端语义相同。LockstepSessionIdentity接收ulong catalogFingerprint，其与Logan字符串指纹的生产绑定仍需总矩阵核验，不能把两者视为已自动互通。

PendingEventSnapshot当前schema1只保存sound并要求unregister/destroy queue为空，不序列化OPointCreateTask。24个内容字段不应机械变成24个entity snapshot字段；需要保存的是生成后的runtime状态。正式capture边界是否明确排除未消费OPoint queue仍列入联合snapshot owner核验，不能假定PendingEvent已覆盖它。

Q05合同增量：OPoint24完整值传递/复制/reset、weapon_strength19+index、BDY zwidth/有效几何、CPoint27含float32及对应content-version identity。Q06独立行为出口至少覆盖显式team不被覆写、hp/mp与百分比在初次注册时正确、kind1随机顺序、kind2 framea/weaponHP、reserve四项、base Z先规则后spread、缺definition/容量终止后续record、不同slot生成可见边界，以及三collector的held-depth选择。每个producer/consumer实际接通即按R回访，不等整组结束。

这份合同只记录静态字段/owner和已有见证依赖；本轮没有新增编译/Play结果。Q03还需完成canonical float、全reserved/AI投影、identity与snapshot边界矩阵才能关闭。


## 2026-09-13 更正：表头准入与运行时查表分开

已核实正式source/ntsd28_core/src/data/dat_parser.cpp第438～480行：weapon_strength entry必须严格为1..9，重复编号为parse error并清current row；entry行其余文本是caption，不是该行字段。object_catalog.cpp第217～224行通过document.ok()拒绝含parse error的整个definition。故本任务此前“index含0/重复原样保存”不得用于native DAT准入。runtime weapon_strength_entry无<=0检查仅说明lookup函数，不构成文本入口可接受0或重复的证据。旧原始AST可留存诊断，正式模型不纳入这些非法行。完整source parser与table接线须按此合同后继验证；不能简单使用当前扁平properties丢失行边界的旧AST来宣称caption/重复准入一致。
