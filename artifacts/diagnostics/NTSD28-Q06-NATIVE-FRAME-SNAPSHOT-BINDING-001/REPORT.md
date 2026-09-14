# Native快照帧绑定验证

当前FOCUSED_TEST_PASS；真实运行时与联合回归仍待，本状态由末尾追加的最终结论更新。

## 范围与依据

正式Logan DatDocument::frame 支持隐式0..998零帧与已声明999。原函数63个合法文档查询/资源向量及18个错误AST诊断见前置NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION报告；该证据为源码模型诊断，不是本包新跑的正式EXE输入验收。Unity前置Native getter已经验证，旧Get/Has/Max857保留。

本包只迁移快照current/collision descriptor校验和重绑。LF2Entity.TryRestoreBaseShellForSnapshot两次查询改Native；BattleStateSnapshotRestore.HasSnapshotFrameData原地使用Native，跨World使用目标catalog，0..998在有definition时可读，999必须声明，>=1000拒绝。负FrameDataId仍按原null sentinel恢复；没有从runtime frame编号擅自补写null descriptor。

字段形状和13/21/24/2/2版本不变，完整checksum/转场身份由测试证明。没有改全局GetFrameDataById、正常frame推进、input/motion/hit、资源部署、Scene、Unity/GAS框架或非战斗逻辑。没有新增runtime manager/queue/cache/关闭阶段。真实Play probe只在暂停的合法snapshot边界执行，最后恢复原始完整World；不把注入857/998之后恢复成功当作高位动作完整tick通过。

## 证据

- RED job cc01a3a9165841629b78de10f298a2a9：20项中16项Current/CollisionFrameDataUnavailable失败，4项保护通过，red-results.xml。
- 编译结束idle、error CS=0后，focused job e7414d3b500d417884817cd508d408a6：20/20 PASS，16.046秒。两profile×local/transfer×7/857/998/声明999；完整checksum、frame/collision id、Trans latch、对象重建；无声明999/1000拒绝无副作用；null descriptor保留。
- 两生产文件相对本包preimage的独立diff以及3脚本sha已归档。不是把整个工作区既有差量都算为本包。
- 保护清单3059路径中2921保持原始hash，较上一Accessor包无新增变化路径或缺失。原有18缺失继续保留；用户HUDBg x30/Scene bcd1047bf912c6a4a8bc9f3a76eaf3fa954211ad064e0402b1c01bf3ba0e9fb6保持。
- 初步ChangeLedger PASS：518记录/27 governed代码差异（包含前序任务），ledger-in-progress.txt。

## 后继边界

父NATIVE-FRAME-RUNTIME-READER-MIGRATION、ZERO-FRAME-CACHE-CONTRACT和完整display/post未关闭。下一帧绑定/direct/step/cost/lifecycle必须成组；源data访问允许声明999，但BattleWorld28::step_frames_range在cost后将action<0或>=999送terminal，negative next先翻面再999处理，不能批量857→1000。只读细节已加入父Task，仍需原函数完整World见证和准确实现范围。

总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE；Q07正式DAT/角色图片未部署，当前真实Scene仍是旧Unity内容。禁止computer-use，未启动第二Unity Editor。

## 最终限定结论（2026-09-14）

VERIFIED / SNAPSHOT_NATIVE_DESCRIPTOR_BINDING_ONLY。联合244/244 PASS（job8a195fe8c1ec496194865944b5a0e985，163.219秒，related-244-pass.xml）；包含新20及旧snapshot/真实Logan两profile回放、HP/MP/出生/显示和Native/legacy接口检查。完整SelfCheck请求18:59:11Z、结果18:59:49Z新鲜PASS。

真实旧内容NTSD_Battle tick5暂停边界，当前descriptor857/碰撞998和Trans latch857恢复正确；修改后checksum与原始World checksum分别完整恢复，实体4→4（play-snapshot-pass.json）。随后既有Q05真实恢复/Renderer保留与关闭，World/slot/logic borrowers/render borrowers全部0，连续两帧Stopped，正常退出Play（play-cleanup-pass.json，19:00:41Z晚于请求19:00:40Z）。这不证明高位动作完整tick、正式EXE物理输入或新图片一致。

最终Editor idle/非Play/编译错误0，Scene dirty=false/root14，用户HUDBg x30与bcd1047b…哈希保持。没有新增缺失，保护3059路径的2921原始hash未变。正式EXE重新SHA核验B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。

实际命令/入口：Goal13_bridge.py refresh_unity/get_editor_state/read_console/run_tests/get_test_job；SelfCheck请求文件；manage_editor play + NativeFrameSnapshotPlay请求 + 既有Q05 ReplayPlay关闭请求；manage_scene get_active；Tools/Validate-ChangeLedger.ps1。最终账本结果见ledger-final.txt。

本三脚本范围结束；父NATIVE-FRAME-RUNTIME-READER-MIGRATION继续，下一先frame/direct/next/cost/lifecycle完整调用表与原函数见证，再准确Record实施。零帧全reader、其余display出生、post和Q07正式资源仍未完成，总目标ACTIVE。
