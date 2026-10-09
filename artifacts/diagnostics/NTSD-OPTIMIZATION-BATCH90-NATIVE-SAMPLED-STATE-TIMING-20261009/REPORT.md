# 第90批：NativeSampledState 八子步骤归因准备

结果：FOCUSED_TEST_PASS / ATTRIBUTION_READY / DIAGNOSTIC_ONLY / DEFAULT_OFF。原项目共享Editor56/56通过；只补观测，不是千人逻辑优化达标或实际热点定位。六项有限首阶段仍5/6、H07 FAIL、已执行69（含本包准备）、34父项关闭0、Goal active。

## 改动与边界

Change NTSD-OPT-H07-NATIVE-SAMPLED-STATE-TIMING-090，Task/Operation NTSD-OPTIMIZATION-BATCH90-NATIVE-SAMPLED-STATE-TIMING-20261009。仅NTSD28InputTwoPassModule.ProcessNativeSampledState八段计时包裹、SimulationWorld.DetailTimingDiagnostics枚举34—41与稳定名称、原CharacterInputLiveSlotLoopEditorTests新23case及旧PhaseCount34→42。原0—33、guard/调用顺序/live frame读取/Ai与InputState条件/异常传播保持；默认仍OFF，未删同步/改变World/writer/collector或加新owner。四既有long数组只冷增256B payload，非全RSS；热路径无新容量扩展。

新八段：NativeSamplePreprocess、NativeComboRoute、NativeThreeButtonRoute、NativeDirectionRoute、NativeBuiltinsRoute、NativeLegacyProjection、NativeCanonicalAiSync（仅AI）、NativeInputStateSync（原角色条件）。父NativeSampledState是inclusive，不能与子段重复累加。88整段约2.25ms只是历史归因线索，不是本包新数据。

## 实际验证

- RED：68a511e1632a48f0a4d9e03c71ba3725，23执行/2等价控制通过/21缺新schema计时失败/0skip；UTC01:06:27—01:07:17，50.285920秒。red-01/tests.xml SHA A99AFBC3BF8CDD400F790D824E2B7F709F613D4FD22D951CE6EA22B8BF949307。25秒状态观察超时不等于测试停止，后续同job终态已确认，没有重提任务。
- GREEN：ec03d804e7424b2aa2bb69e60f03e939，56/56、0skip，UTC01:11:07—01:13:33，145.3290821秒；新23＋原InputBoundaryTiming12/ProducerSnapshotReuse15/DataOrientedCharacterInputStore6。green-01/tests.xml SHA 3FE36B7A088ED3590FDD5C59AEB6B336BC2405C9D0F56E1C7D14DFA1E50CE609。catalog10400不是执行数。
- 新case覆盖schema/gates/条件/inclusive父子/关闭重置/null、16tick输入入口ON/OFF proxy/history/checksum/RNG与资源等价；128预热后1024直接processor+BeginTick分配0B。该0B不是完整logic/render/device证书；未执行异常注入，八finally只静态审查。
- 主代理已复核限定源差异：module方法外内容不变，diagnostics只append enum/常量名称，原tests移除新块并还原Count后与dirty-before归一化相同。没有跨路由冻结frame、移除canonical AI或InputState同步。
- GREEN期间01:13:18Z审计：929保护/9before全同，Ledger exit0（1385 Records/17 governed files/4168历史WARN/0ERROR）、diff check exit0、HEAD不变/暂存空、实际stress request.json absent。最终审计单独附下方及audit-final-01.json/after-manifest-01.json。
- GREEN后原Unity2022.3.62f3/PID74724，idle/nonPlay/notCompiling，sole Menu Scene clean、root8；未保存Scene、未启动第二Editor。

本包使用现有2022共享coplay桥接，按unity-cli技能先确认连接/idle/Scene保护；Unity6 Pipeline缺席不视为Editor关闭，不安装/升级依赖。run_tests准确regex NativeSampleTiming_先RED，随后NativeSampleTiming_|InputBoundaryTiming_|ProducerSnapshotReuse_|DataOrientedCharacterInputStore_ GREEN；get_test_job同ID确认终态并读取原XML。原XMLroot为test-suite，不是test-run；一次只读解析用错root返回null，已更正读取DocumentElement并验证56实际cases，未篡改XML。

## 外部变化与保护

开始前只发现Packages两文件移除com.vovgou.loxodon-framework（执行者未知），保留现状、不回退，before-manifest-02仅更正这两项当前SHA，不排除保护；其它929保护和9before保持。旧88 CPU读数不能被当作新依赖环境同期基线。禁止Scene/资源/Settings/Gen/Plugins/Server、Q06活跃方法体、破坏性Git与旧证据覆盖。

## 未验与下一步

本批没有Play/实际1000AI/Profiler/GPU/M0/正式1800/Android/120FPS，不声称性能改善、八子阶段实际占比或默认推广。89四帧复用无一致收益/NOT_ADMITTED保持，不再重采找PASS。下一仍H07：新的准确Task/输出中对实际NativeSampledState子阶段归因，同seed/workload ON/OFF观察开销与完整logic0GC，不重复85/88旧原件，不删必要同步、不新增Grid/Burst或解冻EXT1/Mono/ATLAS专项，不重开已过五项。

恢复仅经用户批准按9个当前dirty before副本/准确hunk；本包无删除、移动、Gitdiscard/commit/push。Goal保持active，批次结束不是阶段或总目标完成。
## 最终审计

最终限定审计 2026-10-09T01:21:21.4979794Z：929保护/9before均0漂移，三C# SHA与GREEN时同，Ledger exit0（1385 Records/17 governed files/4168历史WARNING/0ERROR）、git diff --check exit0（24条CRLF warning、无非warning输出），HEAD a10c358eeb956f2153e7d5bfbb60c3c1b353fd50、暂存空、实际request.json absent、Menu/Battle磁盘SHA不变。01:22:11Z重核module方法外及旧测试归一化同、八finally；原Editor idle/nonPlay/sole Menu clean root8。未删除/覆盖外部依赖或启动新窗口；完整audit-final-01.json和after-manifest-01.json保留，Operation只限定本次文件操作VERIFIED，不升级H07/Goal。
第一次最终audit将Git CRLF warning的PowerShell ErrorRecord直接序列化导致输出截断，没有文件写入；已转字符串重新执行同一只读审计。两条观测失败不作为测试或性能失败，不新增批次/重构。当前文件操作限定VERIFIED，Change仍FOCUSED_TEST_PASS/DIAGNOSTIC_ONLY，不升级H07或关闭Goal。
