<!-- CHANGE-RECORD
id: NTSD28-Q06-UNARMORED-WEAPON-REACTION-001
status: VERIFIED
change-kind: NATIVE_UNARMORED_WEAPON_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06WeaponReactionEditorTests.cs
authority: Current playable unarmored weapon damage/reaction/impulse/rest/post/audio/spark call chain and WEAPON-REACTION-SOURCE-WITNESS-001.
evidence: Existing 64 Bdefend and124 prearmor weapon cases disagree; live Unity has old random frame/team/self-rest tail, Fall reset and pre-post ordering/rounding differences.
-->

# 无护甲武器完整反应接入

IN_PROGRESS / TEST_FIRST_ONLY。当前准确写范围只新增单Editor测试，不修改生产。原2100向量从同Logan converter和冻结候选初值进入正式candidate pipeline，比较before/after raw47/3、额外impulse/count/统计/links、rest、audio/sparks/完整随机轨迹及主动释放hold后同一finalizer结果。两profile先RED，匹配输入后再声明准确生产/Shadow paths。

完整反应不能只改frame186：强制Fall80应保留；原武器没有旧victim team/random frame/self-rest尾部；0xEE/16攻击者state1002在rest之后；type4/6的0.55为double并有累积替换分支；dvy0分支不夹紧正Y；保持stats、type6资源跳过和其他已验证前置/反馈。

测试finalize先把hold置0是显式诊断输入，不代表修改真实hold时序。音频只验证战斗事件，不迁移音频资源，不改通用Audio/UI。正式资源/Scene/GAS/非战斗/Server/schema不改，禁止computer-use；回滚只本差量且遵守用户授权。

首RED两组2100 before各2100条仅为extra double序列化1.0与源JSON整数字面1的JToken类型差异；raw本身输入匹配。只把expected extra的x/y/z按源C++double语义读为double，再严格比较实际数值，不改其它字段/原向量或生产。原失败保留red/。

生产实施前准确范围补记：两profile各2100已完成RED，before0；原报告保留red-normalized。上述三个生产路径仅修改ApplyWeaponDamage及其局部native反应/同步post、effective kind0 heavy预处理和对应Shadow独立投影。保留已通过的prelude/spark/HP/C25、有序关闭/资源/框架；新鲜2100 direct/Shadow、684早期/完整984/Bdefend回归、SelfCheck和定向Play为出口，未过不标VERIFIED。风险为原旧武器尾部依赖及Shadow覆盖；回滚仅本Record差量，不覆盖现有用户或其他批次修改。

CODE_WRITTEN（尚未验证）：ApplyWeaponDamage改完整native武器事务，新增局部水平/攻击者post函数；BruteForce和Shadow预处理保留effective kind0原dvx/dvy。Shadow writer预测下一步接入，当前不得提升运行验收状态。OID100音频暂沿现有SFX_039映射，额外边界尚未源fixture覆盖。

首次生产验证：EditMode f5f5c19a90444a5ebda8b7603ee22974终态PASS 2/2，两profile各2100 before/after/finalize全部0差异（direct-first-pass/）。接着同Record准确Shadow writer改独立预测与临时Native同步随机capture/compare，删除旧legacy/frame-range guard，测试扩至四组；尚待新编译/测试。新字段仅WriterEffectSnapshot瞬时观察，不改变persistent schema。

Shadow首轮4组：direct2组PASS，Shadow2组FAIL各1470差异/48例，仅type3/4攻击者post样本，ObservedWriter=0且NativePrelude=0；测试误用Character-only PostInteractionTickAll。旧失败shadow-entry-red/保留；测试按SupportsPostInteractionPhase选择原Character/Object观察pass，包围同一冻结candidate消费，不改生产调度或跳过断言。新增同测试文件Play request adapter，运行两factory×direct/Shadow×2100及池回收/Scene checksum守卫。

FOCUSED_TEST_PASS：job fe21d27b4a164f7ea9ca115d2b4c8964，6/6 PASS。2100×两profile×direct/Shadow=8400，全before/after/finalize差异0，观察次数每例1；两profile各4个state1002武器case本地恢复，16个replayed ticks checksum/rest/impulse/实际native状态一致。final-focused/保存XML与四原始报告。父回归、自检、Play尚待运行，不标整体VERIFIED。

复核hit_response.h:32-33：state2000左右比较上下文是int X。生产和Shadow该比较应使用现有整数位置，不用double亚像素；将补同整数不同亚像素的针对性测试，再修正准确两行。

整数坐标focused两组RED：同XInt100的100.1/100.9原错误+5，源上下文应-5。已改生产/Shadow准确两行读取现有XInt；无新持久字段。原RED见integer-position-red-and-oracles.xml，后续重跑含Play完整2100。

最终合并EditMode d8cf1026806f47f5a0555ecdb63f9e7a：48/48 PASS（原34+本文件14：四矩阵、两回放、两整数坐标、六oracle入口）。最终生产8400对照0差异，原失败保留；XML final-tests-48-pass.xml。此后只有两个测试消息文案拼写修订，无断言或生产变化。完整SelfCheck请求后等待终态，尚未Play。

VERIFIED / DECLARED_WEAPON_TRANSACTION_SCOPE：最终48/48，完整SelfCheck10:33:50Z PASS，真实NTSD_Battle Play10:36:58Z两factory×direct/Shadow×2100=8400 PASS，before/after0差异、Scene checksum保持、Renderer2→2；10:37:31Z有序关闭PASS（原位restore4→4，World/slots/两pool全0，连续两帧Stopped）。Editor已退出Play，Scene dirtyfalse/root14/hash bcd1047b…保持，Console error0，生产/正式EXE hash保持。详见artifact REPORT.md。此状态只关闭已声明武器事务，父Q06/full alignment、reduced108、type5覆盖、正式资源及其它reader/表现依赖仍未关闭。
