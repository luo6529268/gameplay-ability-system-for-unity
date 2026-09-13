# Q05 HolderCopy完整载体清理限定交付

FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING。Q05步骤2载体清理限定出口；Q05/总目标仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

## 改动

准确36脚本：13生产、22旧测试/probe、1新测试。移除runtime HolderCopySlotIndex、Entity包装、Character/Weapon/Other默认、OPoint task字段/default/reset及四个配置文件中的五处task赋值，ECS数组/capture/clear/compare/hash，checksum/parity与HitPlan WriterEffectSnapshot诊断字段。bit33留退休空洞，未复用或重排32/34/35；真实holder/target/owner/spawner/2F8/TrackerParent保持。两条factory此前已不读该任务字段，本包没有重写factory或行为。

13生产文件按预变更内容逐项核对，差量仅列出的旧载体/包围旧默认赋值的空if/诊断删除，production-scope-stability.json保存。其余真实行为语句保持，当前包及前两包累计45脚本与全部准确Record路径一致。没有Scene/资源/非战斗/Unity-GAS/外部包修改。

旧sentinel/默认值断言改成员不存在或保留真实关系检查；Kind5无效copy参数删除、3种实际parent情况保留；六held用例改验独立Owner不被持有绑定覆盖。旧G16 pair和death evidence不再输出虚构holderCopy默认。SelfCheck原Team/RelationTeam逐tick验证保持，去掉只服务旧sentinel的HashSet/if。其余旧probe已迁移/编译，不冒称每个都已执行。

## 验证与失败保留

- 新RED jobba6dff0173ca414e83bfae380bcebd1d：11=7FAIL/4PASS。字段/诊断/key存在；实际差异mask32/34/35和任务Clear已通过。red-results.xml保存。
- 中间迁移匹配遇到多行赋值时先在内存停止未落盘；随后迁移误匹配==引起3个SelfCheck语法错误，经preimages精确重算修复SelfCheck和三probe、保留有效复合断言。另删Character空if，恢复Naruto三组逐tick关系检查；最终编译成功。migration-assertion-correction.json及Record保留过程。
- 第一轮36测试类job bce718a6fecf4560a99263d4e38b0ac8：863=859PASS/4FAIL，包含B5/B6/HitPlan/OPoint/held/快照restore/raw/ECS/checksum等，完整选择test-selection.json，原XML first-results-859-PASS-4-FAIL.xml。
- 四FAIL为两份旧B5统计测试仍要求Goal20已退休holder stats；独立NTSD28-Q05-RETIRED-HOLDER-STATS-FIXTURE-CORRECTION-001只改测试。两类job0973d964fde14077896c7b1dda8d428d为27=26PASS/1FAIL，最后1FAIL发现旧Cpoint参数名误导、实际应保留native KnockoutCount358=1；纠正参数名并保留原预期后job8a46cf58cfde409b817d78d19af0e63e最窄1PASS。父focused-reconciliation.json逐项映射原四失败；863不同用例均有通过证据，不宣称新鲜单次863全绿，未重跑整批。生产BattleDamageWriter/Cpoint规则未修改。
- 完整BattleRuntimeSelfCheck新请求2026-09-13T10:34:09.142394Z，结果10:34:58Z PASS且mtime晚于请求；历史结果另存NOT-CURRENT。
- 真实NTSD_Battle两次Goal20_R12 request，改后新结果10:36:13Z PASS，driver tick5→9、beforeObjects4/afterObjects4。Gaara16/action60→OID120/action64 type1实际pickup，Kakuzu25/action250→OID150/action20 type2 replacement；current OPoint OID51/action279 kind2→213 childaction0、post-init/fulltick/unregister通过。两组G16 populated witness去掉旧holderCopy key后前后全部有效字段一致，play-comparison.json无差异。
- 嵌入G16 outer status/cleanup/counters由该调用路径未填充，不能作为独立G16 full-run证据；采用实际witness/root/OPoint断言。测试为明确注入输入/动作与实体setup，不是物理键盘/自然完整技能/全native-world parity。
- 最终Unity编译重载成功，error CS查询0，Editor已退出Play，Scene dirtyfalse/root14，磁盘SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f保持。完整SelfCheck会有预期故障注入日志，不声称Console全无error。
- 保护3059：2954同/87既有或声明差异/18既有Foot缺失，较前包新增22基线差量全在Record，无新增缺失。用户新图/旧Scene差异和未提交工作保留。
- 实际工具均为现有Editor桥接refresh/run_tests/get_test_job、请求文件、manage_editor/play/scene、日志，禁止computer-use；未开第二Editor。git diff --check PASS，Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path PASS：496 Records/45累计差量脚本；receipt ledger-final.txt。没有提交/推送/资源部署。

## 后继

下一NTSD28-Q05-CONTENT-IDENTITY-AND-CAPTURE-BOUNDARY-001，即父Q05步骤3：raw/semantic identity、native candidate/cache/publication与本地验证session binding，以及双OPoint owner空队列capture/restore guard。随后步骤4一次统一entity13/aggregate21/checksum24/character2/base2及trace/binding50字段，步骤5旧版本拒绝/新snapshot回放/同seed输入checksum/pool与slot复用/零残留/Play。

五类reserved载体以及Mass/Oscillate/2F8已限定验证，不重复实施；当前12/20/23/1/1仍未发布中间态，R13只有载体删除子条件PARTIAL_RETURN，完整R13/R15/Q05未关闭，Q07正式DAT/角色图迁移未执行。真实2F8 producer/AI消费在Q06。保持Unity/GAS/非战斗、33ms/3ms、十一阶段、stage.dat USER_HOLD和全部例外，禁止computer-use。
