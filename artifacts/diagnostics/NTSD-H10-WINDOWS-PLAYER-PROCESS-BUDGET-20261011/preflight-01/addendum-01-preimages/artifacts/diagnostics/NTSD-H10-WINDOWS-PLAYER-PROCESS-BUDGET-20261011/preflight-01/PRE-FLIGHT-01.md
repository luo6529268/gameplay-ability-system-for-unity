# H10 Windows Player 进程预算 PRE-FLIGHT-01

结论：**只读预检完成，方案PLANNED，尚需具名读取补齐入口/部署合同，不能直接实施或构建。** 本次未改脚本/Settings、未编译或运行测试、未build/Player/sidecar。H10父项OPEN；旧resume-03成功及全部历史失败保持。

已确认：
- HEAD8d55c6b68912560c7af755c424b41d3464a1921a、暂存空、原10tracked dirty；全部10真实dirty已另存字节备份，新脚本/.meta/工具/build-01均不存在。
- 原Editor只读状态idle/nonPlay/noncompile/nonupdate/nontest；Menu clean8。两个只读MCP命令可达，没有重启或第二Editor。
- disk Standalone IL2CPP=1、background1/frameTiming0、Burst true。旧builder会改这三类值且SaveAssets，不能调用；新builder仅backend暂态Mono＋归因恢复。
- Request默认sound=inherit，simulationOnly=false时解析为dispatch；计划显式dispatch＋simulationOnly=false＋skipLate=false。Atomic DataOrientedCanonical派生正式SoA/Indexed/Unified，不能继承旧JSON的独立候选flags。
- StartRun禁并发；Configure要求World/claimedSlots/两池active全0；真实普通Menu流程已经有角色，不可直接追加1000。App空players副本能跳过其SetupBattleCharacters，其他bootstrap仍待R2实证。
- StartRun内部TryConfigureEmptyDiagnosticWorld不先Shutdown（主会话提供Driver具名证据）。终态CleanupInternal却调用RecreateWorld，后者实际关闭audio并做map=true诊断完成后创建新World。必须捕获旧owner/owned refs，并分别记录runner终态与其后App真实地图关闭；不能把内部map=true当实际地图11。
- Harness固有MobileExtended1050/LooseQuadtree/Manual/paused/enableFrameChecksum=false，单OID1000角色基准。这与普通LocalFreeRun有区别；不能把其压力结果叫普通同配置、全角色压力或H07 PASS。
- App MenuMain返回分支无ResetAll；普通UI三轮复位和prewarm重入仍需R3。
- 已生成2752个已知部署域的逐文件source/target/SHA，96,956,488B；未复制。其它raw UI/Config消费闭包未证明，manifest目前是已知域清单而非完整可运行保证。

具体可审方案：[PLAN.md](PLAN.md)。下一步准确读域：[READ-SCOPE-REQUESTS.md](READ-SCOPE-REQUESTS.md)。机器证据：source-input-fingerprints.json、settings-scenes-inputs.json、initial-dirty-backups.json、proposed-output-absence.json、deployment-manifest-known-domains.json、editor-readonly-state.json、editor-readonly-scenes.json。

候选有限表：1次Mono Development build、2个独立Player进程；普通和combat1000各同进程3轮，第1轮120warm/180smoke、第2/3轮120warm/1800formal。所有数量/阶段待主审确认后才写执行请求；本次额度为0build/0Player/0sidecar。外部100ms Private Bytes与WorkingSet分列，exact PID+CreationTime+QPC对齐，不读CommandLine/env/credential；320MiB/2GiB/3GiB不变，dev PC不认证8GB低配。

阅读范围只涉及授权harness配置/入口/清理及直接局部定义、原四份诊断路径、随后主会话具名App生命周期片段。为提出准确读域，额外仅rg数据类型/方法声明名称；R1-R4方法体未打开，未读factory/StageWave/producer。一次只读范围打印超过文件EOF发生IndexError，已经读到最后一行；无文件变更或运行副作用，后续打印使用边界裁剪。没有调用历史read_thread，没有读取复制旧敏感工具输出。

实际命令包括git status/rev-parse/diff --cached、rg具名定位、Python有界只读文本/哈希/manifest及get_editor_state/manage_scene只读；无refresh_unity/run_tests/build/进程启动停止。最终保护和validator结果追加于收尾原件。预检阶段结束不等于暂停整个H10，也不授权推进下一优化点。

父项覆盖映射及本Task继续顺序已补在[H10-COVERAGE.md](H10-COVERAGE.md)：多角色/技能最坏集合和完整声音producer动态覆盖仍是当前PC真实缺口，CASE-MATRIX必须在具体实施/构建批准前锁定，不由两个单一负载进程自动满足。
