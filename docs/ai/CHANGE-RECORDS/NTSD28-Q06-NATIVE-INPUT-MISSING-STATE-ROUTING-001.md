<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-INPUT-MISSING-STATE-ROUTING-001
status: VERIFIED
change-kind: NATIVE_INPUT_OPTIONAL_STATE_READER
code-path: Tools/NTSD28AuthorityTrace/native_input_missing_state_witness.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeInputMissingStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0AirDashRedirectBuiltinsEditorTests.cs
authority: Current playable InputRouter28::step_sampled -> route_native_ground_builtins reads optional state with fallback -1 at input_routing.cpp907/987; FieldBag::integer uses strict full-token Int32 and returns nullopt for missing/invalid. The raw/physics state default remains zero.
evidence: Parent CPOINT-THROW-NATIVE-RAW-BINDING-001 source392/Unity immediate pass; correct nonAI native-profile full tick leaves 28 implicit99+attack cases, source99 versus Unity65. Existing world/scene work preserved.
-->

# Native input optional state

IN_PROGRESS / SOURCE_WITNESS_FIRST。Task同ID。独立源runner使用未修改的当前playable源码，输出输入路由前后raw、run accumulator、输入与随机；覆盖未声明frame、已声明无state、声明0/其它state、无效state token、高帧和无效action。源端double-run/build manifest固定。不是正式EXE完整观察。

原Unity RouteNativeGroundBuiltins及RouteNativeAirDashRedirectBuiltins（三处state读取）直接消费typed state0，无法区分缺失/无效state。预计新增局部native input state resolver，复用严格LoganNumericDecoder.TryParseInt32和rawProperties；保留旧typed夹具/旧内容入口，不修改LF2FrameData全局state、parser、FrameCache或其它pass。

副作用合同：source在任意非null当前frame时先将非零run accumulator向零递减，再进行state/action分支；不能为了缺失state早退而漏递减，也不能Ground→AirDash双减。准确读取当前definition/current action及action215后state重读；action110/215/182/188的编号分支不因state缺失被跳过。若发现需额外生产路径，修改前补Record；不借此重写全部输入或改GAS/Mono/生命周期。

验收：source矩阵与Unity严格before/after对照、明确输入和RNG；旧ground/airdash及type0输入相关focused，父CPoint392完整tick回访；编译CS0、SelfCheck以及父任务要求的Play/replay/关闭。所有声称限于实际证据；Q06/Q07和总目标不自动关闭。

原始RED保留父artifact following-real-input-red；后继源/测试放本ID artifact。未知分支保持待确认，不降低期望。无新增持久schema/owner/queue，十一阶段关闭顺序不变。Scene/HUDBg30/资源/非战斗/Server保持；回滚仅本差量且需依规则批准，不提交推送，不使用computer-use。

源432已写/编译/双跑同SHA7178ec5e534bc710575838e2a610d477c23b46228c30b9cc5dbdc22965d20898（各1522298 bytes），源定向3552检查PASS，边界见source/validation-summary.json；不是全输入独立oracle。已写单Editor测试：source DAT原文+准确非AI native profile，调用实际InputTwoPass.ProcessNativeSampledState，47bound raw/B2全输入/原生RNG和descriptor+runacc，复用既有B2投影。生产尚未改，等待编译和RED。

首次Unity编译缺NTSD.EditorTools using（两CS0246）；测试job535ff39fabbf48c99f8df26897d8abe3虽工具标success但实际0test，明确不是通过证据。补准确namespace后重新刷新，等待真实编译和432 RED。

Unity job3c5a4eda8c764ec8b6781962e28af954两FAIL：每profile432 before0/after1193，完整原输出production-red。case32/152（声明state0+attack）唯一差异是新动作65的descriptor availability，源可用而Unity Frame.D null；准确新增LF2Entity.WriteNativeInputActionUnchecked路径：旧HasFrame/GetFrameDataById必须改Native getter，仍保留raw写号、latch、无效目标的既有cache处理，不引入Transition。其余计划BCAW三个state读取严格optional helper与AirDash fallback早退前计数递减。解析复用LoganNumericDecoder严格整数，原Source FieldBag::integer完全匹配缺失/非法/+0/溢出边界。

已写两生产文件：BCAW局部ReadNativeInputFrameState覆盖Ground/AirDash及215后重读，Logan用rawProperties+严格decoder缺省-1，legacy typed路径保持；AirDash在非null frame的!owned早退前衰减剩余计数，Ground已处理则不会调用fallback；LF2Entity原生unchecked输入writer直接Native getter。未改全局frame.state、解析器、FrameCache或非战斗。待编译/432复测与原父392fulltick回访。

job5cec09d0c7c540d0be3c74c154ba7509联合28项27PASS/1FAIL：432两profile before0/after0，原CPoint392立即及完整下一tick四项全部PASS，全部Ground及其余AirDash通过。唯一旧AirCore_IsNotTypeGated_OwnsOnlyExactDomain_AndAllocatesZeroWarmBytes期望state3 unhandled计数17保持，实际16。source任何非null frame均先向零递减，state12控制组已验证非处理state也衰减；改动前补准确此Editor文件/单方法oracle：仍断言返回false不执行air action，但计数17→16表明共用前置；保留ObjType3、4096循环/零分配全部断言。不是为旧期望回退原生递减语义。

旧AirDash前置断言修订后jobefd5176c830f45e28536f776a1101d36本类10/10 PASS；原联合其余27PASS保留。当前432四?准确两profile输入+父392四项全部通过，不能称四组432。下一新SelfCheck、父Play/replay/关闭，生产不再扩大。

复核边界：源agent已独立核对helper意图、严格decoder和非null公共前置/递归语义；最后再次请求独立diff review遭agent thread limit，未获得新的独立最终审查，root已读实际diff。不得把之前候选审阅说成新的完整diff审查。源432不覆盖方向/Jump交叉前置或double-tap，详见REPORT。Parent验收单文件worker正在准备，不运行Unity直到写权归还。

Fresh完整SelfCheck18:13:58Z PASS，结果本ID归档；父before-throw replay112场景/224重放tick已PASS。现在进入真实Play验收，原source/立即/fulltick无需重跑独立Editor矩阵。

VERIFIED / DECLARED_OPTIONAL_INPUT_STATE_AND_DESCRIPTOR。fresh SelfCheck18:13:58Z PASS；Play18:15:16Z投掷完整tick1568+输入864全部PASS，Renderer2→2、Scene checksum不变；shutdown18:16:07Z PASS，恢复4→4，World/slots/logic/render全0、两帧Stopped。父before-throw replay112场景224重放tick通过。最终Editor idle/notPlaying，Scene dirtyfalse/root14/SHA BCD1047B…0E9FB6。证据各ID artifact。没有正式资源迁移、物理键、整场视听或全部B2/B6/Q06声明。下一唯一Task NATIVE-INPUT-ACTION-COST-FRAME-READERS-001 READY_LIVE_SOURCE_MAPPING，尚未改其脚本。
