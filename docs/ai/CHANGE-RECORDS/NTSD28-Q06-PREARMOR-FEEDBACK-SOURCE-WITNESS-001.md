<!-- CHANGE-RECORD
id: NTSD28-Q06-PREARMOR-FEEDBACK-SOURCE-WITNESS-001
status: VERIFIED
change-kind: AUTHORITY_SOURCE_DIAGNOSTIC
code-path: Tools/NTSD28AuthorityTrace/prearmor_feedback_witness.cpp
authority: Formal playable battle_world.cpp resolve_confirmed_unarmored_hit release/special-link-rest/first armor/feedback/rest order and simulation_tick_driver.cpp actual type1 selector.
evidence: Existing Unity heavy release uses legacy RNG/ImmediateFrame and different rest or velocity owners; simple feedback early return cannot preserve the native transaction.
-->

# 护甲前置及非角色反馈原函数见证

IN_PROGRESS / SOURCE_ONLY。只新增诊断runner，不修改权威目录、正式EXE或Unity生产。实际代码而非注释决定rest矩阵方向；world.victim_rest(target,attacker)直接读取target.map[attacker]。先720输入：types1..6、armor none/0/1/2、五种关系（none、2/-2、1/-1、坏reciprocal、dormant）、三种special gate（无、effect12/caughtact-3、action_latch20首bdy50）、预置target rest0/5。完整三实体raw、3x3 rest、spark arrays、CRT/native轨迹及status前后一起捕获。

从正式入口resolve_ordinary_unarmored/type1依原driver firstarmor selector调用；冻结候选后设关系和rest用于诊断顺序分离，不能将端点fixture当完整driver reachability证明。type1/reduced不是type0 feedback，明确保留其结果。

验收现有Build-AuthoritySourceCapture脚本构建、两次runner逐字节一致、机器校验数量和事务不变量、正式身份manifest。回滚仅本新增文件和本报告，遵守用户批准规则；不改Scene/资源/非战斗/GAS/Server，禁止computer-use。

首版构建退出0，正式EXE/75源身份保持；生成轨迹前补证据字段：raw50不含持有关系，必须单独输出state/parent/child。另发现Unity FirstBodyResponse在ConsumeEffects/armor之前，原在prelude/armor反馈之后；扩充48个冻结后current30第一bdy1033或1100500000案例（types1..6、none/0 armor、reciprocal release、rest0/5），总768。先重建后才运行，不能把旧binary归为新源码产物。

768源两遍退出0/逐字节一致SHA3268867a…a916。首validator的type1不运行prelude假设在case66失败：fixture ratio15<Bdefend17，全type1实际bypass后进入无护甲前置，不是active reduced。保留intermediate-768轨迹，补180个type1 Bdefend0 active和36个state7面对攻击的普通defense优先案例，最终984；明确记录bdefend/defense，避免把bypass样本当active证据。Unity当前BDEFEND4组重跑仍FAIL direct各968/Shadow各1000，原文件归unity-parent-red。

VERIFIED / SOURCE_MODEL_ONLY。984原向量/5108检查、两遍逐字节一致SHA12323954…c0a4a，正式身份保持。完整source字段/返回语义/coverage correction见同ID artifact REPORT。Unity新测试待RED，未改生产，不能称完整反馈已实现。
