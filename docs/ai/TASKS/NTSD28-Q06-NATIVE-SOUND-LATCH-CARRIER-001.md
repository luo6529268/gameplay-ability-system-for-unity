# 独立帧声音动作记录的数据前置

IN_PROGRESS / TEST_FIRST。父FRAME-TRANSACTION-INTEGRATION，准确路径见元数据。source EntityState28.sound_action_latch=-1；append_native_frame_sounds将合法0..998动作写入，不能共用action_latch；出生EntityState默认、definition transition及fusion set_action明确重置-1。Unity当前无等价字段。本包增加Runtime.NativeSoundActionLatch=-1、Reset和canonical copy、ECS fingerprint/完整checksum/full parity；snapshot复用runtime copy，增加字段必需entity14/aggregate22/checksum25，两个shell未变仍2/2；native trace metadata及工具身份检查同步14/22/25，raw50字段/trace3/raw2形状不变（本字段不假装已加入raw列表）。

旧测试仅更新准确SchemaVersion期望13/21/24及Q05五版本数组到14/22/25，旧payload拒绝用例保留并新增拒绝前一版13/21/24。不改数值/行为断言。新反射测试先RED：新建/reset -1、有符号copy、claimed/raw snapshot、完整checksum/full parity/ECS fingerprint敏感性、两profileaggregate restore、旧entity/aggregate拒绝无副作用及热copy无分配。相关snapshot/replay/schema/trace工具测试、SelfCheck和真实Scene快照恢复/关闭。新字段producer（两个frame声音采样、definition/fusion重置）由父下一事务准确接线，本包只交付可恢复数据，不宣称声音/帧事务完成。

不新增manager/queue/cache：字段随原实体和runtime pool的Reset/归还，不改变十一阶段有序关闭。保留frame action/latch/078/collision的既有字段、所有Input成本字段、非战斗/Unity-GAS/Scene/InputActions/资源/Gen/Plugins/Server；禁止computer-use。回滚需批准仅本差量，不能回滚用户工作；版本未验完不得发布baseline或跨版本传送。修改过程为同包未发布窗口，完成时必须原子验证新版本。下一自动返回FRAME-TRANSACTION-INTEGRATION。

## 最终限定验收（2026-09-14）

VERIFIED / SOUND_LATCH_CARRIER_AND_SCHEMA_ONLY。字段数据前置闭合，事件producer、definition/fusion重置和完整frame/cost/lifecycle尚未接通，不标为声音或完整战斗已对齐。

- 修正后联合job ba3c4d3bc21d4983b07c71044775c3b1：344/344 PASS、362.246秒。34个请求类/方法选择器均核对实际XML执行，无遗漏。related-344-pass.xml/test-selection-validation.json。首次8RED和32中31PASS/1错误Mobile parity fixture留证。
- 完整BattleRuntimeSelfCheck请求19:30:50Z，结果19:31:28Z新鲜PASS。新版本真实Scene tick5暂停snapshot边界，soundLatch857与当前frame1分别保存，修改后checksum及原始World checksum完整恢复，实体4→4（play-sound-latch-pass.json）。此为carrier恢复，不是声音播放验证。
- 随后既有Q05恢复/Renderer保留/有序关闭，World/slot/logic borrowers/render borrowers全0、连续两帧Stopped，正常退出Play。play-cleanup-pass.json的文件mtime已核对晚于19:33:06.146Z请求。最终Editor idle/CS0，Scene dirtyfalse/root14、用户HUDBg x30哈希保持。
- 5组工具共88/88通过。新版native wrapper真实Logan3tick/6实体capture通过validator；同一场景fresh Unity capture的content身份及14/22/25/2/2头完全相同。raw比较为different，仅既有6MISSING/36 occurrences，44已绑定字段相等；新sound latch不在raw50定义中，不能用该比较证明它的producer正确。current-raw-comparison.json/native-process.json/build-manifest.json可复核。
- 最终22脚本hash与测试后相同。6生产的字段/copy/checksum/parity/版本差量、2工具头、13旧schema测试与1新测试均有准确preimage/diff/metadata。保护3059中2921原始hash保持、无新增缺失；当前字段不新增runtime服务、队列或生命周期阶段。最终账本结果见ledger-final.txt。

当前联合版本14/22/25/2/2已通过本数据变更的拒绝/恢复/回放/真实Scene检查。旧13/21/24证据只保留为历史版本；既有源模型诊断可保留其原header，不伪造重标版本。正式资源未迁移，原6MISSING及完整帧/声音行为仍待后继。

下一唯一Task恢复NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD：现可复用NativeSoundActionLatch及FrameSounds，不再为这项缺载体停留只读；按原2676矩阵建立Unity RED并成组处理frame/direct/next/signed成本/terminal及事件顺序。实际WAV映射/播放留Q10回访，不形成Q06/Q07/Q10循环。总目标/Q06 ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use及非战斗/框架改动。
