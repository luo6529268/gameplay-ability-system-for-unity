# Q05 GrabbedBy/TrackerFlag载体退休限定交付

状态 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING。五类退休字段已完成其中两类；ReleaseTick、WeaponState、HolderCopy仍待，Q05与总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

## 实际改动

准确11脚本，6生产与5测试/含既有probe。删除NTSDEntityRuntime两个flag及copy/reset；LF2Entity包装和自赋值同步；Character/Weapon/Other初始化；ECS link arrays、capture/clear/compare和RuntimeFingerprint项。两flag原本没有独立lockstep checksum/parity项，本包没有虚构或扩大那些删除。

真正LinkState/Target/Holder/Caught/Catcher、TrackerParent及其snapshot handle、Owner/Spawner/独立2F8保持。新测试已实测TrackerParent=已注册parent、Owner3/Spawner27/2F831分别捕获和恢复，不把无行为flag删除混同真实关系删除。

已有关系/工厂/拾取测试去掉旧flag sentinel，保留原实际建链/释放/替换/inactive反向字段等断言；Goal20 probe报告carrier absent并保留其余三reserved默认检查。字段不存在由反射检查实际得出，诊断输出不伪造flag零值。历史artifact不改。

## 验证

- 精确前置：NTSD28-B6-LEGACY-GRABBEDBY-NONZERO-PRODUCER-RETIREMENT-PRODUCTION-001及NTSD28-B6-LEGACY-TRACKER-PRODUCER-RETIREMENT-PRODUCTION-001已有VERIFIED行为退休；本包只关闭其留给Q05的存储部分。
- RED7：6FAIL/1PASS，runtime/Entity/ECS六成员仍存在；真实TrackerParent与独立owner snapshot往返原已通过。最终jobf228e2d711f942fd9def10fed9f15a65，407/407 PASS，包含两flag/真实parent、Goal20工厂关系、G16 pickup、lifecycle/negative guard、snapshot/restore/2F8/raw/ECS/hit-plan等相关回归。
- 完整SelfCheck请求2026-09-13T09:42:47.6906533Z，09:43:33Z PASS，mtime晚于请求。
- 实际Play通过既有Goal20_R12 request机制，09:45:37Z新结果PASS，tick5→9、beforeObjects4/afterObjects4。Gaara16/60→OID120/64 type1 pickup、Kakuzu25/250→OID150/20 type2 replacement两个实际driver/collector/held witness PASS；当前OID51 action279中的kind2→213 post-init、完整tick及unregister关系检查通过。OPoint childAction0被保留。为注入动作/输入集合的定向证据，不是物理键盘或完整自然技能发射验收。
- 嵌入g16Witness报告只由RunWitness填充子witness，外层status/message/cleanup计数未被该路径填充；不把这些默认字段当作完整G16 outer-run结论。采用实际子witness、root状态/对象计数和OPoint断言；play-summary.json明确此边界。
- 编辑器自动退出Play，最终Scene isDirty=false/root14，SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f不变。实际Unity编译/重载/测试通过，最终error CS0。
- 保护3059：2983相同/58已存在或声明差异/18既有Foot缺失；本轮新增5个基线差异路径均在Record内，无新缺失。Foot和Scene既有用户工作保持。
- 账本校验通过492 Records/130 governed code files，最终结果回填ledger-final。无新manager/queue/worker，十一阶段关闭顺序不变。

## 后续

下一唯一Task NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001，随后WeaponState、HolderCopy。不得重复两flag、Mass/Oscillate或2F8/raw修复。当前12/20/23/1/1为同Q05未发布中间状态，步骤3 identity/双OPoint guard、步骤4统一13/21/24/2/2及步骤5旧版本拒绝/新回放/Play仍需完成，不能提前发布baseline或部署Q07。

R13只对两flag存储删除子条件追加PARTIAL_RETURN，整体联合schema和其他reserved未关；R15等待对应identity/version证据。保持Unity/GAS、非战斗、33ms/3ms、stage.dat USER_HOLD及用户例外。禁止computer-use，本轮仅桥接/日志/结果/进程。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，492 Records / 130 governed code files，ledger-final.txt保存。
