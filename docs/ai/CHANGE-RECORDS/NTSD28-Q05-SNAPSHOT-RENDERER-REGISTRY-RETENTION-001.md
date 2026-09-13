<!-- CHANGE-RECORD
id: NTSD28-Q05-SNAPSHOT-RENDERER-REGISTRY-RETENTION-001
status: VERIFIED
change-kind: SNAPSHOT_LOCAL_PRESENTATION_RETENTION
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05SnapshotRendererRegistryEditorTests.cs
authority: Approved Q05 real-Scene restore exit and Unity-native presentation boundary; existing LF2ObjectRenderer registration and SimulationRegistryModule.CountActiveObjects contracts.
evidence: artifacts/diagnostics/NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001/REPORT.md; final-82-results.xml; SelfCheck-final.result; play3-pass.json; play4-reentry-pass.json
-->

# Q05 保留原地恢复中的既有 Renderer 注册

IN_PROGRESS / TEST_FIRST。真实Scene tick5 capture成功而restore拒绝；对象4、claimed2，其他前置值一致。实际调用链 LF2ObjectRenderer.OnEnable→World.Register 把两个 Renderer 作为非LF2Entity的ISimObject注册，CountActiveObjects包含它们；原restore把ObjectCount当ClaimedCount，并且随后Clear整个bucket registry，既不能接受该快照，也不能在简单放宽计数后保留Renderer注册。此修复只适配已有Unity战斗表现挂接，不更改Renderer生命周期框架或非战斗UI。

准确三个脚本：Restore中的前置计数/局部拓扑重建；EntityRuntimeSnapshot内部只读活动实体计数方法（基于已存OidMergeDormant/PendingFlushDestroy，不新增字段或schema）；新的focused tests。保留前一RETired-shell pool修复差量，同文件顺序集成，禁止回退它。

恢复前枚举已有bucket：只允许实际LF2ObjectRenderer且双向LogicObject/Renderer绑定成立、owner被目标local snapshot保留；未知非实体ISimObject、丢失owner或纯值丢弃呈现owner均拒绝，不任意保留未建模模拟状态。校验core.ObjectCount等于目标活动实体数加上述既有Renderer数，claimed独立校验；dormant/待flush实体仍占槽却不计活动数，按既有CountActiveObjects定义处理。拓扑重建只移除实体bucket项，保留已预检Renderer项及实例；禁止生成新Renderer、调用OnAdded、解除/重建任意外部owner。拓扑阶段检查claimed而非尚未恢复runtime的活动数。正常退出仍通过原owner完成十一阶段。

测试先RED：有双向绑定且注册的Renderer原地restore保留实例/注册/完整checksum；多次restore不重复注册，未知辅助对象和未保留owner拒绝无副作用；dormant/待flush目标计数正确。温热循环检查无新增分配，既有pool/版本/恢复回归及真实Scene probe复验；完整SelfCheck/Scene hash/账本必做。

风险为bucket局部删除遗漏、错误接受未建模对象、活动数/claimed混用及多次恢复重复注册；通过上述负向及生命周期断言约束。没有新manager/queue/cache/payload/停止阶段。33ms/3ms、五版本13/21/24/2/2、wire/输入/规则/资源/GAS/Gen/Plugins/外部Server保持。回滚须用户批准，仅逆向本包增量；不能用清场或解除Renderer注册来伪造验收。

## RED及已写

job608a8e1e193d4d9686a7634eb1cfae6e：5项中3FAIL/2PASS。原地Renderer恢复因WorldConfigurationMismatch拒绝；两个dormant/待flush用例在旧误判下通过入口却清掉Renderer，恢复后ObjectCount错误为0而非1。未知对象与断开绑定的负向已有拒绝，保留为回归。

已在上述三脚本加入payload内活动实体只读计数、Renderer双向owner/local shell前置、保留非实体Renderer的bucket局部重建（空bucket归还旧registry pool）、claimed独立校验及恢复后活动ObjectCount后置校验。未新增payload字段/manager/队列，保持前一pool归还改动，待编译/focused/真实Play结果。


## 最终限定出口（2026-09-13）

VERIFIED / SNAPSHOT_REPLAY_SCOPE_ONLY。最终82/82 focused（job6400c6d4e1fd432cbe7fe837231dad20）、完整SelfCheck、两次真实Scene tick5恢复4→4/有序关闭全0/两帧Stopped及重入PASS，Scene旧SHA/root14/dirtyfalse保持，Console0error。完整证据统一在 artifacts/diagnostics/NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001/REPORT.md；失败及RED保留，不以旧程序集45项代替最终结果。独立pool归还与Renderer注册保留两个修复按各自Record集成；Q05来源/schema/restore出口满足，BATCH-02限定交付。Q06消费者、Q07正式资源、六MISSING/MP首差、后续视听与整场终验保持未完成。禁止computer-use，总目标ACTIVE。
