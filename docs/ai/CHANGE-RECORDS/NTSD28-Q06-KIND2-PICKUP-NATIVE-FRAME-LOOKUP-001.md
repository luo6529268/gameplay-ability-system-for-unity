<!-- CHANGE-RECORD
id: NTSD28-Q06-KIND2-PICKUP-NATIVE-FRAME-LOOKUP-001
status: VERIFIED
change-kind: PICKUP_NATIVE_CURRENT_FRAME_READ_AND_RAW_HOLDER_BIND
code-path: Tools/NTSD28AuthorityTrace/kind2_frame_lookup_witness.cpp
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06Kind2FrameLookupEditorTests.cs
authority: BattleWorld28.resolve_special_relation_hit kind2 at5310-5408 links types1/2/4/6, resets counter, reads live target definition.frame, copies nonzero weapon_action raw; no destination admission or normalization.
evidence: Actual/HitPlan target getter remains legacy; holder SetHolderAction uses shared legacy raw binder, leaving implicit115/116/high actions without native metadata.
-->

# kind2目标帧读取与持有者绑定

准确五脚本，source-first+test-first。原kind2即使目标帧不存在也先完成关系/计数，仅省略wpoint覆盖；不能添加整体目标帧拒绝。锁定表120/124，type6 HP<=0清weapon_hp，非持有type0/3/5仍清counter并执行覆盖，已有纯plan保持不改。wpoint非零可负或>=1000，直接写原值，不做abs/999规范化；原latch保持，counter由plan归零。

原1200向量：7types加type1两个特殊OID和dead type6共10初值×target action0/99/857/998/999/1000×有/无声明×weaponact0/998/999/1000/-998×holder descriptor有/无。先snapshot/收集初始frame0几何候选，再模拟前一个writer改变目标current action，明确collision snapshot仍0；原kind2读live current。记录出生/关系后以及仅holder后续frame-motion/physics后的raw47+关系/latch/counter及可用descriptor，证明getter与绑定必须一起闭合；source诊断失败不作为规则。

actual/HitPlan只迁移kind2目标getter；holder通过新增窄internal DirectWriteNativeRawFramePreserveWaitCounter复用已验证BindNativeC25Action(action,false)，它实际仅写current/native Frame.D和保持latch的Trans元数据，不修改counter/声音/lifecycle；共享legacy API和其它caller保持。新入口不是通用framework重构。真实必要code符号、测试输出和旧行为差异如实登记。

验证原重复输出、Unity direct/actual+Shadow两profile、原kind2 pure/atomic相关、SelfCheck、真实Scene双factory拾取/原始动作/下一物理读取与有序关闭。无持久字段/schema/新模块/队列；拒绝条件与已批准例外不变。回滚仅本差量且按规则获批，不动非战斗/Scene/资源/Gen/Plugins/外部Server，禁止computer-use。完成后返回reader umbrella的CPoint/throw/direct其它调用。

原1200全部applied，action/latch/counter/relations（特别101/-1）/count/owner及dead6清weaponHp验证通过，随机0，重复SHA cee703d23555b809bf14c60e130e81f8432dce6e78d67a37aafedbe11d87cdfd。首次runner漏DepthIntent参数编译失败已据原签名补none，错误log保留；正式source未改。一次Python断言误把101配对写成-101，已按既有原合同修为-1，没有改源输出。Unity测试已写，先冻结初始0候选再改变目标live current；初始未激活child/parent索引0显式同值，不归一化。生产未改，Authority400 direct/Shadow RED进行中。

RED 8e1f2518803c42fb8fff4bce3806c7ca两组均FAIL，各3232条；before无首差，出现丢失holder descriptor、高帧wpoint未覆盖、下一物理差异。先落实已声明三生产路径，再分类剩余物理资格差异，不能抹去无效action的后继错误。

5696e73ec8fa4ac8abb40f397e0025d3 实际16/16 PASS：pickup四组4800/after physics0差异，missing6和pending6通过。旧pickup两个selector漏Editor命名空间所以未执行，不能记成通过；下一正确补跑NTSD.Test.Editor旧三类及full-frame20。当前仍待Play/SelfCheck。

同测试文件新增真实Scene请求probe：200代表行×两actual factory×direct/actualShadow=800；每行先冻结frame0候选再改目标live current，比较拾取字段及holder紧接物理。覆盖全部七types、两个特殊OID/dead6、998/999/1000/-998、两边描述符存在性。关闭按既有fixture实体归还再World stop，Scene checksum和Renderer baseline校验；不代表完整后继held/WPoint阶段或图片。

## 最终限定出口（2026-09-14 05:12Z）

VERIFIED / PICKUP_NATIVE_READER_RAW_BIND_AND_MISSING_PHYSICS_ADMISSION_SCOPES。原pickup1200全部applied、两遍一致SHA cee703d23555b809bf14c60e130e81f8432dce6e78d67a37aafedbe11d87cdfd，随机0；源顺序保持类型关系/101与-1、dead6清weaponHp、counter归0、非零wpoint raw覆盖。目标current在冻结frame0候选后改变，原函数读live current；同初值逐字段验证，不用碰撞快照替代。

实际修改：actual/HitPlan目标点查改Native；holder新增窄internal wrapper复用BindNativeC25Action(action,false)，保持latch、原始负值/1000及nullable描述符，pure plan和共享legacy API不改。原1200配对：原两组RED各3232条；绑定修复后拾取端0差异，剩余各1250全属后继physics。独立缺帧guard在pending之后、hold0且link>=0时才判断Native当前帧不可用，普通/World exact两入口一致，保留hold/负link与独立dead normalize。998 implicit和声明999仍有效；未全局重绑未审计writer。

eba18c5023404dfdabba81988766b57d缺帧RED6中4PASS/2FAIL留存；5696e73ec8fa4ac8abb40f397e0025d3最终新16/16 PASS：pickup4组4800 before/after/holder后继物理raw47+关系/descriptor0差异，missing6（504分支+4valid对照）和pending6通过。该轮旧两个selector漏Editor命名空间，未执行，已明确补跑。eb2026e0a0eb406a98ec1a5f5a3eeda1正确215/215 PASS：旧atomic140、pure32、rules23及完整frame20组/5400tick；XML实际case已核对。

完整SelfCheck请求05:09:26.7476890Z，05:10:10Z PASS，未修改SelfCheck或旧pickup测试。真实Play-800-pass.json两actual factory×direct/actualShadow×200代表输入共800，raw47/关系/绑定和紧接holder physics通过，Scene checksum保持，Renderer2→2。Shutdown-pass.json恢复4→4、World/slots/两pool全0、两帧Stopped；已正常退出。Scene dirtyfalse/root14/SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6、CS0。Ledger/diff-check交付前再验。

原runner首次漏DepthIntent参数编译失败、一次Python验证误把101/-1写成101/-101均有记录并纠正，未改正式source输出；最终原EXE/75-source hash不变。本两包共原pickup五脚本+缺帧三脚本，LF2Entity重叠，实际两新增测试、一个CPP、四生产文件。无schema/persistent/服务/关闭顺序变更，无Scene/资源/非战斗/Unity-GAS/Server/Gen/Plugins改动，无computer-use或提交。

本出口不包括完整后继held/WPoint/CPoint/throw/图片；source物理见证是拾取后holder原函数端点，真实Play同入口，未声称完整拾取后的整个战斗tick已经新增验收。剩余collision frame/CPoint/direct/raw/held等reader、display/post、Q07继续；旧World epoch formal recovery缺口与raw3MISSING保持。
