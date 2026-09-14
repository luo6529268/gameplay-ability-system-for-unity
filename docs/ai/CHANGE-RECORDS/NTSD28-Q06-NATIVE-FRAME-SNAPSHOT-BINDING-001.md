<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-FRAME-SNAPSHOT-BINDING-001
status: VERIFIED
change-kind: NATIVE_FRAME_SNAPSHOT_BINDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeFrameSnapshotEditorTests.cs
authority: Formal DatDocument.frame and validated native accessor; existing Q05 exact snapshot/restore contract preserving frame identifiers and checksum.
evidence: HasSnapshotFrameData local uses legacy accessor/nonlocal requires declared list; TryRestoreBaseShell likewise legacy; zero frame source63 validated
-->

# 快照Native帧绑定

准确三脚本。BattleStateSnapshotRestore.HasSnapshotFrameData：localEntity使用FrameCache.GetNativeFrameDataById；无local时从目标catalog data校验，0..998隐式合法，999需有声明，>=1000拒绝；负FrameDataId保留已有null sentinel语义。LF2Entity.TryRestoreBaseShellForSnapshot只把current/collision两个descriptor绑定改Native getter；FrameNumber/Prev/Prev2/Trans wait/latch等全部按snapshot原值恢复，不补写或纠正旧snapshot中的null descriptor，不改虚方法GetFrameDataById全局语义。

前置已验Native lookup有声明999/隐式0..998；快照capture本就记录Frame.D.frameId/Prev2D.frameId。本包只关闭恢复拒绝/绑定错误，不变字段布局或版本13/21/24/2/2，不改content hash、snapshot queue、pool lifecycle或其他frame执行。无新manager/cache/queue/关闭阶段。

新Editor测试先RED覆盖低/高隐式和声明999、两个profile、原地和清除local-reference后新world shell重建；完整checksum、descriptor id/wait、frame/latch/collision身份；无声明999/1000拒绝且无副作用，null descriptor保留。相关旧snapshot/回放/Native API/出生/资源和完整SelfCheck；真实Scene高位current/collision descriptor恢复再原始World恢复/关闭全0，不运行高位完整tick（帧/lifecycle尚未成组迁移）。

回滚须批准仅本三脚本差量，用户工作保护；禁止computer-use、非战斗/框架/Scene/资源/Gen/Plugins/外部Server更改。父NATIVE-FRAME-RUNTIME-READER-MIGRATION继续，next frame绑定/step+terminal需成组：source数据可读声明999，但battle_world.cpp post-cost将action>=999判终止；不要一律把旧857替换1000。原frame_machine negative/999等顺序另需原World见证，不借本snapshot包改动。

实施追加：20项RED已运行，16项因Current/CollisionFrameDataUnavailable失败，4项保护通过，red-results.xml留证。两生产文件仅修改上述Native descriptor解析/preflight；当前CODE_WRITTEN，待重新编译和测试。

编译idle且error CS=0。新20/20 PASS，job e7414d3b500d417884817cd508d408a6，focused-20-pass.xml。真实Play请求probe已写入同一声明测试文件，尚待执行。联合回归8a195fe8c1ec496194865944b5a0e985运行中。

联合job 8a195fe8c1ec496194865944b5a0e985 已244/244 PASS，163.219秒，related-244-pass.xml；完整SelfCheck请求18:59:11Z/结果18:59:49Z新鲜PASS。当前进行真实Play descriptor restore，未运行完整高位动作tick。

## 最终限定结论（2026-09-14）

VERIFIED / SNAPSHOT_NATIVE_DESCRIPTOR_BINDING_ONLY。联合244/244 PASS（job8a195fe8c1ec496194865944b5a0e985，163.219秒，related-244-pass.xml）；包含新20及旧snapshot/真实Logan两profile回放、HP/MP/出生/显示和Native/legacy接口检查。完整SelfCheck请求18:59:11Z、结果18:59:49Z新鲜PASS。

真实旧内容NTSD_Battle tick5暂停边界，当前descriptor857/碰撞998和Trans latch857恢复正确；修改后checksum与原始World checksum分别完整恢复，实体4→4（play-snapshot-pass.json）。随后既有Q05真实恢复/Renderer保留与关闭，World/slot/logic borrowers/render borrowers全部0，连续两帧Stopped，正常退出Play（play-cleanup-pass.json，19:00:41Z晚于请求19:00:40Z）。这不证明高位动作完整tick、正式EXE物理输入或新图片一致。

最终Editor idle/非Play/编译错误0，Scene dirty=false/root14，用户HUDBg x30与bcd1047b…哈希保持。没有新增缺失，保护3059路径的2921原始hash未变。正式EXE重新SHA核验B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。

实际命令/入口：Goal13_bridge.py refresh_unity/get_editor_state/read_console/run_tests/get_test_job；SelfCheck请求文件；manage_editor play + NativeFrameSnapshotPlay请求 + 既有Q05 ReplayPlay关闭请求；manage_scene get_active；Tools/Validate-ChangeLedger.ps1。最终账本结果见ledger-final.txt。

本三脚本范围结束；父NATIVE-FRAME-RUNTIME-READER-MIGRATION继续，下一先frame/direct/next/cost/lifecycle完整调用表与原函数见证，再准确Record实施。零帧全reader、其余display出生、post和Q07正式资源仍未完成，总目标ACTIVE。
