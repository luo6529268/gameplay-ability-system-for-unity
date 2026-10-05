# NTSD28-336B44-Q07-HITFA14-COMMON-TAIL-001

状态：RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS；原Editor七例RED五预期首差/两控制通过，修后七例＋SelfCheck局部路由8/8及正式207/115完整Driver一步1/1均通过，最新生成0错/已原Editor导入，原Scene clean。两次完整方法在进入callback前误用OID别名作type的失败保留并已纠正；非自然Sai技能或正式根同初态证书。必要ONE完成按REUSE，仅实际首差/相关改动回访，父Q及总目标开放。准确终态与外部build脚本变化限制见[Record](../CHANGE-RECORDS/NTSD28-336B44-Q07-HITFA14-COMMON-TAIL-001.md)和[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-HITFA14-COMMON-TAIL-20261005/REPORT.md)。下方测试先行是事前合同，不新增角色专属判断。

## 权威和完整路径

当前正式NTSD2.8-Logan.exe SHA336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3。native_ai.cpp145/543/547～558/562～591：behavior14共用目标/HP门及X/Z追踪、14专用Vz上限1.5与朝向保留；不衰减Vy、不追踪/钳制精确Y、不写整数Y、不进行action±50选择。behavior2才有源帧选择。simulation_tick_driver.cpp416调用，playable build.ps1:68纳入；后续577 FrameMotion/600physics独立，不能把AI尾部保持误称整个tick高度静止。

正式data.txt4/type0 Sai与207/type3 ink，Sai334 OPoint action115直接生产；207/115与116～118均hit_Fa14。当前源和镜像ink DAT SHA同BFD40B2B4BB3FC3CD173113535321FFC128F8C642AD584D498FB56D0EC5713D9。旧HP门Record已明确旧SelfCheck14Y期望非权威；最近X/Z包未处理该尾部，不重新执行其18项。

## 准确修改范围

- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs：仅RunHitFa2Or4Or12Or14FrameLogic中hitFa14多余纵向写者/旧帧段尾部；以hitFa类型作共同门，不判断ObjectId/角色名。2/4/12追踪原写序、回收早返、HP门、source X/Z统一入口、clamp/facing均保留。
- Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs：仅CheckCurrentDatHitFa14Routing既有三CLR壳代表期望，Y/Vy/YInt保持且action0维持；3/4代表路由不改。不把人工frame0合成夹具当自然正式DAT证书。
- Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs：仅新增IndexedHitFa14CommonTailPreservesVerticalStateAndAction七参数例；如必要再新增已预声明IndexedHitFa14CommonTailSurvivesOneFullDriverTick一个方法，复用旧wrapper，不新增scenario/trace/request/Scene对象。

## 测试先行和出口

七参数：正式207/115恒等与fixed视野Y-100.25/Vy2.8；fixed Y-40.25/Vy-2.8与正Y3.75/Vy-2.8；fixed207/115/Vx9快分支。X同400、sourceZ600/604、活跃异队target99/0、正HP、slot50、双方source/view完整。在唯一FrameLogic之前存YInt，14之后Y/Vy/YInt/action全保持，X/Z源及目标/HP/count不变；快分支必须暴露旧115→65误写。另正式518/1/hitFa2与907/190/hitFa12在Y-100/Vy2.8仍Y-99/Vy2，确保保留分支不被误停。

RED后只修共用尾部，并更正旧SelfCheck同一行为。GREEN仅上述七例＋ExistingLiveHitFaRepresentativeRoutingRemainsValid一项；不跑全SelfCheck/全EditMode/角色矩阵。生成Editor构建0error、现有MCP refresh后核新程序集。必要完整Driver一个方法：同旧wrapper schema/seed/诊断Stage23/部分target初态，当前LoganRuntime+项目mode Asset；旧875注销/确认槽清，再注册正式207/115到slot1并更新Roster新身份。sourceX400/Z600/Y-100.25/Vy2.8/Vx9，targetsourceX400/Z604/Y0；源帧无Y/Z动作值/OPoint，完整一次StepOneTick检查源X409/Z600、精确Y-97.45/Vy2.8、view比例、无action65等旧尾部残留及count/target；实际native帧推进结果需依正式driver/native counter确认，不改DAT或延长wait来让断言通过。继有scope正常对象/槽/logic pool三项0与发布恢复；不称原Scene自然Play或根同初态。

## 风险、不变项及回滚

2026-10-05 第二次失败后的字段语义更正（再次修改新测试方法前登记）：job4ad8b188865b4d86aed407c55737e4c6 completed1在同一最前断言得到207；与首次raw得到0均未进入World callback。前述“type_sub缺目录type元数据”解释不准确，保留为历史诊断并由本条取代。CharacterAnimtorManager:986/1020～1021明确type_sub是OID别名，catalog加载写entry.Id；真正对象type由LF2Entity.ResolveCurrentDataObjectType读取已注册World RuntimeDataCatalog中的wrapper OID definition.type。下一只把本方法的guard改为catalog entry.Type==3、type_sub==207，并在注册后直接断言共同类型解析器==3；所有tick断言不变，不改生产或DAT。独立只读审阅据此确认旧raw518完整tick仍实际走type3，无须撤销其限定证书。8项GREEN继续复用，只重跑当前完整方法。

完整Driver测试入口更正（运行首次失败后、修脚本前登记）：job2e6e9dc23ce542cea799633b9c6aabee completed1在最前type_sub3断言得到0，尚未进入World callback。直接BuildCharacterDataFromSource只解读DAT，不带data.txt目录type元数据；改用既有LoganObjectCatalog→BuildCharacterFrameConfigsFromCatalog→configs[207]，保留type3及帧Motion/counter断言，不修改DAT、规则或wrapper。八项GREEN仍复用，仅复验本完整方法；旧直接raw518方法的类型边界另做只读审查，不自动称非角色消费者已执行。

实际规则影响仅14的多余高度/速度/帧状态写入；它可以改变后继命中/生命周期结果，这是修复正式差异的预期副作用，须用上述正例和相邻控制验证。DAT、图片、1.5倍显示、相机/背景/地图、项目模式、Scene/Input Actions、非战斗/GAS/Gen/Plugins、33ms/F5、输入/HP/pass/关闭序列保持；没有新增module/queue/pool/cache/服务，关闭阶段无需增项。三脚本操作前逐SHA字节备份保留现有dirty；失败保留原件并精确诊断，不以旧HEAD恢复，恢复须另获准确授权与新Operation。只在证据取得后推进Record、Ledger/STATE/handoff/总表。

