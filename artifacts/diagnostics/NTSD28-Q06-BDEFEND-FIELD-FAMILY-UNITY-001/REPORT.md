# Bdefend字段家族当前实现

IN_PROGRESS / FIELD_FIX_WRITTEN_DEPENDENT_HIT_PATHS_OPEN，2026-09-14。

## 已写修改

- BattleDamageWriter的普通角色/武器/对象无护甲45写入改为Runtime.Bdefend；AlternateDamage有符号累加和对应阈值读取同步，不使用clamp型AddBdefend。
- BattleOrdinaryCharacterDamageRouteResolver的type1 armor匹配defenderArmorDelay改读Runtime.Bdefend。
- HitPlan新增独立transient TargetBdefend捕获/预测/比较；映射standard/D1/alternate分支同步。旧TargetHitStateCount继续观测，兼容字段/API和persistent schema不改，C25h恢复保持。
- 新测试对照原256，显式将legacy HitStateCount设241，与Bdefend初值分开；验证actual/Shadow、raw47/3与既有held/free timer衔接。

## 实际验证

RED e237d2773ed24bf89f43cfd848ae9b4e：四组256均FAIL，direct各1404、Shadow各1436差异，before raw一致；主要有错写旧字段及后继恢复。原输出保存red。

修改后13d3a590cf744b6dad6ee19c0fe49b8e：8项仍均FAIL。原256 direct各968、Shadow各1000差异；原完整driver四组的bdefend0/45已消除，只剩Spark RNG legacy3/native0/CRT0对源CRT2，after-field-fix保留JSON/XML。不得把整组测试写成通过。

已观察到角色type0所有无护甲/type0/type1路径的raw/字段匹配；type3、type5无armor的raw/字段也匹配。剩余准确归属：

1. **96非角色首type0 armor**：原feedback-only，Unity误走damage。Bdefend错写和其他raw首差属于缺失早返事务，不应只改其Bdefend结果。
2. **64无armor武器(types1/2/4/6)**：Bdefend正确，但每例battleGroup1/2、随机动作3/原186、hitReactionTimer0/80三项反应首差。具体随机动作依当前seed，不是恒定3。
3. **32 type5 Shadow附加诊断**：mask0但valid/count条件失败；当前日志尚未输出哪一项，需要细化，而不是删除断言。type5无armor raw正确不等于优化路径已验收。
4. **完整driver Spark RNG**：额外两次legacy不能算C17例外；只有legacy1属于既有掉落例外。

完整SelfCheck请求07:14:00Z、07:14:34Z FAIL，因旧StandardCharacterHitSnapshot仍捕获HitStateCount45。独立BDEFEND-TEST-ORACLE-001将准确StandardCharacter/C30/Alternate fixture观测改为Bdefend，保留HP/PP/统计/rest/动作断言；请求07:17:48Z、**07:18:42Z完整SelfCheck PASS**。原FAIL保留。不全局修改legacy测试。

本轮没有新真实Play验收，不能沿用前轮Play作为新生产验证。最终Editor idle非Play，无运行中test/build/exec；Scene文件SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。无computer-use、非战斗/框架/Scene/资源/Server或schema改动。

## 更新后的依赖顺序

原battle_world约6464在selected armor+noncharacter时调用完整spark事务即返回（之前还有special-link rests与选择前置）。因此下一必须先**HIT-SPARK-TRANSACTION-AUDIT-001**，然后**NONCHARACTER-ARMOR-FEEDBACK-001**，再**UNARMORED-WEAPON-REACTION-001**及**TYPE5-HIT-PLAN-COVERAGE-AUDIT-001**。最后回256/完整driver/两factory运行验收，才能关闭Bdefend、qualification和collision父包。

不是忽略Bdefend剩余项，而是其反馈路径依赖Spark，不能用空return替代。已写三个明确Task，用户指定总目标不缩小。Q07正式DAT/角色图未部署、raw3MISSING/epoch恢复缺口和stage.dat USER_HOLD保持。
