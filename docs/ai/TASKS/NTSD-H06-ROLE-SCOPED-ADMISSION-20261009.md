# H06 现有 Role 限定生产准入

状态：BLOCKED / GC_GATE_FAIL / NO_DEFAULT_PROMOTION。授权：2026-10-09 当前用户具名任务，D1=A；不恢复旧 Goal，不继续其它34项。正式336B44、D023及用户例外保持。

## 当前事实及准确范围

当前真实默认链：GameConfig.BattleCollisionBroadphaseName为空 → BattleRuntimeProfileProductionSource.Resolve → CollisionBroadphaseBackendResolver.Resolve空值BruteForce → SimulationTickDriver创建SimulationWorld → BruteForceSceneQuery.ResolveFormalCollectorMode。显式LooseQuadtree对应现有Role自适应Direct/Sweep/tree；ForceBrute仍可诊断回退。95及未准入候选保持OFF。

唯一首阶段脚本写域：Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs，新增H06具名资格入口，复用WithRoleDriverScenario的当前正式Sakura数据、Manual Driver、配置、封存、logic pool和十一阶段关闭。已有入口不放宽，不修改原Raw/Q06。
条件生产写域：Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeProfile.cs::CollisionBroadphaseBackendResolver.Resolve，仅必要门全过后修改空值默认，不改变有效Brute/Role显式配置优先级。
条件实际窗口接线：Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs，仅需时增加具名冷入口，复用已有suite/observer；不得为了获得PASS改变标准。
这些脚本之外仅当前Task/Change/Operation、新artifacts及before-manifest所列治理文档。Scene/Prefab/资源/ProjectSettings/InputActions/Server/Gen/Plugins排除；活跃Q06方法体不读。

## 现有证据与缺口

- 25：四种1000-participant局部collector候选/handle/itr/RNG一致，OPointBurst只是形状；无实际OPoint/完整Driver，GC计数正例失败，不能复用为0GC。
- 81：同版同配置四个120+180真实窗口，两布局Role有收益；末tick完整文件同，可靠full Driver steady0GC及有序关闭。不是逐tick四场景证明；Role P95约50/53ms，H07仍FAIL。
- 82：两正式root各12与D/C各32逐tick声明hash/双RNG/容量同，实际候选路径有证；缺Concentrated及实际OPoint生命周期、逐tick候选序列和明确输入捕获。
- 86/91/93：按当前代码/配置指纹审查后复用未受影响的实景GC/表现/关闭。95 Manual/headless分配失败单独保留，不覆盖91/93。

## 最小必要实验与决定

1. 当前四场景32tick完整Driver配对，复用82正式Sakura fixture；D/C复用布局，Concentrated明确1000近距角色，OPoint场景在固定tick从已有逻辑OPoint队列生成子实体。逐tick比较输入、全声明checksum、双RNG、候选顺序/handle/载荷、实体数；不将受控spawn称为正式技能/EXE千人trace。
2. 候选捕获使用现有测试hook只读观察正在构建的store，单独标明有hook资格，不作为热路径GC或收益样本。必须保存raw再断言，出现首差先定位；不切默认。
3. 容量引用/容器容量、fallback和关闭三零；既有未受影响fallback/slot reuse聚焦证据指纹可复用，否则只补受影响case。无新runtime缓存/owner；Editor诊断数组仅测试持有，finally解绑hook，在原owner关闭第5—9阶段前停止观察，退出释放，不新增关闭阶段。
4. 正确性通过后无hook实际接线的可靠完整Driver0GC及同配置收益比较，复用已有observer前后正负校准。先短窗判别，不重复同版长窗找PASS；正式H07仍须原120warm+180冒烟/120warm+1800正式门。
5. 必要门全满足才改空值默认并验证实际resolver/World/query路径和显式Brute回退。未满足保持默认，给出真实阻塞；不能仅因整tick未33ms阻止子阶段。

风险：fixture不能代表全部规则；hook会阻断zero-ITR fast path，必须与无hook成本/GC分开；集中真实重叠不能靠漏pair达标。当前dirty保护与逐文件备份见Operation。
回滚：只本次增量，保留所有既有dirty；不reset/checkout/clean/stash/delete/push，不自动恢复覆盖。所需恢复须独立审计授权。
交付：只允许“Role限定准入完成”，不关闭H06/H07父门或Android。后继建议H07-INPUT只整理方案/依赖，不实施。

## 受影响0GC门的事前细化

同一Admission测试脚本拟新增H06RoleAdmission_FourShapeCollectorZeroGc：复用25四形状构造（OPointBurst-shaped仍只是混合形状），Configured Role、1000参与者、4次预热/8次样本、无候选hook；复用BattleScopedGcAllocationRecorder前后1MiB正例/空例，分别报告event和原始unit，不将TimeNanoseconds解释为bytes。封存后观察容量引用/拒绝数、实际路径与fallback；测试finally解封并注销全部参与者，objects/slots为0。它只补25计数API失效的collector GC门，不替代真实OPoint/Driver或实际完整Driver0GC。

正确性与collector门通过后，同一Suite脚本拟新增H06冷窗口入口及request builder：现有四个120warm+180sample的D/C Brute与Configured Role，沿用原完整Driver observer和关闭，无新增生产开关。只新增Editor session bool，由原Suite owner持有并随session清理；不增加Runtime cache。保存新windows-01目录，旧81/93原件不覆盖。适用focused检查请求仅collector/output不同、配置及实际解析路径守卫；完整窗口失败先定位，不重复同版找PASS。

修订记录：qualification02证实984 AI会在tick2起自然生成对象，tick7已999，因此取消错误的固定tick8空位假设，实际受控16-task burst移到tick1前并逐槽断言空位。生成仍走正式队列/Driver，不禁用AI、不改变生产。该修订只影响OPoint case；D/C/Concentrated已PASS不重跑。

收尾结果：四场景完整Driver逐tick一致性通过，D/C/Concentrated collector GC通过；混合OPoint形状234 events/sample稳定失败，已定位collect而非snapshot/end，Brute亦非0且256容量饱和。调用栈受既有Profiler单帧>2GB导出失败阻塞。未进入收益窗口/默认接线；必要门不豁免，当前生产Brute不改。完整事实和后续依赖见REPORT。
