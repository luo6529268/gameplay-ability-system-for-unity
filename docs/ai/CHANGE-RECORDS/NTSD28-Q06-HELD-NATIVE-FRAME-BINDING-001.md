<!-- CHANGE-RECORD
id: NTSD28-Q06-HELD-NATIVE-FRAME-BINDING-001
status: VERIFIED
change-kind: HELD_NATIVE_FRAME_SOURCE_WITNESS_FIRST
code-path: Tools/NTSD28AuthorityTrace/held_native_frame_binding_witness.cpp
code-path: Tools/NTSD28AuthorityTrace/validate_held_release_witness.py
code-path: Tools/NTSD28AuthorityTrace/validate_held_refill_witness.py
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06HeldNativeFrameBindingEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
authority: Current playable SimulationTickDriver28::step -> BattleWorld28::settle_held_refill_objects (C09/C20). Parent frame/native wpoint then weapon_action>=1000 terminal; else child raw action write before native descriptor validity; valid implicit0..998/declared999 then facing, delay and anchor/depth writes.
evidence: Read-only remaining-reader matrix confirms Unity HeldObjectWriter and LF2WeaponHeldStateResolver use oldHasFrame857 after DirectWriteHeldFramePreserveWaitCounter legacy getter. Source battle_world.cpp7967-8018 records different native admission/continuation semantics.
-->

# Held native frame binding

IN_PROGRESS / SOURCE_WITNESS_FIRST. Precise current write scope only new source diagnostic runner; no Unity production or tests authorized by this Record until exact source evidence and file list added. Parent remaining-live-reader audit stays open.

Build unmodified formal source closure in workspace, never write authority source/release EXE. Synthetic reciprocal holder/child (types0..6), initial distinct action and counters/latch/snapshots; enumerate implicit/declared low/high/999, negative and>=1000 terminal. Capture full raw states, runtime descriptor availability/wait/state, links, rng, held pass flags and complete following tick where feasible. Source errors for null descriptor are expected observations, not reason to discard rows. Keep action-write-before-failure and lifetime evidence. First isolate normal kind1/no throw; later add throw/drop/cover branches before declaring full held transaction. Do not silently widen scope to refill rules or source identity transforms.

Source-only acceptance: formal authority/build hash manifest verified; deterministic double-run, row count and target/lifetime observations; no claim of Unity match. After witness, create exact Unity fixture and production scope with RED proof and include both weapon and nonweapon paths, sameTick/followingTick/actual factories/replay/SelfCheck/Play/shutdown as affected. Existing terminal/refill/invalid relation contracts protected.

No new runtime ownership/schema or shutdown stage. Risk: descriptor change enables previously blocked downstream branches, requiring full observation not just raw action assert. Rollback: only owned diagnostic diff with required approval, preserve all existing work. No computer-use, Scene/resource/GAS/nonbattle/Server changes or commit/push/delete.

预审影响边界：DirectWriteHeldFramePreserveWaitCounter也由BattleDamageWriter.ApplyNativeType3TargetGenericContinuation调用。后续若改该共享setter，必须明确纳入type3响应保护/原生高帧见证，或在held专用调用处使用已存在的native writer并保持所有counter/latch语义；不能按名称假定仅持有系统使用。当前不改任何Unity脚本。

后继回归注意：NTSD28B6WpointMissingActionContinueProductionEditorTests当前用777/-888当missing；777在当前原生定义是有效implicit。源见证追加777，有证据后以独立准确测试修订处理该旧oracle，保留历史原失败与negative/invalid分支断言，不能通过复活旧HasFrame使旧测试变绿。

源文件已写，worker交回后root增加777声明/隐式对照，每type20行×7type=140。kind1/dvx0/cover0、holder0/child70、initialaction10与目标独立、counter7/latch11；输出DAT、精确before/after/following raw/B2/descriptor/link/RNG、两次完整tick held结果与存活。即将编译，不预判源或Unity通过。

首源build/double-run exit0、140行SHA077c915…f7af7b，但独立检查发现所有held updates/unsupported/terminal为0且诊断no reciprocal parent link：spawn child清理了预先写的parent linked-child。已归source-initial-link-fixture-fail，不当作有效行为证据。root仅将双向链接设置移到两个spawn完成之后，重新构建；权威源码未改。

修正reciprocal后源140双跑exit0/1467643bytes一致SHA8ddb447a80caa5edb8a2c55f1cb2ceda35b3fed3503f57d614fd5589655b9610；实测105valid/21unsupported/14terminal，1344独立立即raw/descriptor/input/RNG检查+420附加行检查PASS，following126存活且全部lifecycleSuccess。独立只读review预期105/21/14与实测一致，证据REPORT.md。不是Unity140通过，kind3/dvx/refill及完整following对照仍待。下一同Task准确新增Editor对照范围，然后RED再决定限定held调用点迁移；当前唯一code-path仍sourceCPP。

Unity fixture预声明：新增单Editor文件NTSD28Q06HeldNativeFrameBindingEditorTests.cs，读取源140完整DAT/before；实际world两profile×logic/renderer factories、完整双方生成后恢复关系，比较before/held after/full following raw、B2、descriptor、links与RNG/events。有效/unsupported/terminal行全部保留且记录具体差异，before不等时不能归因production。复用现有source parser/完整loader、source capture及有序shutdown工具，不修改其实现。此阶段仅测试；任何生产路径变更另行先登记准确符号。

Unity首次j ob b42a5699b8bb44458836e4224aba7d7d四FAIL，尚未完整矩阵：logic路径CompareJson对actual null JValue按对象索引抛异常；renderer路径Editor中新建Mono pool的Awake未初始化，_availableObjects null。原XML存fixture-initial-fail。root修比较器将对象/非对象差异作为叶差异累积，保留生灭差异；先EditMode仅执行两logic case，renderer两case转真实Play已初始化pool执行，不修改productionpool/Scene或伪造Awake。新增同测试文件请求probe用于真实Play全部四case，记录Scene checksum/borrowers/异常，单独按既有关闭probe收尾。

测试分配明确：EditMode公开TestCase仅两profile logic；同一完整方法的renderer=true保留并由真实Play probe执行四组，不在未初始化Mono池的EditMode声明不可运行用例。覆盖范围不缩减，首次四失败保留。

生产RED已确认：job92209404a51f4dca973f26b9c0ef71ec终态两FAIL，两profile140 before0，immediate728/following859，完整artifact logic-first-complete。准确生产预声明只两符号BattleHeldObjectWriter.RunStep12 generic初次WeaponAct绑定/准入 与 LF2WeaponHeldStateResolver.Act同一位置：调用既有DirectWriteNativeRawFramePreserveWaitCounter替代该位置旧heldsetter，HasNativeFrame替代该位置HasFrame。terminal>=1000仍在前，所有refill/投掷/drop/姿态/RNG顺序保持；不改共享DirectWriteHeldFrame、其type3命中caller或后续throw/drop setter。无新owner/schema/lifecycle组件。验证140立即/fulltick需归零，再补两factory Play/相关旧terminal/invalid/refill回归，旧777测试oracle须另记录。

根复验记录：初次binding修复后immediate0/following90，独立非holder-query关系修复后job9999f8df833c431abea6539af0dcd739源140两profile before0/immediate0/following0；query4PASS。联合95实际50PASS/45旧fixture/oracleFAIL，原XML与全JSON保存在after-query-fix，未标整批通过。旧fixture问题独立HELD-LEGACY-FIXTURE-NATIVE-CONTRACT-001处理；新正式factory未见nullTrans，生产sharedbinder保持。正式330静态域cover2/selected12/18为0的新只读报告FORMAL330-DORMANT-STATIC-DOMAIN.md已落盘，限定ITR2/OPoint2静态上界，不代替本140或动态身份域。

后继同文件replay预声明：全部140×两profile同World tick0持有前capture，执行HeldObjectProcessAll+两完整tick，restore后对raw/descriptor/link/B2/RNG状态重新执行比对，拒绝旧cursor。源unsupported与terminal全部保留；不验证跨Worldepoch、资源部署或全部held分支。


最终本轮状态：IN_PROGRESS / INITIAL_BINDING_VERIFIED_RELEASE_READERS_PENDING。源140两profile before/立即/完整tick均0差异；query含slotreuse5/5、旧fixture91/91；同World280场景560重放tick PASS；完整SelfCheck20:01:02Z PASS；真实Play560(140×两profile×两factory)全部PASS、Scene checksum保持/borrowers2→2；最终20:02:44Z关闭restore4→4/worldslots两pool0/两帧Stopped。Scene SHA BCD1047B…0E9FB6保持。三个独立子包只按声明查询或测试范围VERIFIED，初次读帧父包仍需后继投掷/drop/refill读帧/RNG相关证据，不标完整held/Q06。原失败及纠正均保留。

下一源扩展预声明（不改Unity生产）：同CPP保留默认140字节输出，增加--release模式，覆盖kind1 DVX±6与kind3 DVX0/±6、type0..6、左右朝向、上下输入四态、cover0/1、后续0..5/40动作声明或隐式。初次target20声明固定，使后继writer/RNG区别可定位；另追加少量初次implicit777高帧控制。输出完整参数、DAT与既有before/立即/following/raw/B2/links/RNG；必须保持初次caller所见输入mask。新数据单独release-source，不覆盖默认source140或重跑其Unity已过矩阵。下游refill另外准确向量后继，不把release源通过当Unity或完整held通过。

Release源1150已build/double-run exit0且原140字节不变。新增独立验证脚本预声明：validate_held_release_witness.py依据settle_held_refill_objects与native_random.cpp重新计算几何/释放动作/关系/2F8/全raw和原生随机表、调用序列/最终RNG；只验证立即事务，following保留后继Unity对照。release-source独立，不覆盖初次140。

独立release模型首跑296552检查只有1712个depth mask前置断言失败，其余完整after/RNG均匹配。核对authority capture contract_input_mask明确up/down位2/3（4/8），不是错误模型中的32/64；仅纠正独立validator输入位码，源/Unity未改，原validation-initial-mask-model-fail.json保留。

Release1150源独立296552检查PASS，SHA8a6c0cc2f8b20d7deeb360e70350fffce4f69a63f21ebfd58702793f605f44c3/13208826bytes，原140逐字节保持。下一修改已有单Editor fixture为复用矩阵函数，新增两profile release-source1150测试入口；原140入口/结果名/Replay/Play接口保持，release结果单独前缀。先完整before/立即/following RED，不提前改release生产。

Release生产RED jobc1d3ba0b88054b3daceea691db9b5e6c：两profile1150 before0/immediate1384/following1002，完整文件release-production-red。准确扩展生产符号预声明：BattleHeldObjectWriter.RunStep12两释放action写者/重物RNG、ThrowHeldObject深度和2F8、DropRandomly最终action；LF2WeaponHeldStateResolver.Act两释放action/RNG及ThrowHeldWeapon深度/2F8。目标：post-release0..5/40原生descriptor；type2不论kind始终native0x41865E；type1/4/6写ObjectAiExcludedGroupSourceSlot2F8而不是独立SpawnerSlotIndex；上下单边即使DVZ0也写0，其它方向保持原Vz。所有抽样次序/关系/HP/owner/position不改；损伤drop/refill/SyncHeldPose其它读者留后继。正常factory1150覆盖实际路径，generic武器DAT壳体分支需另增同源对照，不将未测分支算通过。

Release修复实测：job b573b82a834a415c9925f12441a11b0f terminal SUCCEEDED，正常factory两profile各1150 before/立即/following均0差异，证据release-after-fix。父任务仍IN_PROGRESS。下一测试预声明：只扩已有NTSD28Q06HeldNativeFrameBindingEditorTests.cs，增加release-source中type1/2/4/6共664行的两profile通用LF2OtherObject实体测试，使用相同catalog/DAT/slot及完整before恢复，断言实际CLR类型；比较立即及完整下一tick。通用实体直接注册，已有幂等Shutdown回收；不增加生产接口、资源或Scene修改。输出release-generic独立文件，正常1150和初次140保持。验收before与两阶段差异均0；失败保留，不跳过用例。回滚仅人工撤销本测试扩展，保留证据。

通用壳补验终态：job f60db972d4724aacb78ecd0a5c51aa1d SUCCEEDED 2/2，两个profile各664行before/立即/following均0差异，release-generic-pass归档。只覆盖release-source1150中type1/2/4/6子集，属于受控CLR适配测试而非正式factory自然生成组合；完整following/Shutdown已执行，不含renderer。只读review无确定错误；报告scope文字应明确子集。编译CS0、Validate-ChangeLedger exit0、git diff --check exit0。父任务仍IN_PROGRESS，等待release replay/Play/新SelfCheck与其它held读帧。

最新release相关回归：query5/5及旧fixture91/91 PASS。新完整SelfCheck20:35:34Z FAIL R5-HOLD-002 Spawner旧断言；已保留原失败，独立oracle审计待准确Task/Change。Scene SHA保持。详见RELEASE-PROGRESS-REPORT.md，父状态仍IN_PROGRESS，release replay/Play与refill后继未验。

Release replay/Play后继预声明：只扩已有NTSD28Q06HeldNativeFrameBindingEditorTests.cs。复用同World snapshot逻辑新增release-source1150两profile回放，比较恢复前完整capture及held后两tick并拒绝旧cursor；保持旧140入口。Play probe增run-release请求模式，1150两profile两factory共4600例，单独输出release结果；Scene暂停checksum、pool借用数量保持，随后现有Q05关闭重入probe。通用664为明确逻辑适配子集，不当正式factory。修正generic矩阵scope字符串描述为筛选子集。禁止编辑生产/资源/Scene，失败不跳过；脚本仅在SelfCheck终态后改。

Release replay实际PASS：job cc26547f32ed4f81a5d4629e501178f6 SUCCEEDED 2/2，两个profile各1150场景/2300重放tick，共2300/4600，同World完整capture一致且旧cursor失效；release-replay-pass已归档。两次bridge查询30s观察超时，继续同job最终通过，没有重启。release Play run-release现已通过现有Editor进入Play，等待实际4600终态；场景仍旧138内容，矩阵为合成authority DAT，不是正式部署或按键/图像验收。

最终release声明范围已验：Play4600四矩阵各1150 before/立即/following0，borrowers2→2/Scenechecksum保持；20:45:41Z独立关闭restore4→4、world/slots/两pool0/两帧Stopped。完整SelfCheck20:39:17Z PASS、release replay2300/4600 PASS，CS0/Ledger590/diff PASS。Scene dirtyfalse/root14/hashBCD1047B…0E9FB6保持。Parent IN_PROGRESS / INITIAL_AND_RELEASE_SCOPES_VERIFIED_REFILL_PENDING。下一补给源向量先行，remaining reader/display/post-display不关闭。

Refill源见证扩展预声明：唯一脚本写域Tools/NTSD28AuthorityTrace/held_native_frame_binding_witness.cpp，保留默认140和--release1150逐字节输出，新增--refill模式。复用完整before/after/following/raw/B2/RNG/held结果，参数显示真实childOid与DAT type；OID122/123正例、普通OID78控制，holder state17/非17，childHP负/0/1/2/5/6/30边界，holder HP/PP封顶边界，2F4=-1/0，action0已声明/隐式，保留非0Zz和各轴速度/latch/counter/snapshot。必须确认正式system_rules默认ID122/123及buildclosure，源system_rules不定制以掩盖差异。后继Unity测试/修复在源双跑和独立检查后另准确声明。暂不修改Unity生产、不建资源、不改变已验两个模式。

根集成源矩阵补12个OID78普通对象控制，共242行；原230轴保持。独立验证器新增准确code-path: Tools/NTSD28AuthorityTrace/validate_held_refill_witness.py，按source耗尽/非耗尽完整立即事务重算HP/MP/2F4/统计/raw/descriptor/RNG，following只保留实测不伪称独立模拟。对应源单双跑、原140/1150字节一致检查均待。

Refill源实测build exit0、242双跑一致SHA83af13d…42d5dc0；原140/1150字节保持。单Editor fixture扩展预声明：新增refill-source242两profile矩阵入口及refill前缀，CreateWorld按params.childOid(旧缺省78)绑定catalog/factory；Definition仅refill模拟正式loader type_sub==0补id，旧key明确区分；Capture可增加独立2F4/HPMP消费总量投影，仅expected含该字段时纳入对照，Restore同字段。原140/release入口保持，不改production。先建立完整before0；独立源模型通过后运行Unity RED并保留。

Refill源242双跑/独立229293检查PASS，original140/release1150字节保持；新fixture已CODE_WRITTEN/COMPILE_PASS，job fd1f7330323c4a5e9e1f3893c13c372d终态FAILED，两profilebefore0/immediate660/following992，原XML/JSON归档refill-production-red。尚未改refill生产；下一按完整调用图声明共享补给事务最小符号，修同步RNG/原生action0及按OID的通用类型分支。Zz无源等价证据，不据静态猜测改。Ledger590/30 PASS，CS0。

Refill生产准确预声明（242 RED后）：仅LF2WeaponHeldStateResolver.ProcessDrinkConsumption改为instance转发，并新增static ProcessNativeRefillConsumption(holder,held,ref result)；BattleHeldObjectWriter.RunStep12非weapon分支在WPoint/terminal之前调用同核心，exhausted提前return避免重复消费。按正式ObjectId122/123与holder native state17，直接Runtime算术保持消费统计；耗尽先复用weapon release helper或generic等价cache/link零清理，再child nativeaction0/counter0/Vy0/native同步RNG(callsite4181C9/4182C0,7)-3，parent nativeaction0/counter0/childWeaponFlightCounter0，保留weapon hook一次。world随机有效为前置，无legacy fallback；不改Vz/位置/朝向/快照，PS.zz旧weapon适配保持，不推断native同字段。现有protected wrapper/API、对象组织、pass/lifecycle/schema不改。必要旧伪OIDfixture必须独立Record，不能回加type_sub生产fallback。验收242两profile before/立即/following0后补generic适配、相关回归、自检、replay/Play/关闭；失败保留。

Refill生产已写：两个声明文件完成共享事务，独立review无确定重复消费/顺序/字段问题。首轮compile CS1503在conditional随机callsite int→uint；仅补两个u后缀，刷新编译中。原错误据实记录，不声称首编成功。

242修复后两profile before/立即/following0，job cbfe7a03f6464c85aba510d6895f01cf 2PASS。旧100回归先94PASS6旧RNG oracleFAIL，独立oracle修订后ca6cfd93df7e42648921705d9a64f3c3 100PASS。后继同Editor精确扩展：genericShell选type1/6共202两profile、refill-source242两profileRunReplay、Play run-refill242四矩阵968；全部沿用既有capture/原生RNG/清理接口，结果refill前缀独立，保持原140/1150入口。

Refill限定最终验收：242普通两profile/202generic两profile before即时following0，100回归PASS与11联合PASS，484场景968重放tick、SelfCheck21:07:43Z、真实Play968、关闭21:09:16Z PASS。Scene checksum/borrowers2→2、restore4→4/worldslots两pool0/两帧Stopped，场景dirtyfalse/root14/hash保持。生产二路径独立review通过，CS0/Ledger592/31/diff0。父状态IN_PROGRESS / INITIAL_RELEASE_REFILL_SCOPES_VERIFIED_REMAINING_CALLERS_PENDING；下一只读核对无caller SyncHeldPose与damaged守卫，不重做已验包。

VERIFIED / DECLARED_INITIAL_RELEASE_REFILL_AND_STATIC_CONTENT_SCOPE。最终caller审计：SyncHeldPose全Assets/Packages仅定义无直接caller；formal330同2033关系图selectedchild10/12/18均0，两artifacthash本轮核对匹配，详见FORMAL330-STATE10-DOMAIN。当前source无child12/18unsupported，旧表述已纠正。仅关闭三已验scope及静态内容边界；identity变换保留关系/其它producer域强制由remaining-reader身份变换任务回访，不声称任意runtime或所有held分支完整一致。未删除/重写dormant或无caller兼容代码。下一CPOINT-INPUT-ACTION-SELECTION-001与raw caller矩阵；Q06未完。
