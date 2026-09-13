# Q03 联合版本、语义身份与快照边界合同

2026-09-13。状态：CONTRACT_RECORDED / IMPLEMENTATION_NOT_STARTED。本文区分当前代码事实与Q05必须实现的适配要求；不表示版本已升级或快照验收完成。

## 当前版本与最小变动集合

| 域 | 当前 | Q05目标 | 原因/保持边界 |
|---|---:|---:|---|
| EntityRuntime snapshot | 12 | 13 | 独立+2F8载体及五类reserved清除，copy/reset/capture/restore保持一套真相 |
| Aggregate BattleStateSnapshot | 20 | 21 | 协调引用新的entity/character及一致的语义身份 |
| Lockstep checksum | 23 | 24 | 字段集合改变，同版本对比，不能填旧默认伪造相等 |
| Character shell | 1 | 2 | 删除Mass，保留HeldWeaponHandle/DeadBlinkCount/OPoint标记 |
| Core scalar | 11 | 11（目前没有新增scalar的依据） | 33ms、RNG、主循环等既有字段不因内容扩展而盲升版 |
| EntityBase shell | 1 | 2 | 出口复核确认EffectOscillate/EffectOscillateDirection仍能恢复并被晚帧读取，随reader退休删除两项 |
| Living/Weapon/SpecialOther shells | 各1 | 各1（当前字段形状无需改变） | +2F8在runtime，strength是definition binding；不得把TrackerParentHandle与旧TrackerFlag混同删除 |
| RuntimeSlot/Stage/Roster/Rest/PendingEvent | 各1 | 各1（队列边界采用拒绝，非新序列化域） | 普通OPoint task不写入PendingEvent；有pending时拒绝capture/restore，保持tick边界要求 |
| LockstepSessionIdentity / packet布局 | 1 | 1 | 复用现有catalogFingerprint比较槽，不新增wire/transport或网络功能 |

前三项按D-022已批准的唯一协调窗口；必要character/base shell同步。**更正：此前暂定EntityBase保持1的结论不成立。** 当前LF2LivingObject.ProcessEffects仍读取Oscillate并写Sprite位移，LF2Entity恢复两个effect字段；Goal17只退休producer，其原Task最终节明确reader/shell后置。Q04仅退剩余reader，Q05删除两个载体和base-shell成员并升2，不能重做已完成producer退休。若实施发现其他payload变更，必须仍在同窗口的准确Task/Record补充。内容定义CPoint27/OPoint24/BDY/strength另有解码合同身份，不机械转换为实体snapshot字段。

## 已观察的身份边界

`Animation/LoganObjectCatalog`以`LOGAN_OBJECT_DEFINITIONS_V1`、catalog SHA和registry顺序/DAT路径/SHA构造DefinitionFingerprint。`LoganVisualContentCandidate`再加图片输入形成发布SourceCacheKey。它们证明源文件版本，未包含解码语义版本。同一原文在19字段与27字段decoder中可得到不同值而raw指纹不变。

`LockstepSessionIdentity`通过构造参数接收ulong CatalogFingerprint；packet、StrictDelayedInputBuffer、StartBarrier和session identity hash传递/比较该值。当前Assets/NTSD/Scripts内直接构造此identity的8个文件均在Test目录；`BattleLockstepSession`本身由调用方注入identity，`InProcessBattleWorldBootstrap`接收barrier并不计算Logan内容指纹。**当前没有证据表明Logan字符串指纹已自动绑定到该ulong。** 外部调用方不等于无调用方；这里不把S0 seam重新设计成网络启动流程。

Q05必须建立两个明确身份层次：

1. 原始输入身份继续保留，以支持Q01/资源复核；不得将新decoder版本冒充原DAT发生变化。
2. 新的解码语义身份包含明确的解码合同版本和原始内容身份，进入native内容candidate/cache/publication及本地验证会话的可比较身份。继续保存完整摘要；若投影进既有ulong槽，必须固定字节序和投影规则，并验证两端相同，不依赖GetHashCode或进程随机hash。

本次冻结的语义摘要编码：ASCII `NTSD28_LOGAN_DAT_SEMANTICS_V2` 后一个NUL字节，再拼接DefinitionFingerprint十六进制解码所得32原始字节，对该字节串做SHA-256；显示为64字符大写hex，完整值独立保存。投影到既有CatalogFingerprint槽时取摘要前8字节little-endian uint64，若结果0则使用1以满足现有StartBarrier非零门槛。原始DefinitionFingerprint和完整语义摘要仍需一起记录，ulong不充当完整身份唯一证据。未来解码合同改变必须变tag；这里只发布V2这一协调窗口，不改协议布局。

Q05事前Record应引用以上精确字节合同，用“同DAT换decoder版本”和“同decoder换任一DAT”两种用例证明身份变化；发布/cache、比较工具和本地验证会话绑定同一语义值。旧Unity内容/合成fixture的现有调用保留可识别来源，不以空值或常量指纹代替正式Logan身份。此项不授权改变wire布局、InputDelay/TargetTick或外部Server工程。

## 已观察的snapshot入口

- `SimulationWorld.TryCaptureBattleStateSnapshot`（Core/SimulationWorld.cs:862）先Invalidate目标buffer，逐域capture，再TryPublish；此函数没有直接检查OPoint queue。
- `BattleLockstepSession.TryCaptureBattleStateSnapshot`检查protocol error、World存在和driver tick匹配，再调用World；也没有OPoint queue检查。
- `LockstepSnapshotRing.TryCaptureNext`检查capture tick后调用同一World入口。
- `BattleWorldPendingEventSnapshotBuffer.TryCapture`只读取sounds，并要求pending unregister及slot-released destroy为空；不保存任何OPoint task。
- restore入口检查snapshot/identity，并在`_ticking`时拒绝，然后按域恢复和重建derived state。当前所读入口没有OPoint-specific guard。
- logic-only队列由`BattleLogicObjectPointRuntime.PendingTaskCountForDiagnostics`暴露；renderer队列由`LF2ObjectPointFactory.PendingTaskCountForDiagnostics`暴露。它们是不同owner，不能只检查其中之一。

上述是静态入口事实，尚未执行“有pending仍capture成功”的runtime反例，不能将其写成已复现Bug。

Q05边界要求：只在非tick/非structural playback、声明的所有spawn队列均空时capture或restore；未满足则返回明确失败，目标snapshot保持无效，原world/queue不变。**禁止为取得snapshot临时FlushTasks，也禁止丢弃pending来让capture通过。** 队列观察必须使用当前World/lifecycle owner已有引用，不能为校验访问会新建singleton的Instance。若不是该World拥有的全局队列，不得跨World误判；accurate owner接线先列Record。

不把24项OPoint定义序列化成新pending payload；常规late OPoint在结构边界即时产生，其结果进入entity runtime。需要跨tick排队的其他语义仍遵守已有队列及snapshot前置条件，不能借本任务实现新的恢复策略。

## 五类reserved与+2F8恢复策略

`NTSDEntityRuntime.TryCopyCanonicalStateTo`同时服务claimed entity及独立raw slot快照；新+2F8默认/reset -1，两者都必须复制。五类reserved的实际存储、初始化、alias、ECS镜像/hash、checksum/parity条目全部在Q05统一删除；先前行为退休保留，不恢复其旧reader。

补全alias：`LF2Entity.HolderCopySlot`包装HolderCopySlotIndex，角色初始化写99、其他对象/武器初始化写-1。这些初始化也必须进清理清单，不仅搜索底层字段名。`stampReleaseTick`参数是当前不使用的兼容形参，不是实际writer；`GetResolvedWeaponStateForExternalUse`返回当前frame state，不读Runtime.WeaponState。`TrackerParentHandle`仍是有效实体关系，不能因TrackerFlag退休而删除。

任务别名也在删除出口内：`OPointCreateTask.holderCopySlot`声明/Clear、StageSpawnTaskConfigurator、LF2CharacterLateRuntimeModule、BattleRespawnModule与LF2Entity两处task配置当前只写-1，未找到读取它的生产语义。它们必须随Q05清除，不能只删entity carrier留下另一套任务载体。相反，BattleStateSnapshotRestore/BattleLogicEntityFactory中名为weaponState的局部变量是BattleWeaponShellSnapshot，保持不变。补充引用分类见reserved-alias-reference-matrix.json，包含test/diagnostic，不能拿总行数当行为reader数。

renderer queue已有生命周期owner为`SimulationTickDriver._battleObjectPointFactory`（在Preparing时捕获，Shutdown阶段丢弃pending并最后解绑）；Q05应复用该已捕获owner建立World capture前置观察。`SimulationWorld.ResolveObjectPointFactoryForSimulation`对renderer路径会访问Instance，不能为snapshot校验直接调用它来创建服务。

旧Oscillate准确清单：LF2LivingObject.cs中的LF2EffectState.Oscillate/OscillateDirection及Reset，ProcessEffects的交替SetXY与TimeOut清位移分支；LF2Entity.TryRestoreBaseShellForSnapshot两项赋值；BattleEntityBaseShellSnapshot两项capture/属性。Q04移除晚帧读取/写入，保留Blink/Stuck/Super/TimeIn/TimeOut和延迟速度现有逻辑；Q05才删除载体和升级base shell。Native render_snapshot.cpp:1593按render_phase_008<0和交替phase产生±3 body shake，是B9另一条规则，不移植旧Oscillate来假替代它。NTSDSpec内无caller的静态表API不因同名自动删除。

+2F8的实际候选reader是`LF2Entity.ResolveFrameLogicTargetByHitFa`及`LF2WeaponFrameLogicResolver.ResolveWeaponHitFa12Target`目前使用的Spawner排除组路径。前者由hitFa1/3/通用hitFa/11调用，hitFa4使用预指定target。新字段不得进入character AiSensingSnapshot.OwnerSlot来混同owner；该snapshot现存OwnerSlot是另一合同。ECS identity/copy/hash及实际object-AI fast/fallback需要独立加入字段；若某snapshot根本不消费object AI，不添加无用重复镜像。

## CPoint float32 canonical规范（适配决定，尚未实现）

Native catch_point解码三个throw速度为float32，随后reader转换成double。Q05 Value与legacy DTO均保存float32，不在中途取整或以double独立真值绕开float32舍入。

规范化序列采用版本化的固定32bit单元：24个int字段存有符号int32，三个float字段存其IEEE754 binary32位模式；落盘采用明确little-endian。catalog先写int32条目数，再按source block顺序逐条写以下27单元：

`kind,x,y,injury,cover,vaction,aaction,jaction,daction,taction,faction,baction,uzaction,dzaction,throwvx,throwvy,hurtable,fronthurtact,backhurtact,decrease,dircontrol,throwinjury,throwvz,z,recover,drain,gain`。

throwvx/throwvy/throwvz对应槽位保存binary32 bits，其他槽位为int32。V2身份在外层解码合同中声明，不能把该数组当作旧19项数组；所有writer引用同一顺序，不按属性反射枚举。保留原始DAT文本与decoder有效值的区别。

有限负零保留符号位；非法文本、NaN或Infinity遵循native float_or_zero产生正零。原版strtof对有限的下溢/舍入结果应按bit验证，不因为errno假设而拒绝；超范围非有限值归零。值对象的Equals/GetHashCode、canonical writer和比较器应使用同一bit合同，避免+0/-0在Equals相等但hash不同。runtime算术仍使用数值，不将bit模式当伤害/速度整数。

Q05验证至少覆盖1.5/-2.25、-842150451→float32 -842150464、+0/-0、最小subnormal、舍入邻界、溢出/NaN/Infinity/尾随垃圾、重复key最后生效，以及copy/adapter/canonical输出完整保存。不得以JSON打印的小数相同代替bit一致。native数值语法现已有 `NTSD28-Q03-NUMERIC-DECODE-WITNESS-001` 37用例实际parser→FieldBag→decoder位模式证据，包含hex/指数、负下溢零及packed action；Q05复用这些固定输入输出，不重新猜语法，也不把ITR首整数规则用于普通int。

## Trace版本与字段可用性

现有Tools/NTSD28Parity/TraceContract.cs是battle-trace/descriptor/comparison/validation/self-test v2；EntityFieldContract定义49字段，+2F8尚不在其中。Unity Diagnostics/NTSD28UnityEntityRawCapture是raw tick v1、49字段/43 verified、6 missing；Test/Editor/NTSD28UnityRawCaptureEditor组装raw header。AuthorityTrace/authority_source_capture_main.cpp导出对应entity对象。

Q05同窗口增加`combat.objectAiExcludedGroupSourceSlot`，native读取object_ai_excluded_group_source_slot_2f8，Unity读取新独立runtime字段。battle trace及其contract/comparison/validation/selftest tag整体v2→v3；Unity raw header/tick及RawEntityCaptureComparator的对应tag v1→v2；authority source capture wrapper v1→v2。其他独立B0-domain/B2-input-RNG子schema在payload未变时保持。计数49→50，只有新字段经copy/reset/capture证明后才调整其binding；不得顺便把原6个MISSING无证据升为VERIFIED。

准确影响路径：Tools/NTSD28Parity/{TraceContract,EntityFieldContract,RawEntityCaptureComparator,AuthorityCaptureValidator,TraceContractSelfTest}.cs，Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp及其build manifest，Unity Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs与Test/Editor/NTSD28UnityRawCaptureEditor.cs。版本严格拒绝交叉拼接，source/content/decode/schema身份进入capture头与comparators；canonical CPoint bit比较由数值见证和内容projection处理，不在entity trace中重复塞入定义27项。旧49字段捕获保留历史身份，不回填+2F8=-1冒充新证据。最终脚本Record必须同时覆盖新增tag的所有literal caller/selftest；R15按同版本证据回访。

## Q05准确验收要求

新版本capture→修改world→restore→同seed/input重放要得到同版本checksum；新+2F8对claimed/raw slot、slot reuse和pool reset都检查。旧entity/aggregate/checksum/character-shell不混用；旧midbattle snapshot明确拒绝，不填默认适配。reserved注入不能继续影响生产结果，删除后的测试改为验证当前真实字段与行为，不能简单删掉整个旧回归。

队列边界需分别验证logic-only、renderer owner、非本World队列、正在tick、capture失败原状态保持、正常空队列成功。身份需要source/decoder/content/schema组合证据。上述要求服务当前单机战斗对齐，不授权联机/回滚架构扩建。Q03总出口还需把本合同的路径和剩余数值语法证据并入最终矩阵。

## Q05 实际live reader补充（NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001，不另开版本窗口）

帧完整读取新增FrameSounds有序声明、UsesLoganFrameNumbers、centerz/chp/cmp、nativeDvx/nativeDvy/nativeDvz与dx/dy/dz六double。源依据battle_world field_number_or_zero与render_snapshot见该Record。最终semantic identity/相关frame内容hash必须覆盖这些值与profile，不得只用旧37int或单sound投影认证新内容。既定entity13/aggregate21/checksum24/character2/base2窗口保持，不自动提升其他payload版本；实际schema/hash受影响路径在实施前另列准确Record。consumer仍由Q06/Q09/Q10各自接线和验收。
