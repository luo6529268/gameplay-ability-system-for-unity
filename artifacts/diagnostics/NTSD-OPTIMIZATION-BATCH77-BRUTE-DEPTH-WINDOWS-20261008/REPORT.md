# 第77批：深度拒绝候选实际千人OFF/ON报告

SCOPED_WINDOWS_COMPLETED / MIXED_RESULT / NOT_ADMITTED / PERFORMANCE_FAIL。有限接线/测量包完成，不是H07或H11阶段完成，也未推广候选。56已执行、阶段4/6、34父项关闭0，Goal保持active。本轮按用户通知仅原NTSD Editor74724/6402，未操作另项目；Unity2022.3.62f3/正式336 authority/33ms、3ms、max2/规则、publication、segment/fail-closed、十一阶段shutdown保持。

## 实际结果

各窗120warm+180sample、1000 actual AI、同seed1314149188/input ai、DataOrientedCanonical/ordinary Brute。65request字段除outputPath逐字段同；未修改请求schema、生产Query/Renderer、Scene、资源、ProjectSettings。新Suite仅cold owner/观测，四production flags仍ON，其余candidate/branch timer/renderer binding reuse OFF。一次固定顺序AB，以下仅本轮观察，不当统计上稳定收益或跨设备保证。

| 工作负载 | OFF mean / P95 ms | ON mean / P95 ms | mean变化 | P95变化 | OFF→ON droppedBacklogTicks | OFF/ON SetPass平均 |
|---|---|---|---|---|---|---|
| Dispersed1000 | 99.762094 / 132.251350 | 90.871554 / 141.625745 | -8.9117% | +7.0883% | 1075→902 | 1994.314607，同值 |
| Combat1000 | 120.189139 / 158.784990 | 90.361139 / 119.763390 | -24.8175% | -24.5751% | 1255→913 | 1995，同值 |

ON两窗P95均>33ms且drop非0，故PERFORMANCE_FAIL；maxCatchUp四窗2，maximumBacklog字段0不能豁免丢弃的wall-clock debt。这里只引用现有report计数原scope，不把它伪装成单独steady域计数。统一RenderPass路径未变，SetPass各89有效样本仍高；不把它或CPU DrawMesh当真实GPU batch，不做Frame Debugger/GPU capture/EXT1 M0。不能声称120FPS/Android认证或当前生产默认已经提速25%。

## 正确性、容量、0GC与退出

四窗harness/workload有效、实测最低AI/roster1000，capacityCritical/totalRejectedOrDropped/runtimeCapacityGrowthReject/presentationThrottle delta均0，最后6类活动清理残留与cleanupException全0。原World/native/pool/driver沿既有owner与11阶段清理；suite最终MEASUREMENTS_COMPLETED/DONE/error空/orderedShutdown true/remainingObjects-slots-borrowers均0，Menu唯一loaded、clean8roots/非Play/idle，Menu/Battle文件原hash保持、error-CS Console0。

三prepared容量participant/ITR/body=1050/14700/5250，四窗19/18/23/17次fresh冷观察无变化、精确cache fallback delta0、独立depth flag未漂移。所有窗口最终restore true，候选production default仍OFF。数组identity资格复用76，77只新观察容量，不把它写成每tick数组identity新证书。

两ON观测到最后collection的最大depth拒绝122787/125534；这是fresh观察值，不是全tick累计，也不据冷观察0证明整个窗没有任何拒绝。76的32tick零覆盖保留，不与本180稳态的正覆盖矛盾；不修改layout/input/tick/规则以制造coverage。

可靠fullDriver GC observer前后1MiB positive/empty校准均通过，每窗300accepted/180steady，invalid/rejected/sequenceError/steady events均0、provenSteadyAllocatedBytes0。只覆盖整个同步主线程StepOneTick；不是H11完整物化—上传—录制—提交或PlayerLoop/Android/GPU/Native memory证书，43camera FAIL仍保持。公共report schema没有central独立stale/unresolved字段，本批不据全局capacity0推导它们，原阶段门仍OPEN。

同一workload两次tick300完整final-checksum文件原字节SHA一致：
- Dispersed：F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA，overall35910769...b8c64。
- Combat：E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12，overall81c5ef64...112c。

四运行共1200实际steps/720steady。上述是新鲜末态证据，不等于1200tick逐tick trace或全native对齐；76 scoped Driver资格依赖hash D33345A3...9242已核保持，原受限权威证据不升级为全角色保证。

## 修改和测试证据

唯一C#写域：Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs。before43ECB5B1...83D8C、tests-only437E0214...FAC8、最终6FE1054C0FE8F02223B706E218D559E90C9215B35001EC274A733591DA8470E7。新增depth菜单/冷flag owner/RunState-SuiteState/三容量观察/恢复/36 tests；旧BeginSuite17参数、request65字段、旧tests/helpers保持，ordinal冷apply/observe/complete补排斥depth以禁止混跑。没有Query/default、Admission/Role、render backend、数据格式或第三方修改。

实际MCP：
1. 6402 get_editor_state/get_loaded_scenes：原Menu clean/唯一8roots/idle/noTests，read_console error-CS0；manage_asset import精确Suite。
2. run_tests EditMode group '^NTSD.Test.Editor.BattleOptimizationWindowsAiSuiteRequestTests.BruteDepthRejectWindow_'：job8b212b8d889449d6a557b62cf77bf58f，新36有效RED（缺方法/字段）原件red-results-01.xml SHAEDBCB742...6FA0。
3. 实施后run_tests新36＋旧coarse/binding Request各4/Menu各1=46：job403e76360ad94b56be07c1b192a31f66，46/46/0skip/3.4792491s；green-results-01.xml SHA5D9A625D...17268，green-terminal-01.json保留。
4. clean/compile/source再核后execute_menu_item一次 'NTSD/Validation/Optimization/Batch77 Depth Reject Full Driver GC 1000 AI OFF ON'；自动切干净Battle、四窗、关闭后返回Menu，不保存Scene、不重复启动。windows-launch-01.json/win四report及request/observation/final-checksum/logic-gc/terminal/suite-result均保留，windows-audit-01.json记录逐字段事实。
5. Tools/Validate-ChangeLedger.ps1实际exit0/1373Records/1code diff covered；git diff --check exit0。4395历史warning非本批error，不清理；最后保护、backup、source与fresh Editor审计另见final-audit/validation-final。

导入reload期间一次6402暂拒绝，PID74724继续运行、同端口自动恢复；不重启/装Roslyn/插件。测试初次插入的自加重复签名在任何测试前修掉，不把它当有效RED；matrix初37/47因合并Second/Wrong owner校正为36/46且执行前记录。第一次validator Write-Host绕过pipe造成输出截断，实际0结果仍观察到；随后正确6>&1捕获摘要核1373/1/exit0，没有删历史或重跑测量。一次H07行过滤过宽导致empty JSON解析失败，改准确'^| H-07'只读检索，无写/规则影响。

## 后继有效动作

该候选有Combat约25%观察信号，但未达阶段、Dispersed尾耗时反升；不重复已过资格/局部cost/四窗或追加1800寻找PASS，不默认推广，也不因第77批结束或计数停止Goal。下一准确包先用现有阶段/分支证据和当前普通Brute代码，选择残余collector/capture/cache成本中尚未解决的具名热点；是否需要一份最小新测量取决于当前证据能否裁决，不凭约51ms旧分支时间直接当本候选新热点。不可扩大角色/平台/历史对齐，也不重开Foot/H06/M13/M14已完成评估。

中央SetPass原因75已闭合：Auto全目录145页计划2320MiB>512MiB回退SourceTexture2D，排序后纹理频繁交替分段。本批没有解决这条绑定/资源布局问题；定向修复若改ATLAS/bank/资源需准确新授权，不能暗中增预算、排序重构、GPU instancing或动态图集。该单项门不阻止其余已授权CPU/H11工作。


# 第77批报告（事前快照，非当前状态）

PLANNED / WINDOWS_PENDING / NOT_ADMITTED。冻结Task/Change/Operation事前登记，未有测试或性能结果。仅已有Suite冷接线、实际千人OFF/ON和旧校准GC，不启动专项M0/资源优化。最终真实命令、证据及剩余项将追加，H07/H11 OPEN、Goal active。

