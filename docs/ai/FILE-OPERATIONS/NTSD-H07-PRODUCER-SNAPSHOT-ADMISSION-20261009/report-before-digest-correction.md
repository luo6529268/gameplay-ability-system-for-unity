# 生产者 Runtime 快照复用限定准入：本次不采用

结论：SCOPED_MEASUREMENTS_VERIFIED / NOT_ADMITTED / PRODUCTION_DEFAULT_OFF。当前 producer 局部收益存在，但混战整 tick 均值回退14.448675%，原因未闭合，未通过已批准的收益门。没有进入生产接线阶段，不重复同版窗口寻找PASS。第一项 Role限定准入完成的结论保持；H06父项/H07/Android仍开放，95仍OFF，旧Goal和第三项不恢复。

Task / Operation：NTSD-H07-PRODUCER-SNAPSHOT-ADMISSION-20261009。Change：NTSD-H07-PRODUCER-ADMISSION-WINDOWS-001，VERIFIED仅指冷测量实现与结果，不代表候选通过。

## 真实修改与运行范围
仅在 BattleOptimizationWindowsAiSuiteEditor 新增 BeginScopedProducerAdmission、BuildScopedProducerAdmissionRequest、新输出根与SuiteState模式bool及冷路由；沿用88的request、Apply/Observe/Restore、可靠GC、容量与十一阶段关闭。新增 H07ProducerSnapshotAdmissionEditorTests.cs/meta，六项配置等价和两个Explicit只读设置快照。生产 World、Host、guard及AI行更新代码未改，复用默认保持false。
新增bool owner为原Editor Suite，容量1；starting标记finally清，序列状态走原终态释放。没有新Runtime缓存/队列/manager，不改变十一阶段顺序。
原项目Editor 2022.3.62f3，重启后PID39108/MCP6402；测试前sole Menu clean8/nonPlay/nonCompile/nonTest。只启动一次四窗，固定DataOrientedCanonical、Role、Sakura1000、既有seed/输入/renderer，OFF/ON只由原冷owner切换。每窗120warm+180sample，不是1800正式证书。

## 当前测量
| 布局/复用 | tick均值ms | tick P95 ms | producer刷新均值ms | CharacterInput均值ms | dropped tick（全run） |
|---|---:|---:|---:|---:|---:|
| 分散 OFF | 62.096542 | 103.079055 | 1.514758 | 16.327482 | 899 |
| 分散 ON | 45.343205 | 55.435655 | 0.468723 | 11.361804 | 382 |
| 混战 OFF | 50.428038 | 97.519545 | 1.111011 | 13.043353 | 323 |
| 混战 ON | 57.714222 | 138.610895 | 0.578927 | 14.293548 | 380 |

分散整tick减少16.753337ms、26.979500%，但OFF长尾明显，不能将全部差值归因复用。混战producer减少0.532084ms，但整tick增加7.286183ms；collector16.874329→18.973303、late6.395358→8.118136、NativeSampledState3.211644→4.046531。多个未改相位一起变慢，说明顺序窗口存在广泛时间波动；这是已观察相关性，环境噪声、系统争用、候选间接影响的因果均未证实。不能以猜测噪声豁免回退门，也不把父子时间相加。当前结论是不准入，不是证明复用必然拖慢。
原88约0.54ms producer、0.7ms整tick信号保留其历史身份；不能覆盖本轮混合结果。收益门不通过的原因不是尚未33ms。

## 一致性、0GC、容量、关闭
四窗均实际AI/roster1000；固定Role实际collector3、mismatch=false，fallback峰值4/4/3/3、abort0。冷owner applied/unchanged/restored均true；采样观察OFF max skip0/fullRefresh1000，ON max skip1000/fullRefresh0，非逐tick累计数。
每对65请求字段仅outputPath不同，20 finalParity/finalLockstep hash同，末tick300完整snapshot字节同。分散SHA F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FAF86D8AD7B6AA；混战SHA E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。末态不是所有tick或Native全域证明。
完整同步主线程Driver scope前后1MiB正/空负校准通过，accepted300/steady180/nonsteady120，invalid/rejected/sequence0，steady events0/proven0B；非稳态142/24/24/24事件。不能外推camera/GPU/worker或全300tick0GC，不能把GC.Alloc次数当回收停顿。
四窗capacityPressure PASS、criticalDelta0、violating0；每窗对象/entity/slot/active pool/reference pool及cleanupException为0。Suite DONE/MEASUREMENTS_COMPLETED/completedRuns4/error空，orderedShutdown true，objects/slots/borrowers0、原Scene恢复、Battle SHA保持。inactive缓存1050不是active残留，不清理。
设置前后JSON完全相同；最终原Editor idle、Menu sole clean8。2390文件保护中唯一变化为已登记Suite；真实备份SHA都同，HEAD/staged不变，第一项所有生产改动和窗口证据保持。

## 实际命令与结果
经原Editor MCP6402（8字节长度帧，Python TCP bridge）顺序执行，未启动另一Editor：
- run_tests mode=EditMode, groupNames=[^NTSD.Test.Editor.H07ProducerSnapshotAdmissionEditorTests.Scoped, ^NTSD.Test.Editor.BattleOptimizationWindowsAiSuiteRequestTests.ProducerSnapshotOwner_, ^NTSD.Test.CharacterInputLiveSlotLoopEditorTests.ProducerSnapshotReuse_]：job fe8eee3f1f10420b83000f2a9d114848，44/44 PASS、0skip，qualification-tests.xml。
- run_tests mode=EditMode, testNames=[NTSD.Test.Editor.H07ProducerSnapshotAdmissionEditorTests.AdmissionEditorSettings("before")]：job48ab990c0b6b449694eda35214ee5750，1/1 PASS。
- execute_menu_item menu_path=NTSD/Validation/Optimization/H07 Scoped Producer Snapshot Admission：一次四窗，原件windows/。
- run_tests mode=EditMode, testNames=[NTSD.Test.Editor.H07ProducerSnapshotAdmissionEditorTests.AdmissionEditorSettings("after")]：job6d2b7b9dcab24e5b9ee0963a789ae9a4，1/1 PASS。
- Python只读摘要/断言：配对请求、20hash、snapshot字节、校准、容量/关闭、冻结源和备份核验均PASS，measurements-summary.json、preservation-audit.json。
- 原Editor完成脚本导入并运行上述测试；末read_console(types=error)返回6条MCP Client handler exited传输日志，没有可据以声称Console完全无条目的结果，不将传输消息当C#失败。
- pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity：交付前执行，结果见下方与validator.log。

复用依据：RefreshCharacterInputSnapshotsAfterProducer与95前保存的87/88候选方法逐字相同，SHA B3548A0A2D21804A1C93BC5B6C6533BF8766D9B593B5419B73D79E89A1ED51DB；本轮生产World/Host未改。当前15个guard/陈旧修复/派生/人工/hook/AI行/逐tick输入双RNG/局部GC测试已包括44项；88的88配对/176完整Driver step及正式root限定字段按原scope复用，不把旧整测试文件SHA或旧64/87测试总数宣称本轮新鲜通过。25局部collector、82有限Driver、91/93与95不同scope保持各自边界。
未执行：生产复用接线及其接线测试（收益门未过）、额外同版AB、120+1800正式性能、Android/Player/GPU认证、Q06读取、新优化路线。不是遗漏必要正确性/0GC门。

## Editor弹窗事件与下一项
第一项13/13生产测试XML已生成后，原PID74724状态查询超时；用户确认有弹窗并表示点击后Editor退出、自行重启。弹窗标题/按钮/原因未知，不能归因为代码崩溃或代理重启。新PID39108经安全检查才启动本轮，恢复状态已验证。
两项具名任务到此收口：Role正常空配置默认已推广、Brute显式对照/非法回退保留；producer复用不准入、生产OFF。后续H07-INPUT只建议结合91已有归因，从同tick重复捕获/同步的准确成本、字段失效和可辨收益形成独立方案；不得自动重跑本候选/实施95/AI降频或疏散。若要单独定位本轮时间波动，需要新的明确诊断问题与能改变判断的实验合同，由用户决定。
