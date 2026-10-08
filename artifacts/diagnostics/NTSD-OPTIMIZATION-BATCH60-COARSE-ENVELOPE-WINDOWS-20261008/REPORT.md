# 第60批保守包络真实千人对照

结论：SCOPED_WINDOWS_AB_COMPLETED / GAIN_SIGNAL / PERFORMANCE_FAIL / NOT_ADMITTED。四实际1000AI短窗有效，候选仍默认OFF；H07未达33ms/drop门，H11完整camera旧FAIL保持。本对照Task收口不等于优化阶段完成。

## 一次原Editor顺序对照结果

| 场景 | OFF logic mean / P95 ms | ON logic mean / P95 ms | mean下降 | collector mean OFF→ON ms | ON dropped tick |
|---|---:|---:|---:|---:|---:|
| Dispersed1000 | 93.884 / 154.139 | 75.636 / 126.166 | 19.4369% | 58.495→43.172（26.1956%） | 712（OFF922） |
| Combat1000 | 92.676 / 143.767 | 78.454 / 123.513 | 15.3462% | 58.856→45.820（22.1490%） | 702（OFF839） |

各120warm＋180sample，minAI/minRoster1000，seed1314149188/brute/无worker，原请求与同组OFF/ON只output身份不同。一次顺序窗仍含Editor/环境干扰，不宣称稳定长期百分比，不与57非同期数值作严格A/B。
Unity显示平均间隔OFF/ON：分散285.392/240.391ms，混战267.782/237.323ms；倒数约3.504→4.160、3.734→4.214FPS，仅所采间隔估算，非硬件Present/Android/120FPS证书。
中央CPU submission draw平均两模式约1991、原Harness ProfilerRecorder SetPass分散1994.315/混战1995未改善；不当GPU capture真实batch。未打开Profiler/FrameDebugger/GPU/M0，只复用既有Harness统计。

## 验证和范围

- 新29＋旧15共44/44、0skip，GREEN job398a84646c584b59b5b9f28ad9052381；原XML35,272B/SHA A61EB905AD8D0B3B7186FFCA44DADBCCF104D4FB4269EA5F9B876BD8DA7C51C1。有效RED29缺新接口的预期失败原件保留；复用59完整Driver资格，不重复资格/cost。
- 四完整StepOneTick GC.Alloc前后空/正例校准通过，begun/ended/accepted各300、steady180，invalid/rejected/sequenceErrors0、steady0events，工具由零事件推导0B；TimeNanoseconds不换算bytes。仅logic scope，不代替H11完整物化—上传—录制—提交0GC。
- 四production机制保持、exact/geometry各300/noFallback；envelope applied/unchanged/restored true。OFF方向/拒绝0，ON抽样最大164835/143319、169830/137486，是抽样最大不是累计300tick。
- 同组末tick300的20个parity/lockstep hash及完整snapshot字节SHA同，分散4,210,038B/混战4,208,480B。此为终态证明，不当本窗每tick或全native证明；受影响逐tick资格复用59。
- 四窗capacity passed/criticalDelta0，teardown activeGO/World objects/entities/slots/pool0；suite DONE/MEASUREMENTS_COMPLETED/completed4，11stage orderedShutdown true、objects/slots/borrowers0。原Battle/Menu SHA同，原Menu clean8roots恢复/PID19040/6402 idle。
- 唯一Suite相对准确dirty副本438增/4删，源1542837B7ACBEA8E2E6963A06BE692F8D04B4EE62BC228147BD08F130BF88510测试/实景后同；99guards/8原backup＋SharedResult第9副本同，HEAD45bbed41c64e0599601fa0df4028072d9f83303a同、sharedrequest absent、公共GREEN XML同。未改生产默认/Q06body/Scene/资源/settings/Server/Gen/Plugins，无Git丢弃/删除/move/第二Editor。
- 原件windows-01各report/request/observation/terminal/final-checksum及suite-result保留；精确SHA/数据见windows-analysis-01.json、windows-pair-parity-phase-01.json、windows-pair-comparison-01.json、post-windows-protection-audit-01.json。48.156s Play reload短暂TCP拒绝后原Editor恢复，未重复launch。

## 未达项与后继

H07仍PERFORMANCE_FAIL，正式120+1800/central unresolved-stale独立门未过（UNKNOWN，不从capacity0推断）。H11旧43迟发12event UNKNOWN/FAIL不被本四logic0覆盖；六项阶段4/6、限定产物5/6、34父关闭0、22—60共39已执行、Goal active。
候选有实景信号但NOT_ADMITTED。bool＋WorldRect只有字段payload17B，不等于精确resident增量或下界，padding可能吸收bool；ON/OFF共用同一已有array，不意味着58字段没有驻留代价。下一[61预算准入](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH61-COARSE-ENVELOPE-BUDGET-20261008.md)仅READY，核实际managed stride/容量/owner及steady/transition；旧编译类型不存在则历史delta保持UNKNOWN，不造旧镜像。
collector ON平均43.172/45.820ms仍自身>33ms，后继需据真实热点调整同范围策略，不重复四窗找PASS，不自动推广默认或解冻Role-aware/EXT1/Mono/ATLAS，不因次数停止Goal。

## 工具与文档更正

首次大分析整体序列化capacity/parity导致输出截断，改明确字段读取同原件，未据截断作结论或重测。有界log tail首次Math.Max int溢出，改long后读取reload。总表一次旧数字索引暂时错置两个M13行，已依据准确dirty副本逐文本恢复原结论；一次final patch乱序hunk拒绝，已核零文件写入后用正序文本补。失败与历史原件保留，六项唯一行/34父项及最终validator/diff另核。

最终文档核验（UTC 2026-10-07T23:18:27.4458259Z）：Tools/Validate-ChangeLedger.ps1 实际 exit 0，git diff --check exit 0（26 CRLF 提示，无其它差异错误）；总表34唯一父项、六项各三处表行，M13原结论保留。旧第59批概述已明确标为历史，避免其“60仅READY”与当前结论混淆。本次输出计数器误用行首正则未统计到缩进行，不采用其0 warning/error推导；详细历史告警数本轮未重新统计。原测试与实景证据不重跑，H07/H11 OPEN、Goal active、61仅READY不变。见同批 validation-final-01.json。
