<!-- CHANGE-RECORD
id: NTSD28-Q06-COLLISION-FIXTURE-DEFINITION-IDENTITY-001
status: VERIFIED
change-kind: COLLISION_SELFCHECK_FIXTURE_DEFINITION_IDENTITY
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Current source definition.frame(snapshot) reads actual definition content; collision source336 validates definition identity, not manually substituted Unity frame pointers.
evidence: Fresh SelfCheck05:43:27Z fails CheckLooseQuadtreeShadowBroadphaseContracts at1805: invalidFirst is created with a body definition but only its Frame pointers are replaced by empty data.
-->

# 碰撞自检夹具定义身份

IN_PROGRESS / TEST_ONLY。仅BattleRuntimeSelfCheck.CheckLooseQuadtreeShadowBroadphaseContracts中的invalidFirst夹具：开始就使用无bdy/itr的真实定义，而不是初始化带body定义之后只替换Frame.D/Prev2D。原source直接读definition，所以当前fixture实际有body；应修正测试输入，不恢复旧缓存指针语义。

保留invalid AABB fallback顺序和唯一目标断言，保留旧FAIL结果。不改production、不放宽断言、不修改共享SetCollisionAuditFramePair为任意同id不同descriptor伪造原定义。若后续存在其他旧夹具问题，必须记录具体调用和内容身份再按本同职责范围追加，不批量改测试预期。

验收是重新实际完整SelfCheck、必要对应focused，源向量保持；无runtime/schema/关闭副作用。回滚只本夹具差量并按规则获批。用户Scene/非战斗/资源/框架不改；禁止computer-use。
已写：invalidImmediateFrame声明前移，invalidFirst的BuildCollisionAuditData直接使用该空frame；其它代码和唯一目标断言未改。等待compile及完整SelfCheck。

## 第二处旧合同纠正（修改前）

重跑05:46:32Z已越过invalid AABB断言，但CheckAuthoredFrameGates旧DATA-01C明确要求Prev2缺失回退current，与原336见证冲突。追加同SelfCheck函数的collision断言更新为Native implicit/真正缺失区分、不回退current；保留legacy HasFrame/GetFrameDataById本身的旧API测试及其它动作准入断言。第二FAIL已保存，不改生产来迎合旧测试。
已改第二处：450使用Native隐式零帧且不等于current原frame；-1/未声明999/1000即使Prev2D是有效原frame仍null。保留ImmediateFrame/CPoint旧API准入检查及隐式无itr零candidate断言。

## 最终限定出口

VERIFIED / SELFCHECK_FIXTURE_AND_COLLISION_ASSERTION_ONLY。实际同文件两处：invalidFirst真实空定义、CheckAuthoredFrameGates的Native collision oracle；原两FAIL保留。第三完整SelfCheck05:48:49Z PASS，后续Play168/有序关闭PASS、最终CS0/Scene dirtyfalse哈希保持。完整证据在COLLISION-FRAME-UNITY-001 artifact。父collision仍IN_PROGRESS因96候选首差，不能由本自检修订关闭生产对齐。
