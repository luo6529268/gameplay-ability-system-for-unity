# NTSD28-B6-CPOINT-KIND2-HURT-CONSUMER-RETIREMENT-PRODUCTION-001

本合同在本轮任何脚本修改前建立。状态：BLOCKED / PRECHANGE_SCOPE_BLOCKED / PRODUCTION_UNCHANGED_FROM_P3。
需求来源：用户2026-09-10批准三个独立退休包并限定文件范围，硬性要求既有测试保持PASS；Goal14 triage仅为定位线索，当前Authority需重新核验。
范围：退休无Authority的CPoint kind2 hurt覆盖；字段/converter保留，旧snapshot布局可读取但续跑规则改变，不承诺旧规则等值重放。
原状与实际路径：
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
- Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs

新增focused测试及meta已获授权，尚未创建；实施前须将实际测试路径登记code-path。无新runtime模块、无关闭阶段变化。
前置：包1沿当前playable caught/damage/settlement链正向证明无hurt consumer；包2/3完整repo caller/serialized/反射名称审查，发现生产caller即硬停。当前整体暂停源于旧测试范围冲突，见Temp/Goal17_PrechangeScopeReview.md；不得擅自修改未授权旧测试。
不变量：schema/Snapshot/Checksum/shell、NTSDSpec本体、DAT/converter、Scene/资源不动；不引入FrontHurtAct/BackHurtAct替代，不修改其他effect/impact/caught逻辑，不清理用户修改。
验收：新focused先RED后GREEN；三包一次共享B6(583+新增)、前置92/80/17/72/24/23/32/140、refill9、fullSelfCheck、双build0error、validator、固定Scene SHA/dirtyfalse。新测试失败与旧测试失败分开报告；本轮尚未运行上述检查。
副作用/兼容：仅改变各自退休行为；旧snapshot字段布局不变，但不能承诺旧行为续跑等值。外部程序集/API兼容与repo内部caller区别记录，不凭repo扫描断言不存在所有外部调用。
回滚：用户明确批准后仅反向本包增量，保留当前P1/P2/P3未提交基线。当前脚本增量为0，无需回滚。禁止Git清理/提交/push与跨包顺手修复。
当前验证：只读工作树、源码和Unity状态检查；未运行本轮RED/编译/回归/Play。没有报告新测试失败；这是确定的事前授权范围冲突。
