<!-- CHANGE-RECORD
id: NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001
status: VERIFIED
change-kind: NATIVE_OPOINT_BIRTH_VITALS
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleSpawnVitalsWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06SpawnVitalsEditorTests.cs
code-path: Tools/NTSD28AuthorityTrace/opoint_spawn_vitals_witness.cpp
authority: Formal Logan BattleWorld28::materialize_supported_spawns at7670-7706 and spawn_at1272-1278; playable materialize_native_frame_zero_entry calls it.
evidence: source and both synchronous post-init owners read; native vectors/RED pending
-->

# OPoint出生资源与显示初值事务

准确五脚本事前范围。新无状态BattleSpawnVitalsWriter.Apply(entity, point)按正hp/mp优先、OID5/52默认10/5及其他500/500，再读当前definition.stats正ohp/omp，以long乘除100；写canonical HP/HPBound/HP3/PP与MPMax（stats.max_mp存在且有效时取原值，否则取计算后的MP），四display值/step按出生值初始化。Health.MP、PPMax、PPBound等历史无authority镜像不随意更改；parent/relation/weapon_hp/位置/动作/随机/待复活字段不属本事务。

两个PostInitLiving最前同步消费，再保留原parent/link等主体，移除其旧OID5/52固定覆盖块。两个入口都在实际Init和当前FrameCache绑定后、返回entity或结束多生物迭代之前调用，中間无await，不新加生命周期/发布边界。此处是新出生后处理，不用于SnapshotShell（该入口不调用PostInitLiving）；正常bootstrap/Stage.Initialize和特殊clone出生的显示初始化留父DISPLAY任务随后补齐，不在未声明文件写入。

native runner用真实DatParser/World/ObjectDefinitionCatalog upsert及原materialize_supported_spawns完整创建child，不复制规则计算expected；覆盖符号/defaults/percent截断/int64、7种type、max_mp缺省/0/负/正、真实Logan目录的实际definition。SOURCE_MODEL_DIAGNOSTIC_ONLY不冒充正式EXE交互。

Editor tests先RED，原矩阵逐值/两PostInitLiving入口、同shell非默认重复初始化/无稳态分配、实际logic factory与真实Scene两个materializer，以及原13display/HP/MP/OPoint/回放相关回归、完整SelfCheck、有序关闭零残留。直接post-init seam只证明writer接线，不能代替完整factory/Play；真实DAT probe须标明source内容身份和受控输入，不修改正式配置。

无新runtime字段、schema、服务/manager/queue或关闭职责，13/21/24/2/2及十一阶段保持。回滚须批准仅本五脚本差量，不动前包/user文件；禁止computer-use/非战斗/框架/Scene/正式资源改动。记录实际source函数名为materialize_supported_spawns，前Task中materialize_frame_spawns称谓不准确，已纠正。

新发现独立reader缺口：native DatDocument.frame为0..998未声明帧返回各自零帧，Unity FrameCache上界857且HasFrame只判声明帧。现有HP/MP资格用HasFrame，原native矩阵invalid只覆盖9999；不得把其PASS推广为未声明帧通过。另立ZERO-FRAME-CACHE-CONTRACT任务追全consumer并回访HP，不在本出生资源包顺手改FrameCache。本包源fixture采用明确存在的action0/实际声明frame，保持工作独立推进。

## RED与生产实现

Native fixture3716已生成，source manifest07CD47A…保持。实际目录首次因OID0未被native生成而exit3，原失败TSV保留；runner按ObjectSpawnPlanner28的非正oid忽略规则显式排除并输出stderr，正在重建。Unity新10项RED全部FAIL（job4af2b7e4d7a34eb4b04d20843dff37bc），两caller/四type全factory分别得到10/500/17而目标50，原XML归档。现新writer及两PostInitLiving已按声明写入，原低HP块删除；保留parent/link/运动及历史镜像，生产正在编译。

首轮Unity10/10 PASS（job6618199fc0a64b3cae51388d52a07c5a），含3716 native及四type真实logic factory重复出生。新增两profile实际Logan329 metadata与native完整materializer输出比较，catalog3900ECBC509557DB；新同文件Play probe检查两种完整materializer各出生两次、返回前vitals/display、立即释放及World checksum恢复，再既有Q05关闭。当前相关合并job57d4683aac0a40fdaf45fa5eb6f97d51运行中，生产未再改。Native实际目录最终329/330（OID0按原规则跳过）、重复输出一致，native-validation.json留证；正式EXE/source manifest保持。

联合回归job57d4683aac0a40fdaf45fa5eb6f97d51：212/212 PASS，144.7277秒；本包12项含3716矩阵、两profile各329实际Logan定义写入（catalog3900）、两caller/四type工厂与复用，另前181及owner传播相关项通过。focused-212-pass.xml和两profile actual-births.json归档。此时SelfCheck/实际Play尚待，不提前VERIFIED。

## Play路由假设纠正（修改探针前）

首次Play两次logic完整出生101/201正确，route1断言Renderer存在失败，因为当前Scene UsesLogicOnlyEntityMaterialization=true，表现工厂合法转发logic。完整checksum已恢复，既有Q05关闭全0，失败和cleanup均归档。准备用同一声明Editor探针在paused/idle snapshot边界保存该非快照flag，以现有SetLogicOnlyEntityMaterialization在各route选true/false；finally先归还child、恢复原flag，再恢复完整World/checksum并断言flag一致。setter只禁止_tick中切换，当前暂停且无tick；不改生产或场景配置，不放宽任何运行时gate。记录原/恢复flag，renderer分支若其他前置失败必须如实报告。

## 最终限定出口

VERIFIED / OPOINT_VITALS_AND_DISPLAY_BIRTH_ONLY：native3716及实际329、Unity212/212、完整SelfCheck、两完整路径各两次真实Play/模式与checksum恢复4→4/关闭全0及两帧Stopped通过，CS0/dirtyfalse/root14/用户Scene SHA保持。初次OID0诊断准入和Play模式假设失败留证。详见同ID artifacts/REPORT.md。未完成整个OPoint/父display或零帧资格；HP/MP资格已降级待补，下一ZERO-FRAME-CACHE-CONTRACT，不跳post/Q07。

最终账本验证PASS：515 records/22 governed code diff；日志ledger-final.txt，历史Record未在当前diff的WARNING保留。总目标继续ACTIVE，无用户输入阻塞。
