<!-- CHANGE-RECORD
id: NTSD28-Q06-BDEFEND-FIELD-FAMILY-UNITY-001
status: IN_PROGRESS
change-kind: NATIVE_BDEFEND_FIELD_FAMILY
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleOrdinaryCharacterDamageRouteResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06BdefendFieldFamilyEditorTests.cs
authority: Current native +0x0B8 field overwrite45, reduced signed add, armor match input and C25h recovery; original256 source witness and full-driver0/45 first difference.
evidence: Current actual damage/armor reader/Shadow still use legacy HitStateCount rather than Runtime.Bdefend; C25h already correctly decrements Bdefend.
-->

# Native Bdefend字段接入

IN_PROGRESS / TEST_FIRST。准确四脚本。先原256两profile direct/实际Shadow，设置独立legacy HitStateCount=241验证不误写；比较raw47/3、Bdefend、armor结果及既有C25 held/free恢复，保留任何未覆盖分支差异。

实际范围：BattleDamageWriter的StandardCharacter/Weapon/StandardObject无护甲45写入、AlternateDamage有符号累加及对应阈值读取；BattleOrdinaryCharacterDamageRouteResolver的defenderArmorDelay输入；HitPlan捕获独立TargetBdefend并在已映射StandardObject/StandardType3/Type3D1/ActiveD1/StandardCharacter/Alternate预测和比较。新增仅transient WriterEffectSnapshot字段，无persistent schema变化；原TargetHitStateCount继续捕获比较以发现误写。已存在非角色首type0 feedback早返必须保留；不统一对所有hit写45。

不修改HitCounters API/copy/checksum、C25恢复、OID300/kind7旧分支或当前/Prev2/Y等其他响应条件；若原256暴露这些必要依赖则另明示source/Record，不隐藏失败。签名加法直接写Runtime.Bdefend，不能调用clamp型AddBdefend。

验收compile、原256/当前完整driver、相关旧回归、SelfCheck、真实两factory/Shadow/关闭、Scene保持。Spark RNG残留另任务，不豁免。回滚仅本差量并按规则授权；禁止computer-use、Scene/资源/非战斗/GAS/Server变更。
已写256原输入测试×两profile/direct与actualShadow：legacy字段独立241，raw47/3和现有private C25h helper的held/free衔接；timer测试仅临时纯module实例，无生产注册或新增关闭模块。生产尚未改，准备RED。

RED e237d2773ed24bf89f43cfd848ae9b4e四组失败，raw before无首差，原Bdefend覆写/累积及不同legacy哨兵已复现；red目录保存XML/JSON。开始已声明actual/armor reader/Shadow修复。

生产已写：三个无护甲45写Runtime.Bdefend，Alternate signed加与阈值读取Bdefend，armor delay读Bdefend；五映射Shadow45及Alternate迁移，新增transient TargetBdefend capture/compare（沿计数器mask37），旧TargetHitStateCount仍观测且不在这些Native分支写入。原兼容字段/模块和C25未改，待复跑。

13d3a590cf744b6dad6ee19c0fe49b8e八项均FAIL但原完整driver Bdefend0/45已清，只剩Spark RNG。字段256实际after修复direct968条/Shadow1000条：角色全部、type3/5无armor的raw/field已匹配；96非角色首type0误走伤害、64无armor武器反应仍错，另32type5 Shadow诊断待区分valid/count。非角色feedback原只有append_confirmed_native_spark后return，不改raw；因此必须先完整Spark事务，再反馈early-return，不能只跳过Bdefend写入。暂不关闭字段家族，所有原失败归after-field-fix。新SelfCheck已请求。

## 当前非关闭检查点

字段修复已写、角色/普通type3/5原字段吻合，当前完整driver bdefend已清，只剩Spark RNG；但256仍被96feedback、64weapon reaction及32type5 Shadow guard首差阻止。SelfCheck旧观测经独立Record纠正，07:18:42Z完整PASS，未新增Play。本包保持IN_PROGRESS。优先级改为Spark→非角色armor feedback→weapon reaction/type5 plan→回256/full driver。准确证据和数量见同ID artifact REPORT，不豁免任何失败。

后继更新：HIT-SPARK-UNITY-001已限定VERIFIED，旧完整driver四组现PASS/Spark RNG差异清除。当前父记录不关闭，继续非角色反馈→武器反应/type5覆盖→回BDEFEND256；旧96/64/32未在本轮重测。源/测试/Play见spark artifact REPORT。

后继生产进展：NONCHARACTER-ARMOR-FEEDBACK已写前置/反馈并通过684四组、Play2736、自检和本地回放；正式BDEFEND入口现direct192/Shadow208，余64weapon+16type5 guard。完整984余124weapon+108noncharacter reduced，父包保持IN_PROGRESS。下一weapon→type5→reduced后回访。
