# 护甲前置与非角色反馈原函数见证

VERIFIED / SOURCE_MODEL_ONLY。984输入、5108检查，first/repeat逐字节一致，SHA12323954821eceea6cfc7aa3155ae66102c4389f4fdb47813dd8febc374c0a4a。原代码实际调用的结果用于后续Unity RED，不是Unity已对齐证据。

## 构建与输入

实际运行Build-AuthoritySourceCapture.ps1，最终OutputDirectory Temp/NTSD28PrearmorFeedbackFinal，RunnerSource Tools/NTSD28AuthorityTrace/prearmor_feedback_witness.cpp，ExecutableName prearmor_feedback_witness.exe；退出0，两遍runner退出0/stderr空。正式EXE SHA B1E13AE1…D2819033、75源manifest07CD47A0…D778F保持，最终runner源码EC81B6C6…CC0F3。详细manifest和完整输出在同目录。

720基本组合覆盖target types1..6、armor无/0/1/2、关系无/2与-2/1与-1/坏reciprocal/dormant、gate无/effect12 caughtact-3/latched20首bdy50、target rest0/5。48项补冻结后current30第一bdy1033或1100500000；180项补Bdefend0的type1 active；36项补state7面对攻击的defense与rest优先。每例完整before/after三实体raw50、独立links(state/parent/child)、3x3rest、spark数组和两类随机轨迹。

冻结后改变关系/rest/current属于诊断初值分离；不能单独证明完整driver可达或物理输入验收。测试明确设置source初值links，不能据此宣称Unity默认spawn links已对齐。

## 已确认的顺序与实际字段

1. 原ordinary unarmored路径在选择armor和普通target rest拒绝之前处理reciprocal 2/-2 release。其第一随机调用是synchronized(0xEC,6)，本seed结果3，写child action；清target/child interaction_state，保留parent/child槽历史。
2. 速度写到target/holder的motion.y=-1.0000000000000258，child原Vy7.5保持。
3. 以world.victim_rest(targetSlot,attackerSlot)的实际getter定义矩阵，原写入是[0][2]=45、[1][2]=30；不能依据字段名或源码注释反转它。初始target rest[1][0]=5保持。
4. special-link-rest gate在armor之前，可在1/-1和dormant关系上写同一45/30矩阵，不需要清关系或消费release随机数。只要求保留的linked_child与child.parent互相匹配；不是ResolveActiveHeldSlot的同义词。gate读取action_latch首bdy（本矩阵kind50），不是previous itr。
5. 首type0所选armor针对非角色反馈在普通rest拒绝和current第一bdy处理之前：210例feedback全部applied，只允许上述前置变化，保留HP/Bdefend/team/frame等其它raw，追加一个id10火花和两次CRT。armor.spark199在反馈flag=false时不覆盖itr.spark。
6. 其中24个current第一bdy案例仍留在action30，不触发1033动作或encoded chance RNG；仅已触发的release有0xEC调用。
7. type2不支持分支有180例，但依然在执行相同无护甲前置之后返回unsupported；198例普通rest拒绝也发生在前置之后。
8. type1 ratio15<Bdefend17会bypass，重新进入无护甲前置；后续result.selected_armor被补成type1并不意味着走type0反馈。Bdefend0 active或真正进入reduced的分支按另一顺序处理，96例reduced rest拒绝完全不改三实体/links/rest/RNG。
9. 一个必须保留的返回语义：defended wrapper调用reduced，而reduced在rest>0提前返回时尚未拷贝defense_decision，unarmored wrapper因此继续无护甲分支。state7+首type0+rest5的6例最终仍feedback；type1顶层入口直接返回reduced结果，不能把两条入口统一成一个简单defense bool。

结果总计：510 applied / 294 rejected / 180 unsupported。validation.json保留精确检查范围；active reduced其它具体损伤响应不是本报告新裁决的完整范围。

## Unity已观察问题及后继

- LF2Entity.ApplyHeavyHeldTargetReleaseConsumeEffect当前写holder对attacker的45、用legacy随机ImmediateFrame、把Vy写child，与源矩阵/owner/帧事务不同。
- SequenceRunner.CanConsumeRecordedCandidate一开始就检查live vrest；但原type0反馈在更后面才遇到普通rest门，因此有冻结候选情况下的顺序差异。
- SequenceRunner第一bdy响应位于ConsumeEffects之前；原前置/armor反馈在第一bdy之前。
- special-link-rest的dormant使用和分支上下文必须一起处理，不能用只在击倒尾部调用的ApplyKnockdownHeldPairVrest代替。
- 当前BDEFEND256四组重新运行仍FAIL：两profile direct各968、Shadow各1000条差异，见unity-parent-red；没有把旧96/64/32当成未重测的新结论。

当前Unity parent NONCHARACTER-ARMOR-FEEDBACK-001先建立单测试脚本，比984同输入before/after，随后按第一差异制定准确生产code-path。不得仅在DamageWriter最前面插入无条件return，或为通过测试恢复旧随机行为。源984不等于Unity/正式EXE动态验收。

原768中间输出和错误validator假设（type1一律不进prelude）保留intermediate-768。无Unity生产/Scene/资源/非战斗/GAS/Server变更，禁止computer-use。当前Spark生产上一批验收保持。
