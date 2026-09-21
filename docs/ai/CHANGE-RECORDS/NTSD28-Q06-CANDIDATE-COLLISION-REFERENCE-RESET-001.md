<!-- CHANGE-RECORD
id: NTSD28-Q06-CANDIDATE-COLLISION-REFERENCE-RESET-001
status: VERIFIED
change-kind: CANDIDATE_PASS_COLLISION_REFERENCE_RESET
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CandidateCollisionReferenceResetEditorTests.cs
authority: Formal playable BattleWorld28 candidate prelude battle_world.cpp4026 and ordinary landing source186 following tick.
evidence: Parent post-binding four matrices each before0/immediate0/following78; focused RED pending.
-->

# NTSD28-Q06-CANDIDATE-COLLISION-REFERENCE-RESET-001

IN_PROGRESS / TEST_FIRST。总目标Q06授权；父普通落地186四组following78，全部CollisionYReference=-10 expected0，旧RED存父after-binding-fix。当前正式playable BattleWorld28 candidate pass在battle_world.cpp4026无条件清每个active slot的collision_y_reference；发生于physics之后、任何unordered pair及op30回填之前。源另外platform_source_slot_f4/render_shadow_offset_10c同步清零，当前Unity无已确认等价carrier，留明确平台联合合同，不机械映射Zz。

准确范围：Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs，仅CollectCollisionCandidates入口，在任何fast path/role roster过滤前遍历现有ActiveEntitiesByRuntimeSlotForModule，清CollisionYReference。Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CandidateCollisionReferenceResetEditorTests.cs新增focused测试。源码186既有见证复用，不编辑source或修改其expected。

前置/不变量：核查active traversal及直接query/world两个入口，无pair、无ITR、缺descriptor也必须清零；physics不得提前清，candidate以外查询不改变值；不写对象身份、动作、速度、碰撞几何、RNG、schema、关系、停止顺序。已有runtime字段不新增manager/queue/lifecycle模块。类型0与非角色均覆盖，正负零、重复收集、slot孔洞验证；平台op30完整producer仍未实现不得宣称已对齐。

验收：先focused RED，然后source186×四矩阵before/即时/完整tick及focused复测，compile/SelfCheck/回放/Play与关闭、独立review。任何回填caller未确认则先只读，不用测试清零伪造。回滚仅同ID精确diff、遵守用户批准规则并保留其他工作与失败证据。Q06未完成/Q07未迁移。

实际RED job24e9bd675b0746b1bf9c8f4751de8742终态4FAIL，均-37 expected0；XML已归档production-red/results.xml。独立只读复核确认active slot遍历不依赖ITR、HP及descriptor，排除pendingUnregister/dormant/PendingFlushDestroy；未发现现有op30 producer。已在声明collector入口加单字段reset及focused额外active lifecycle/inactive/空World/fastpath开关；初版测试命名空间拼写导致编译失败，已改NTSD.Animation并重编译后成功执行RED。当前新改动编译/复验待执行；平台整体保持未实现边界。

首轮联合job5605f4e820d646c484fbefb0dd2512c1共15，4失败为新fixture消费期内改fastpath开关（既有保护正常），其余包括源186×四组全部PASS，before/即时/following均0。失败XML保留first-post-fix/results.xml，测试在两轮间补world.EndCollisionCandidateConsumption，不动生产保护。独立只读review确认单字段4行位置与无分配active遍历正确、Runtime初始化与清理合理；无nullable新风险，平台域不得闭合。最新联合job0bbead38ad244ed1adc8ac109ebdabd3运行，待终态。

Q06候选高度参考reset独立包已生产写入；联合job0bbead38ad244ed1adc8ac109ebdabd3 SUCCEEDED15/15（新5/载体6/源矩阵4），普通落地186×两profile×两路径before/即时/following全部0。实际未运行旧B4 fixture（初始filter漏Editor namespace），后继须NTSD.Test.Editor.NTSD28B4Type0OrdinaryLandingEditorTests补跑。完整SelfCheck请求已消费，现有Editor PID62860执行中，实际日志Logs/kind8-real-play-editor.log已有RunAllChecksStatic/CheckActivatedRuntimeProfileContracts调用；结果待新Temp/NTSD_BattleRuntimeSelfCheck.result，禁止重复启动或测试并行。之后同World回放/真实Play及关闭仍待。平台op30/previousXYZ/其他两个reference字段仍未闭合，不称整域完成。Q06 active/Q07未迁移/禁computer-use/非战斗/Scene/资源修改。

最新验收：candidate reference reset与普通落地native binder联合15/15PASS，source186×四组before/即时/following均0；旧B4普通落地已以正确Editor namespace补跑6/6PASS（job4191c162de2543eb83166574b9983c84）。完整SelfCheck 2026-09-21T00:11:47.663892Z PASS文件与时间证据已存candidate包。CS0、Ledger597/11PASS、diffcheck无错误，Scene hashBCD1047B…0E9FB6保持。下一唯一工作：同一普通落地186夹具补同World回放，再真实Play两profile/两路径/两factory及Q05关闭重入；脚本扩展前登记Record准确符号。两个包仍IN_PROGRESS（尚未完成replay/Play），不是平台op30域对齐。当前无运行test/SelfCheck/build/agent/Play；复用Editor62860，禁computer-use/非战斗/Scene/资源修改，Q06未完/Q07未迁移/总目标ACTIVE。

最终限定VERIFIED：详见artifacts/diagnostics/NTSD28-Q06-ORDINARY-LANDING-NATIVE-FRAME-BINDING-001/ACCEPTANCE.md。源186矩阵0差异/联合15+旧6/回放744场景1488tick/完整SelfCheck/Play1488/真实退出重进两次关闭PASS，Scene保持。平台op30、raw3、previousXYZ及Q06后继明确保留。
