# Q05 双 OPoint / 完整执行入口快照边界出口

状态 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING。准确11脚本（10生产+1测试/probe），另1个worker测试夹具独立Change。总体Q05及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE；本结果仅关闭父步骤3的boundary子条件。

World聚合capture/restore、driver/session/ring现在在完整tick、structural mutation、worker flight/awaiting ack、停止态或本World任一OPoint队列非空时拒绝。拒绝不消费队列、不创建服务、不停止worker、不改变原World或待恢复snapshot；capture目标明确失效。logic队列按固定World owner，renderer按显式target→registered parent→既有host fallback；优先driver已捕获factory。observer在既有World绑定/关闭阶段9解除，不新增manager，不改变十一阶段关闭顺序。只支持既有owner线程入口，不承诺任意并发World API。

完整范围补入Host两种StepOneTickInternal、worker Execute及现有InProcessBattleKernelHost.TryStepOneTick，使输入准备/回调及结果发布也处于边界。core tick、六structural方法和四Host/worker/kernel方法原body token连续保持；见两份body-preservation.json。修改只有平衡try/finally边界元数据，不改变规则/pass/输入/协议，未更改schema。

实际验证：
- 原14 RED全部失败；初18 GREEN通过。扩展19及相关175首次174PASS/1FAIL，worker旧Y45夹具经独立Change保留原parentY40、补parent落地0、child改5；正式physics_integrator type0接触钳制与当前OPoint公式支持，生产运动未改。
- 新Host输入反例RED1：GetFrameInput/BeforeSimTick/AfterSimTick三次都错误capture成功（expected0/actual3）；完整入口scope后通过。
- Unity桥接run_tests最终187/187 PASS，包含20个本包边界例及snapshot/restore/ring/worker/ordered shutdown/OPoint/structural/raw snapshot、InProcess authority、FormalKernelFullReturn。准确XML/job已归档，不以发现数量代替执行数。
- 完整BattleRuntimeSelfCheck：请求11:58:10.2121605Z，结果11:58:48Z之后文件PASS；实际精确mtime见SelfCheck-result-utc.txt。SelfCheck运行时一次Console桥接30s超时，之后读取成功；无重启/第二Editor。最终error CS查询0条，真实编译与测试已执行。
- 真实NTSD_Battle Play：tick5，对象4→4；已捕获renderer owner；一条probe-owned logic task和一条renderer multi task分别触发拒绝，任务reference/count保持；driver拒绝不改变worker引用；probe仅在断言后移除自己注入的任务，空闲capture成功；自动退出Play。workerWasPresent=false，因此worker运行中证据来自focused，不能宣称真实worker Play或物理技能键/全技能通过。
- Scene isDirty=false/root14，SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f保持旧基线。3059保护项2945同/96既有或声明变化/18旧缺失，无新增缺失，相比semantic包新增9项均在本包或独立fixture声明内。

命令通过现有Temp/Goal13_bridge.py（Python -X utf8）执行refresh_unity/run_tests/get_test_job/read_console/manage_editor/manage_scene；SelfCheck用既有request/result机制。精确测试选择见test-selection.json（原14类）及final-job-start对应新增InProcessLockstepAuthoritySessionEditorTests/FormalKernelFullReturnCommitSeamEditorTests；最终XML187记录为准。未使用computer-use，未改Scene/资源/非战斗/GAS/Gen/Plugins/外部Server，未删除旧素材、提交或推送。

后继：先只读核对新增FrameSounds/profile/centerz/chp/cmp/六double及BMP/stats/armor/piece在所有实际内容hash/版本消费者的覆盖，区分源身份、值hash、运行时checksum、缓存guard和历史诊断，不凭raw+tag身份关闭其他消费者。再按Q03同窗口完成entity13/aggregate21/checksum24/character2/base2及trace v3/raw v2/source wrapper v2/50字段+2F8；最后旧版本拒绝、新capture→restore→同seed/input replay及后续Play。当前12/20/23/1/1仍INTERMEDIATE_UNPUBLISHED，禁止发布或跳Q07。

最终审计：Validate-ChangeLedger.ps1 PASS，500 Records/当前5脚本diff覆盖；其他本包脚本已在用户外部创建的HEAD f3e11239，不能把5解释为全部包scope。精确11脚本/11个原body保持检查与187结果见validation-summary.json、final-test-selection.json。
