# 独立声音动作记录与快照版本

当前CODE_WRITTEN / COMPILE_PASS，联合Unity回归仍在运行；最终状态以末尾追加证据为准。准确22脚本：6 Unity生产、2诊断工具、13既有schema夹具及1新测试文件。

## 数据与范围

新增NTSDEntityRuntime.NativeSoundActionLatch，默认/reset -1，canonical copy保持有符号值，独立于Frame/WaitCounter；ECS fingerprint、完整checksum、full parity均覆盖。原runtime snapshot已用canonical copy，故只需提升entity14，聚合22，checksum25；character/base shell结构未变仍2/2。native capture header及工具TraceContentIdentity同步此五版本，trace3/raw2仍50字段（44绑定6MISSING），没有把新声音字段冒充已有raw绑定。

源依据：EntityState28.sound_action_latch=-1；BattleWorld28 append_native_frame_sounds合法action0..998且与latch不同才采样，先写latch再取数据；fusion set_action与definition transition成功路径重置-1。parent frame事务负责生产采样/定义变更接线，本包只建立可恢复状态，不宣称事件、声音播放或完整frame对齐。

6生产文件相对preimage差量仅Runtime新增3行、ECS1行、checksum字段/版本2行替换1行、parity1行、两个snapshot各常量替换；两个工具各一行版本头。既有测试只改准确schema断言与Q05旧checksum历史加入24用例，保留旧23；新测试覆盖默认/copy/零热分配、两profile claimed/raw checksum及restore、ECS敏感、Authority400 full parity与旧13/21拒绝。没有更改游戏数值断言，diff逐文件归档。

## 已运行证据

RED 48296b0b3e804fcf84a67f629dfd4be0：8/8失败，6缺字段、2旧版本仍接受。red-8.xml。

初轮0ae92dae53bb4b918f1a5d2d811787ba：32项31PASS/1FAIL。新Mobile测试误用仅Authority400支持full parity的接口，原结果留证；已按既有API边界只在400取full parity，两profile完整checksum/claimed+raw恢复/fingerprint均保留，并加raw-only checksum变化检查。不是扩大或修改production parity权限。版本夹具签名CRLF匹配遗漏在测试前用准确patch修正，最终Unity编译idle/error CS0后启动联合回归。

工具dotnet run --project Tools/NTSD28Parity五组：self-test51、self-test-raw-entities14、self-test-b0-domain-raw12、self-test-b0-domain-comparator6、self-test-b2-input-rng-raw5，共88/88通过。Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28Q06SoundLatch -ExecutableName authority_sound_latch_capture.exe成功；native-current.raw.jsonl以正式runtime catalog执行3tick/6实体，validate-authority-capture为valid-source-model-capture/certificate=false。build-manifest/native-process.json记录确切命令及当前EXE/源码/runner/binary SHA，Unity新鲜capture联验待。

保护3059路径中2921原始hash保持；较前包无新增变化路径或缺失（该统计为路径集合比较，不能解读为本轮生产脚本零修改），本22脚本准确diff另列。用户HUDBg x30/Scene bcd1047bf912c6a4a8bc9f3a76eaf3fa954211ad064e0402b1c01bf3ba0e9fb6保持，原18缺失未处理。当前账本520 records/49累计code diff通过。

禁止computer-use；Unity/GAS/非战斗/Scene/InputActions/正式资源/Server/Gen/Plugins未修改，未提交/推送/删除。字段随既有runtime pool Reset/归还，不增加manager/cache/queue/关闭阶段。Q10 WAV映射/实际播放回访与Q06事件生产分开验收，不能形成Q06→Q07→Q10→Q06循环。

## 最终限定验收（2026-09-14）

VERIFIED / SOUND_LATCH_CARRIER_AND_SCHEMA_ONLY。字段数据前置闭合，事件producer、definition/fusion重置和完整frame/cost/lifecycle尚未接通，不标为声音或完整战斗已对齐。

- 修正后联合job ba3c4d3bc21d4983b07c71044775c3b1：344/344 PASS、362.246秒。34个请求类/方法选择器均核对实际XML执行，无遗漏。related-344-pass.xml/test-selection-validation.json。首次8RED和32中31PASS/1错误Mobile parity fixture留证。
- 完整BattleRuntimeSelfCheck请求19:30:50Z，结果19:31:28Z新鲜PASS。新版本真实Scene tick5暂停snapshot边界，soundLatch857与当前frame1分别保存，修改后checksum及原始World checksum完整恢复，实体4→4（play-sound-latch-pass.json）。此为carrier恢复，不是声音播放验证。
- 随后既有Q05恢复/Renderer保留/有序关闭，World/slot/logic borrowers/render borrowers全0、连续两帧Stopped，正常退出Play。play-cleanup-pass.json的文件mtime已核对晚于19:33:06.146Z请求。最终Editor idle/CS0，Scene dirtyfalse/root14、用户HUDBg x30哈希保持。
- 5组工具共88/88通过。新版native wrapper真实Logan3tick/6实体capture通过validator；同一场景fresh Unity capture的content身份及14/22/25/2/2头完全相同。raw比较为different，仅既有6MISSING/36 occurrences，44已绑定字段相等；新sound latch不在raw50定义中，不能用该比较证明它的producer正确。current-raw-comparison.json/native-process.json/build-manifest.json可复核。
- 最终22脚本hash与测试后相同。6生产的字段/copy/checksum/parity/版本差量、2工具头、13旧schema测试与1新测试均有准确preimage/diff/metadata。保护3059中2921原始hash保持、无新增缺失；当前字段不新增runtime服务、队列或生命周期阶段。最终账本结果见ledger-final.txt。

当前联合版本14/22/25/2/2已通过本数据变更的拒绝/恢复/回放/真实Scene检查。旧13/21/24证据只保留为历史版本；既有源模型诊断可保留其原header，不伪造重标版本。正式资源未迁移，原6MISSING及完整帧/声音行为仍待后继。

下一唯一Task恢复NTSD28-Q06-NATIVE-FRAME-TRANSACTION-INTEGRATION-001 / READY_FOR_EXACT_PRECHANGE_RECORD：现可复用NativeSoundActionLatch及FrameSounds，不再为这项缺载体停留只读；按原2676矩阵建立Unity RED并成组处理frame/direct/next/signed成本/terminal及事件顺序。实际WAV映射/播放留Q10回访，不形成Q06/Q07/Q10循环。总目标/Q06 ACTIVE/FULL_ALIGNMENT_INCOMPLETE，禁止computer-use及非战斗/框架改动。
