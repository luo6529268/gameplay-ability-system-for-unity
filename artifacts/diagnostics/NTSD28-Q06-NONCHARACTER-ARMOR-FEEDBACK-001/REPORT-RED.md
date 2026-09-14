# 非角色护甲反馈 Unity RED 基线

IN_PROGRESS / RED_CONFIRMED_BEFORE_INPUT_MATCHED。尚未修改本批Unity生产代码，不能报告反馈已经实现或对齐。

原输入来自PREARMOR-FEEDBACK-SOURCE-WITNESS-001的984向量；新Editor测试使用正式Logan converter构造三个实体，冻结候选后设置相同关系、动作锁存、current第一bdy、rest、速度、Bdefend和面对方向。

Unity编译error CS为0。EditMode job c447868aa79045e9b4192290ad605a05终态FAILED，两profile均实际跑完984例：受测before raw47/3以及额外links/rest/spark数组0差异；每组after有4641条差异、涉及712个case。并不是1968个case全部失败；raw其余3个未绑定字段不能据此称初值完全对齐。完整XML和JSON在red/。

## 已定位的生产差异

- 无护甲武器反应仍有team/action/hitReactionTimer等已有首差，属于已排队UNARMORED-WEAPON-REACTION，不能用它掩盖当前护甲反馈。
- 首type0 armor、rest5时原追加spark/CRT2，Unity在CanConsumeRecordedCandidate提前拒绝，没有火花。case31等是清晰隔离见证。
- reciprocal 2/-2时原target Vy=-1.0000000000000258、child action=3（seed42 synchronized0xEC/6）、child Vy7.5保持；Unity旧consumer既错过正确顺序，又用legacy RNG/ImmediateFrame和错误字段。case36/37等捕获raw、links、3x3 rest和完整随机差异。
- special link rest需要在无护甲前置处理，可作用于dormant历史；不能只用当前active held getter或击倒尾部helper。
- 第一bdy1033/1100500000应在前置及type0反馈之后；Unity先跑第一bdy响应，早于ConsumeEffects。case732等及源24个反馈跳过第一bdy例证明需改生产入口顺序。
- type1 bypass与active/reduced必须分开；state7 defense的rest早返结果也影响上层fallback，详见原源REPORT，不能用target has armor或state7的简单bool替代完整调用顺序。

## 修改入口与约束

实际四种consumer（LF2CharacterInteractionResolver、LF2CharacterDatInteractionResolver、LF2WeaponInteractionResolver、LF2SpecialAttack）汇入BattleHitCandidateSequenceRunner。该入口当前次序为live-vrest门→runtime itr/资格→first-body→consume-effects→Shadow观察/实际backend dispatch。源码要求普通hit先确定defense/armor入口分支，再在unarmored路径执行release和special-rest，之后armor反馈，再普通rest和first-body。

下一步继续本Task，先把此顺序建立为单一正式candidate事务owner，声明准确生产code-path和Shadow预测位置后实施。不能仅在ApplyWeaponDamage/ApplySpecialAttackDamage最前面加return；那时可能已被rest拒绝或first-body改写。也不能在runner和backend重复消耗release RNG。

需要核清的准确已有入口：LF2Entity.ApplyHeavyHeldTargetReleaseConsumeEffect、BruteForceSceneQuery.ResolveRuntimeItrForPair的release flag、BattleDamageWriter的普通/typed/defense route，以及HitPlan preprocess/consume/writer三个观察阶段。原生action_latch映射Runtime.WaitCounter；源rest矩阵直接是[0][2]=45、[1][2]=30，不按字段名称反向解释。无关系native links默认0已作为本测试显式初值赋值，不证明Unity默认spawn links一致。

当前Change Record只声明单测试脚本，任何生产修改前必须补精确路径、符号、前后职责、关闭边界与验收；无需再次询问总目标授权。Source诊断CPP是独立Record。不得绕过未通过用例或关闭BDEFEND/父collision任务。

上一批Spark的34/34、SelfCheck08:06:43Z、Play1440/关闭仍是同一生产代码的既有通过证据；本轮没有新Play或完整SelfCheck。新BDEFEND256四组重跑仍direct968/Shadow1000条差异，来源在source artifact unity-parent-red。

保留Unity/GAS/非战斗/Scene/资源/Server边界，禁止computer-use；Q07未部署、raw47/3、schema15/23/26/2/2和用户例外保持。总目标ACTIVE / FULL_ALIGNMENT_INCOMPLETE。
