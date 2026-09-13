<!-- CHANGE-RECORD
id: NTSD28-Q05-SNAPSHOT-RETIRED-SHELL-POOL-RETURN-001
status: VERIFIED
change-kind: SNAPSHOT_POOL_OWNERSHIP_FIX
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05SnapshotRetiredShellPoolEditorTests.cs
authority: Approved Q05 replay/lifecycle exit; existing world-owned pool contract and ordered shutdown stage8 postconditions.
evidence: artifacts/diagnostics/NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001/REPORT.md; final-82-results.xml; SelfCheck-final.result; play3-pass.json; play4-reentry-pass.json
-->

# Snapshot退休shell归还所属logic pool

IN_PROGRESS / TEST_FIRST。父真实Logan slot/pool验证两profile均恢复状态成功，但poolActive=4而恢复只有3个实体；关闭后残留。调用链确认RestoreSnapshotTopology将旧实体解绑并清表，随后纯值snapshot materialize新shell，却没有归还不再被目标snapshot保留的旧pool borrower。World shutdown只释放当前注册表实体，已脱离表的旧borrower无法被阶段8回收。不是允许清空pool来让测试变绿。

准确两个脚本。修改RestoreSnapshotTopology：旧实体解绑/slot-1后，对目标snapshot所有local shell引用都不再保留的rendererless旧实体调用同一world.LogicReferencePool.Release，保留的实体不回收；采用当前slot快速检查并必要时扫描其他snapshot槽，防止保留实体移动槽被误回收，不新增manager/queue或warm分配。pool.Get/GetSnapshotShell仍负责既有Reset，不在表未清时额外调用可能Unregister的Reset。若不保留的实体仍有Renderer/ShadowRenderer，应在现有恢复前置阶段拒绝，禁止越过呈现owner直接回收，复用EntityShellMismatch，不变更11阶段次序。

新focused覆盖纯值替换、保留旧shell+额外未来borrower、恢复前拒绝仍有Renderer的丢弃路径、World关闭pool0。父用例是既有真实失败证据；新测试先跑RED。保留slot generation历史恢复合同、payload/schema13/21/24/2/2和source identity，不改战斗规则、Renderer生命周期架构、Scene/资源/非战斗/外部Server。临时引用不跨恢复调用保存，结束无新关闭职责；回滚须批准仅此差量。

## RED与已写

独立RED3全部失败：local snapshot未来borrower active2而应1、pure-value替换active3而应1、带Renderer丢弃恢复被接受。现增加已有恢复前置中的呈现绑定检查、RetainsLocalShell同槽快速路径/跨槽引用检查，以及解绑后对真正退休对象调用world.LogicReferencePool.Release；无新字段/队列/分配，保留池在Get时Reset的合同。当前重跑focused与真实Logan验证，尚未关闭父Q05。


## 最终限定出口（2026-09-13）

VERIFIED / SNAPSHOT_REPLAY_SCOPE_ONLY。最终82/82 focused（job6400c6d4e1fd432cbe7fe837231dad20）、完整SelfCheck、两次真实Scene tick5恢复4→4/有序关闭全0/两帧Stopped及重入PASS，Scene旧SHA/root14/dirtyfalse保持，Console0error。完整证据统一在 artifacts/diagnostics/NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001/REPORT.md；失败及RED保留，不以旧程序集45项代替最终结果。独立pool归还与Renderer注册保留两个修复按各自Record集成；Q05来源/schema/restore出口满足，BATCH-02限定交付。Q06消费者、Q07正式资源、六MISSING/MP首差、后续视听与整场终验保持未完成。禁止computer-use，总目标ACTIVE。
