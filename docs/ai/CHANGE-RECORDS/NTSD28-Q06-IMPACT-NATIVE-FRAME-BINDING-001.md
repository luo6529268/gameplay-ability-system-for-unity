<!-- CHANGE-RECORD
id: NTSD28-Q06-IMPACT-NATIVE-FRAME-BINDING-001
status: VERIFIED
change-kind: IMPACT_NATIVE_FRAME_SOURCE_WITNESS_FIRST
code-path: Tools/NTSD28AuthorityTrace/impact_native_frame_binding_witness.cpp
code-path: Tools/NTSD28AuthorityTrace/validate_impact_native_frame_binding_witness.py
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06ImpactNativeFrameBindingEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
authority: Current formal playable BattleWorld28::resolve_special_relation_hit kind10/11/17/18 and ordered native impact transaction.
evidence: Independent live caller/prediction audit and formal330 respond/default182 inventory; no fresh Unity RED yet.
-->

# NTSD28-Q06-IMPACT-NATIVE-FRAME-BINDING-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。Q06 remaining live reader确认BattleHitCandidateSequenceRunner→BattleDamageWriter.TryApplyNativeImpact的Action operation仅一处旧raw binder。Character/CharacterDat/Weapon共享caller；对应HitPlan.ProjectNativeImpactWriterEffect只预测frame/runtimeframe，不读旧getter，不扩大到旁边kind15。原B6 atomic integration的plan/资格/owner/operation顺序已VERIFIED，不重做。

权威为正式playable BattleWorld28::resolve_special_relation_hit kind10/11/17/18完整事务：character owner chain后写environment/-credit/source、Vx/Vz与pending、raw signed respond或182、公共Y/Vy；object action0或保留state1000/2000及2.3垂直分支。counter/latch/snapshot保留；无999→0或abs。完整字段before/after/following必须捕获，不能只验action。

当前第一写域仅Tools/NTSD28AuthorityTrace/impact_native_frame_binding_witness.cpp。20代表：12character绑定（182显式/隐式、857显式/隐式、999显式/缺失、69/900/998隐式、-1/-999/1000）；kind10为主交错kind11负environment与17。4object（type1/type2 action0显式/隐式），2preserve（type4/state1000、type2/state2000），2拒绝（kind11/env0、kind18/character）。YInt -2/-3、preciseY哨兵和Vy>-6/==交错；真实两级owner与候选，不修改权威数据/源码。角色使用有效owner chain；非目标实体状态也必须捕获。

内容证据：formal330有kind10=121、kind11=61，respond全0，无kind17/18；type0中182显式157/隐式1(OID899 c/0/spe.dat)。这是候选内容域，不证明实战碰撞可达。读表报告在remaining-live-readers artifacts，不宣称玩家Bug。

先source双跑和独立核对，再准确声明Unityfixture及单生产caller。验收按用户等价类：主路径代表+必要另一入口smoke，未变B6证据复用；闭包出口一次相关回归/SelfCheck/代表回放/Play和关闭，不做无意义乘积。保留raw3等未映射边界；源derived witness不是正式EXE录制。禁止共享getter/Kernel/schema/Scene/资源/非战斗修改；回滚只本ID精确diff，保留已有变更。Q06未完/Q07未迁移。

source20 runner完成且根构建session83693运行。准确新增validator validate_impact_native_frame_binding_witness.py：从captured before与params独立核算准入/impact有序字段结果、descriptor、pending与RNG不变，比较全部四实体after；不声称独立重建所有spawn初态，也不模拟following。生产及Unity测试尚未改。

IMPACT-NATIVE-FRAME-BINDING-001 source20已build并双跑一致SHA4041ba46914f43292e97058fb9c8687f67482cf384dbcb3590e1d0a0946d6edd，独立18285检查PASS（captured-before→四实体即时after、准入/descriptor/pending/RNG，不独立重建spawn或following）。Build manifest闭包07CD47…778F，源CPP FFBAFC…60D8A。下一准确扩Record新增单Unity fixture，复用正式catalog type与四实体factory，全部spawn后恢复0→1→2 ownerchain及target70 before；额外capture environment/catchSource/impactSource/pendingXYZ/descriptor，sourcepreviousXYZ仍明确source-only；主20代表+必要smoke，不扩乘积。先before0/即时/following分开RED再决定唯一BattleDamageWriter Action caller，生产未改。静态formal respond全0且唯一implicit182 OID899无bdy/itr，不认定实战Bug。前state1218已验不重做；Q06未完/Q07未迁移/总目标ACTIVE，无运行build/job/agent/Play，Editor62860复用；禁computer-use/非战斗/Scene/资源修改。

Unity脚本前扩充：新单fixture NTSD28Q06ImpactNativeFrameBindingEditorTests.cs，主Authority400/DataOriented20例、Mobile/Legacy6代表(0,2,4,12,16,18)。正式catalog的OID77/78/79与type配置，四槽0/1/2/70工厂spawn后统一恢复before，含owner链与impact/pending额外字段。直接实际DamageWriter.TryApplyNativeImpact后比即时，再实际fulltick1；before完整比较先行。previousXYZ仅source-extra不伪造，raw47+额外字段对照，源raw3缺口保持。只测试不改production，先RED。

新fixture编译CS0，首轮job062e45567b6444279ccde7344ff8947d两FAIL。主20 before240项全pendingX/Y/Z JSON整数与浮点tag（0=0/13=13）误报，smoke6 before72同因；原XML/JSON存initial-json-number-type-failure。仅这三个source/native double字段按Value<double>精确==比较，不加容差/不round，不修改sourceexpected或生产。实际字段值差异仍报错，等待复验获得有效RED。

有效RED job9eaa5a848e4148bca201c8c90f213da0终态两FAIL：主20 before0/即时15/后继5，smoke6 before0/即时3/后继1，全部descriptor/snapshot差异，原证据production-red。生产修改前扩准确路径BattleDamageWriter.cs，仅TryApplyNativeImpact内Action case单caller改victim.DirectWriteNativeRawFramePreserveWaitCounter；保留operation顺序及其它字段/plan/准入/HitPlan，负值raw与nulldescriptor不终止剩余运动事务。新编译+20/6复验确认因果，后继与原B6边界不扩。回滚仅本caller，保存其他同文件任务改动。

IMPACT-NATIVE-FRAME-BINDING-001生产单caller已修：source20/独立18285，Unity有效RED主20 before0/即时15/后继5、smoke6 before0/即时3/后继1；Action操作改native binder后20+6均before/即时/following0。联合job5b92ec860e18446cb36b010bef8608cb 53/53PASS(新2+literal3+相关HitPlan48)，约1.42秒，未跑旧B6全量；证据after-binding-fix，原JSON数字tag夹具失败及有效RED均保留。fullSelfCheck请求已提交，现Editor62860执行，结果待新Temp/NTSD_BattleRuntimeSelfCheck.result；不得重复启动/并行Unitytests或C#编辑。下一仅6代表sameWorld replay与Play(Authority/DataOriented/renderer6+Mobile/Legacy/logic6)及Q05关闭，脚本前准确登记Record；无需再跑矩阵/旧测试除非新失败。Task未关闭，Q06未完/Q07未迁移，总目标ACTIVE，禁computer-use/非战斗/Scene/资源修改。
独立fixture review确认owner链/typed catalog/源extra恢复正确；即时Unity用真实shared writer，不重验源候选生成；previousXYZ不比较，raw3保持。smoke6不覆盖type2及第二拒绝，它们由主20承担。

IMPACT单caller修后fullSelfCheck 2026-09-21T01:06:28.907250+00:00 PASS已归档after-binding-fix；此前运行中状态由本条覆盖，当前无运行job/build/agent/Play。联合53/53与20+6零差异保持，独立生产review单行通过。下一仅6代表sameWorld replay/Play及Q05关闭，不重复已过矩阵或SelfCheck。Task IN_PROGRESS/Q06未完/Q07未迁移/总目标ACTIVE。

代表验收扩展前登记：仅现有NTSD28Q06ImpactNativeFrameBindingEditorTests.cs新增6例(0,2,4,12,16,18)同World replay×Authority/DataOriented与Mobile/Legacy；先impact再两fulltick，恢复后逐阶段一致和旧RNG cursor失效。Editor-only ImpactPlayProbe复用现有模式，仅Authority/DataOriented/renderer6与Mobile/Legacy/logic6共12，Scene checksum/borrowers保持，再既有Q05关闭。保留主20默认，Play/代表输出分开避免覆盖；当前生产不改，SelfCheck证据复用。

最终VERIFIED_SCOPED：同ID ACCEPTANCE.md完整回链source20/18285、有效RED→零差异、53/53与SelfCheck、代表回放12/24tick/Play12/Q05关闭PASS。单caller及既有B6职责保持；不关闭Q06。
