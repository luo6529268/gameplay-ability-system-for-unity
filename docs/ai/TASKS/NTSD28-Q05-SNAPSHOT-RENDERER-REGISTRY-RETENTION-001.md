> 当前状态 VERIFIED / SCOPED_EXIT；以下实施期文字保留为历史，最终证据见文末。

# Q05 真实Scene快照恢复中的Renderer注册保留

IN_PROGRESS / TEST_FIRST。父任务 `NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001` 的必要修复；范围、原状、准确三脚本、风险/验收/回滚见同名Change Record。实际World=4、战斗槽=2来源已确认是两个LF2ObjectRenderer的ISimObject注册。不能修改全局ObjectCount语义、清场、把Renderer强转成战斗实体或改非战斗框架。

先运行新focused RED；再实现活动实体与呈现注册分开验证、保留原有双向绑定的Renderer、拒绝未建模对象、局部重建实体项。测试、完整SelfCheck、真实Play恢复/关闭/重入/零残留全部通过后返回父Q05步骤5；前一pool归还修复保留。Q05仍未发布，Q06/Q07不得先行。


## 最终限定出口（2026-09-13）

VERIFIED / SNAPSHOT_REPLAY_SCOPE_ONLY。最终82/82 focused（job6400c6d4e1fd432cbe7fe837231dad20）、完整SelfCheck、两次真实Scene tick5恢复4→4/有序关闭全0/两帧Stopped及重入PASS，Scene旧SHA/root14/dirtyfalse保持，Console0error。完整证据统一在 artifacts/diagnostics/NTSD28-Q05-JOINT-SNAPSHOT-RESTORE-REPLAY-VALIDATION-001/REPORT.md；失败及RED保留，不以旧程序集45项代替最终结果。独立pool归还与Renderer注册保留两个修复按各自Record集成；Q05来源/schema/restore出口满足，BATCH-02限定交付。Q06消费者、Q07正式资源、六MISSING/MP首差、后续视听与整场终验保持未完成。禁止computer-use，总目标ACTIVE。
