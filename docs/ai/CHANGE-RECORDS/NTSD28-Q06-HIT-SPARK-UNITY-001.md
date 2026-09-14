<!-- CHANGE-RECORD
id: NTSD28-Q06-HIT-SPARK-UNITY-001
status: VERIFIED
change-kind: NATIVE_HIT_SPARK_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeHitSparkWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06HitSparkEditorTests.cs
authority: Formal Logan playable battle_world.cpp append_confirmed_native_spark and three callers; 438 source witness SHA b5df61136227ffcc17f15947ca22360d1234586ca71aa94907632d5e8eece6f8.
evidence: Existing RecordKind0Hit loses original index and selected armor, reads current frame, uses legacy RNG and incorrect geometry; full-driver RNG first difference remains.
-->

# Unity 命中火花事务

IN_PROGRESS / TEST_FIRST。先以源438完整双host数组和CRT轨迹测试现有入口；随后新增无资源static writer，严格实现guard/host/capacity/编码/snapshot/几何/Y-X CRT顺序。范围仅上述七脚本。

SequenceRunner以LF2Entity的栈式值类型scope携带原candidate index，using/finally覆盖BeforeDispatch与Dispatch，退出恢复旧值；不使用持久CurrentItrIndex，不修改snapshot/schema。直接调用没有scope时，仅在snapshot itr引用匹配时使用该序号，否则保持手工兼容调用默认0。不得由runtime itr拷贝猜序号。

DamageWriter在已判定的角色unarmored/reduced尾部拥有emission，使用命中前route.Armor；两个Character resolver移除紧邻该writer的重复Record调用。旧resolver虽未确认主生产可达，其同一writer调用在签名职责变更后必须避免重复；其余OID300调用保持。Weapon/special已有Record通过公共writer接入，feedback缺失仍另任务，不假装本包闭合该分支。

Writer不创建服务、队列、Renderer或pool；命中记录仍由原实体C01/C25和关闭第7/8阶段回收，停止新tick/输入的第1阶段关闭上游dispatch。scope无独立资源、同步退出不留状态；新增测试覆盖嵌套、异常恢复、实际candidate拷贝和复用。十一阶段顺序、共享随机算法、C17例外、schema/非战斗/GAS/Scene/资源/Server保持。

验收：438两profile及两factory完整记录/CRT；实际路由一次写入、kind非0/容量/缺帧、原index>0，原完整driver与Shadow、自检、真实Play/关闭/Scene hash。先保留RED，不将源438的诊断端点等同完整行为验收。回滚只撤本任务精确差量且遵守用户审批规则，不覆盖其它工作。

RED 9cf5f861四组失败：logic438旧records/CRT首差，renderer尚有测试前置NullReference，完整XML/JSON保留red目录；后者需先补测试catalog前置，不能当生产失败。

实施前补充准确第八脚本HitPlan：其ProjectKind0HitRecord仍复制旧current/legacy随机计算；必须独立投影新CRT state/count与record，reduced显式传selectedArmor。scope提前覆盖Prepare observation至Dispatch/Observe确保预测和actual均拿到原index。保留全部已有mask观测，不弱化Shadow。Renderer RED根因为EditMode无Awake初始化pool，改真实Play跑两factory，不修改生产pool初始化。

首次compile四条CS1061：CRT观测原只在另一个pair snapshot，WriterEffectSnapshot尚缺字段。按本Record补独立transient RngCrtState/RngCrtCalls、capture/compare并保留legacy RNG字段；不改persistent schema或跳过随机观测。

实际已写8脚本：Append完整源事务；LF2Entity scoped index及无护甲wrapper；统一runner作用域涵盖Shadow预测和actual；角色writer reduced传hit前armor、普通尾部唯一emission；两个外层重复删除；HitPlan独立几何/CRT transient capture/compare。测试新增源端点、源288角色路由+拷贝itr、异常嵌套、非0 guard与Play双factory876。编译/新测试待执行，父任务仍IN_PROGRESS。

e4a87be9最终10项6PASS/4FAIL：完整driver四组首次PASS，异常/非0 guard通过；端点各96和actual各64差异全部cover非0。定位是新测试误用ConvertToFrameData legacy入口，现成ConvertLoganFrameData/LoganCombatRecordDecoder已经正确处理cover；修测试走正式Logan converter，不修改生产parser/资源。原矩阵route数134/152/152，实际排除feedback与missing后的精确count为282（此前288是人工误算）。全部FAIL保留after-emitter。

c1260a2d最终10/10通过：源438×2=876端点、原真实角色路由282×2=564、scope异常/非0 guard、full driver四组8向量与Shadow全部0差异。独立22生命周期21PASS/1旧SimTU测试hook FAIL；新SelfCheck旧无World fixture FAIL，分别由HIT-SPARK-SELF-CHECK-ORACLE-001、SPARK-C01-TEST-HOOK-001记录。原证据未删，Play尚待。

补已声明验收中的本地快照回放：同源四个route/index组合、两个profile，先源endpoint后完整tick1快照，tick2/3恢复重放比较完整checksum、双host数组与CRT状态/计数，scope退出仍0。仅本地恢复，不覆盖已知跨World allocation epoch缺口。

Play首轮08:07:56Z源端点876 PASS、Scene checksum不变、Renderer2→2；08:08:44Z关闭PASS。为满足实际分发双factory出口，复用已过的282角色route helper进Play，同批将验证端点876+actual564=1440；只有测试代码补验，不改生产。

VERIFIED / DECLARED_HIT_SPARK_TRANSACTION_SCOPE。最终34/34、source876端点/actual564、原full driver四组、local replay16ticks、SelfCheck08:06:43Z、真实Play1440与08:11:53Z有序关闭均PASS，Scene hash保持。准确命令/job/范围与失败回溯见同ID artifact REPORT.md。非角色armor反馈仍下一Task；不将父任务或总目标标完成。
