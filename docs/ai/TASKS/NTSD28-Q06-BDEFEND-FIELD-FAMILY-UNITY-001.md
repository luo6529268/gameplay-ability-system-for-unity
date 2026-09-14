> 当前字段已写但父包IN_PROGRESS。新依赖顺序：HIT-SPARK-TRANSACTION-AUDIT→NONCHARACTER-ARMOR-FEEDBACK→UNARMORED-WEAPON-REACTION/TYPE5-HIT-PLAN-COVERAGE→回256及完整driver。反馈只调用spark即返回，所以Spark是必要前置，不是跳过Bdefend验收。

# Unity Bdefend字段家族接入

READY_EXACT_CALLERS_AND_TEST_FIRST_RECORD。前置原256/1280限定VERIFIED，见BDEFEND-FIELD-FAMILY-SOURCE-WITNESS-001报告。原case按四分支分布148覆写45/96保留反馈/6有符号累加/6armorHP保护，不能只改一条45或统一给所有非角色写45。

1. 先准确定位实际type0/weapon/object unarmored writer和source分支，区分非角色首armor type0反馈早返；为准备修改的BattleDamageWriter、BattleOrdinaryCharacterDamageRouteResolver、BattleEcsHitExecutionPlan及新测试建立精确Record。
2. 用原256初值写RED。Runtime.Bdefend和legacy HitStateCount必须设不同值以暴露错误读取，不能用两个初值相同的fixture。比较原raw/分支、签名45覆写或带符号加法、armor匹配/绕过、已有C25 held/free衔接；两profile/actual两factory/Shadow。
3. 无护甲和真正的reduced分支按原写Runtime.Bdefend；正负原值都保留语义。不要使用clamp型AddBdefend处理有符号累加。type1 armor匹配输入同步Bdefend。Legacy HitStateCount字段与兼容API/copy/checksum保留，禁止global alias。
4. HitPlan新增独立Bdefend前后观测/预测/比较（transient projection字段，不是新persistent schema），保留旧HitStateCount观测以防误写；逐一确认StandardObject/Type3/D1/ActiveD1/Character/Alternate对应source，不凭名字统一替换所有45。
5. 不修改已验证C25h恢复；OID300、kind7旧branch和reduced current/Prev2/Y条件在现有256覆盖外，若实际为必要耦合，先补原证据并准确扩范围，不能宣称整个defend链已完成。
6. 编译、256与原完整driver、旧相关回归、SelfCheck、真实Play与关闭验收。完整driver仍会有Spark RNG失败，保持原失败且路由至HIT-SPARK-TRANSACTION-AUDIT，不豁免两次旧随机流。

禁止修改Gen/Plugins/Server、Scene、资源和非战斗/Unity-GAS框架。用户HUDBg30/场景hash保持，raw47/3与epoch恢复缺口未自动关闭，Q07正式DAT/图片尚未部署。禁止computer-use。
