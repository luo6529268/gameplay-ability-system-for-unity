<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-PHYSICS-MISSING-FRAME-GUARD-001
status: VERIFIED
change-kind: NATIVE_PHYSICS_FRAME_ADMISSION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06MissingPhysicsFrameEditorTests.cs
authority: BattleWorld28.step_physics:1798 checks pending first, then advances nonzero hold, then negative link early return, then rejects unavailable definition.frame before integration.
evidence: Pickup1200 original following-physics vectors; post-binding pickup fields all match but 1250 position/velocity differences per route remain for unavailable holder action999/1000/-998.
-->

# Native physics缺失帧资格

准确三脚本。在LF2Entity增加查询predicate：hold==0且LinkState>=0且Native当前动作点查null。ExecuteNativePhysicsForWorldPass和World exact Ecs入口共同拒绝这部分物理计算并清现有completed marker；pending仍先挡，hold非零和负Link保留原处理，独立dead resource normalization不被跳过。使用native accessor，998 implicit及声明999有效，不按旧857或单纯action>=999判断；不全局重绑Frame.D或修改未审计的writer/legacy独立SimTU入口。

先测试RED：两后端×7type×3缺失动作(-998/缺999/1000)×正负零hold×pending×负/零link；位置/速度/hold顺序必须按源保留，type0有效998/声明999反例必须继续物理。再修predicate/两caller、重跑pickup1200后继physics与旧pending/full-frame联验、SelfCheck/对应Play和关闭。无新persistent/schema/manager或关闭阶段，回滚只本差量且按规则获批；禁止computer-use/非战斗/框架/资源/Server修改。父pickup保持IN_PROGRESS直到后继物理联验完成。

RED eba18c5023404dfdabba81988766b57d：6中4PASS/2FAIL，valid998/decl999对照通过；缺帧时type0及1/2/4/6在hold0/link0/pendingfalse仍移动或改速度，两backend复现。现已写predicate和Execute/World两个gate；只在hold0且link非负时查询缺帧，原其它顺序保留。

5696e73ec8fa4ac8abb40f397e0025d3 实际16/16 PASS：pickup四组4800/after physics0差异，missing6和pending6通过。旧pickup两个selector漏Editor命名空间所以未执行，不能记成通过；下一正确补跑NTSD.Test.Editor旧三类及full-frame20。当前仍待Play/SelfCheck。

## 最终限定出口（2026-09-14 05:12Z）

VERIFIED / PICKUP_NATIVE_READER_RAW_BIND_AND_MISSING_PHYSICS_ADMISSION_SCOPES。原pickup1200全部applied、两遍一致SHA cee703d23555b809bf14c60e130e81f8432dce6e78d67a37aafedbe11d87cdfd，随机0；源顺序保持类型关系/101与-1、dead6清weaponHp、counter归0、非零wpoint raw覆盖。目标current在冻结frame0候选后改变，原函数读live current；同初值逐字段验证，不用碰撞快照替代。

实际修改：actual/HitPlan目标点查改Native；holder新增窄internal wrapper复用BindNativeC25Action(action,false)，保持latch、原始负值/1000及nullable描述符，pure plan和共享legacy API不改。原1200配对：原两组RED各3232条；绑定修复后拾取端0差异，剩余各1250全属后继physics。独立缺帧guard在pending之后、hold0且link>=0时才判断Native当前帧不可用，普通/World exact两入口一致，保留hold/负link与独立dead normalize。998 implicit和声明999仍有效；未全局重绑未审计writer。

eba18c5023404dfdabba81988766b57d缺帧RED6中4PASS/2FAIL留存；5696e73ec8fa4ac8abb40f397e0025d3最终新16/16 PASS：pickup4组4800 before/after/holder后继物理raw47+关系/descriptor0差异，missing6（504分支+4valid对照）和pending6通过。该轮旧两个selector漏Editor命名空间，未执行，已明确补跑。eb2026e0a0eb406a98ec1a5f5a3eeda1正确215/215 PASS：旧atomic140、pure32、rules23及完整frame20组/5400tick；XML实际case已核对。

完整SelfCheck请求05:09:26.7476890Z，05:10:10Z PASS，未修改SelfCheck或旧pickup测试。真实Play-800-pass.json两actual factory×direct/actualShadow×200代表输入共800，raw47/关系/绑定和紧接holder physics通过，Scene checksum保持，Renderer2→2。Shutdown-pass.json恢复4→4、World/slots/两pool全0、两帧Stopped；已正常退出。Scene dirtyfalse/root14/SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6、CS0。Ledger/diff-check交付前再验。

原runner首次漏DepthIntent参数编译失败、一次Python验证误把101/-1写成101/-101均有记录并纠正，未改正式source输出；最终原EXE/75-source hash不变。本两包共原pickup五脚本+缺帧三脚本，LF2Entity重叠，实际两新增测试、一个CPP、四生产文件。无schema/persistent/服务/关闭顺序变更，无Scene/资源/非战斗/Unity-GAS/Server/Gen/Plugins改动，无computer-use或提交。

本出口不包括完整后继held/WPoint/CPoint/throw/图片；source物理见证是拾取后holder原函数端点，真实Play同入口，未声称完整拾取后的整个战斗tick已经新增验收。剩余collision frame/CPoint/direct/raw/held等reader、display/post、Q07继续；旧World epoch formal recovery缺口与raw3MISSING保持。
