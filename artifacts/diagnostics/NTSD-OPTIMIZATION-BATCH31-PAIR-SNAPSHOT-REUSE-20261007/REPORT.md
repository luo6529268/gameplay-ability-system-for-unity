# 第31批精确方向快照复用

最新结果见下方限定报告；原PLANNED是事前历史，不再表示窗口未跑。

PLANNED：准确Task/Change/Operation已冻结；尚未改脚本/运行测试/测量，不称FPS改善。原30证据复用，本批只减少已确认重复读取，验证受影响域。

# 第31批命中关系快照复用：限定验证报告

当前结论：实现/120项聚焦与两真实1000AI短窗的限定行为验证通过；局部重复读取显著减少，但真实短窗收益不一致，PERFORMANCE_INCONCLUSIVE / NO_DEFAULT_PROMOTION。H07性能仍未达33ms，H11完整0GC未通过，Goal active；不新增成功项数。

## 实际改动及所有权

准确写域为BruteForceSceneQuery、RoleAwareCollisionShadowSelfCheckTests、BattleOptimizationWindowsAiSuiteEditor。只在一次cached attacker→target方向的栈上惰性捕获PairSnapshot，再供同方向itr/body逐次record；不跨方向/tick保存、不新增heap/cache/lease/容量。每body原group/Kind4计数/nearest/reject/capacity/select/RNG/候选顺序照旧。legacy/fallback仍逐body捕获；其record主体改为共享helper，未称默认brute代码完全未触及，也未测定该wrapper的默认brute成本。

既有Factory、Q06、Scene、资源、Settings、Server、PERF/ATLAS/EXT1/Mono均未修改。生产backend选择仍未切换；该Role优化不会自动使默认Battle获益。

## 编译与聚焦检查

原Editor PID19040/6401，Unity2022.3.62f3；未启动第二Editor或强制reload。两过滤类：
- NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests
- NTSD.Test.Editor.BattleOptimizationWindowsAiSuiteRequestTests

最终job ec8f93e0fca04ce19ff21d6a146a82b4，120/120实际PASS，5.1262615s；7新增collector＋3新增request＋110相关既有回归，不是全9371测试。原Editor最终无compile/reload pending。

失效/失败原件保留：invalid-red-zero-tests-01.json（新增block放错类导致2CS0246，旧程序集0用例不算PASS/RED）；red-01.json（9预期RED＋1容量fixture错误）；green-failed-01.json（2 fixture错误：遗漏ForceRoleAware、误用kind7 nearest）。更正仅新test fixture，未修改规则以迎合检查。最终见green-02.json。

局部1000逻辑参与者固定fixture，8warm、旧/新各16sample且交替顺序：capture92160→15360，collector mean13.960275→11.0231125ms，约21.0%下降；不是1000AI、FPS或0GC证书。

## 两个真实1000AI短窗

原菜单NTSD/Validation/Optimization/Batch31 Role Pair Snapshot Reuse仅调用一次；120warm＋180sample，各场在采样期11/12次观察均min1000 active AI与base roster。两个request65字段逐项与29对应短窗核对，只有outputPath不同。原正常renderer/sound、seed0x4E545344、DataOrientedCanonical、无worker、max2、strict requireZeroGc字段未放宽。

| 工作负载 | 29整tick均值 / P95 ms | 31整tick均值 / P95 ms | 29→31 collector均值 ms | 29→31可见帧均值 ms |
|---|---|---|---|---|
| Dispersed1000 | 48.4233 / 73.4995 | 44.1631 / 51.6676 | 15.6509→14.6023 | 165.0681→161.8275 |
| Combat1000 | 45.7206 / 54.9208 | 46.9036 / 81.2761 | 15.2590→15.7901 | 157.4969→157.5204 |

分散整tick均值下降8.8%，混战上升2.6%、P95明显上升；两个都>33ms，可见帧仍约158–162ms。不同时间的短窗不是同机同时配对实验，不能把全部差值归因本patch；本轮不追加同构重复测量或长窗刷到PASS。真实整体稳定收益未证明，候选不准入生产默认，不称截图0.7FPS已修复。

31精确loop均值7.0931/7.7472ms，DirectBroadphase3.2375/3.5147ms；CharacterInput11.0490/11.4411ms、LateEntityUpdate5.6743/6.1239ms。这些既有分相统计为后继实际热点依据，不为新广泛重构授权。

## 限定一致性、容量和0GC边界

两份末tick300扩展snapshot全部10个hash及完整JSON SHA与29对应原件一致：
- Dispersed：F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA
- Combat：E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12

这是末tick的Unity旧/新一致，不是逐tick/native trace或全行为认证。两capacityPressure PASS/criticalDelta0、canonicalMismatch0、compatFallback0、AI preCommitFallback0/postCommitBreach0。droppedBacklogTicks分散364→352、混战331→324仍非零，不改33ms或max2。

raw报告均StoppedCleanly/harnessValidity=true/zeroGcGatePassed=true；suite明确zeroGcEvidenceStatus=UNCALIBRATED_COUNTER / UNKNOWN、zeroGcPassed=false。不能拿旧未校准counter的raw PASS覆盖27/28可靠采样的完整0GC FAIL；本轮不是H11收口/生产证书。

## 关闭、保护与执行边界

suite DONE/MEASUREMENTS_COMPLETED仅表示两个窗口完成。十一阶段关闭、remaining objects/slots/borrowers均0，两个run teardown restored/cleanupExceptions0；保留inactive pool容量1050不是active残留，不清理用户/缓存内容。原Menu restored/8roots clean；Menu与Battle SHA均与suite事前值一致。

post-run-audit-01.json：232非写域SHA无变化、11当前字节备份一致、HEAD8107196b保持，3code SHA与pre-run相同；validator PASS1327records/11 governed files，4248历史warnings，git diff --check无问题。共享30终态已事前备份，唯一授权runner输出变为31终态SHA B2D986749C4913790F0BB605E29E8E1A2AA637E9FA003582C22A70D2F9EE8CD1；共享request仍不存在。未删除/移动/reset/checkout/clean/stash/push。

实际命令族：原6401 manage_scene/get_editor_state；run_tests与get_test_job；单次execute_menu_item；Get-Content读取冻结request/报告、Get-FileHash比较原JSON/Scene/保护、git diff/status只读；Tools/Validate-ChangeLedger.ps1。原件在本目录，after.json和final-audit-01.json作为最终文档交付后审计回链。

## 收口与后继

本子批EVALUATION_COMPLETED / PERFORMANCE_INCONCLUSIVE，Change RUNTIME_PENDING是整体准入/正式性能与权威证据待，而非否认两个窗口实际已跑。六项有限交付仍5/6、34父项关闭0、H07/H11/Goal开放。复用现有collector/CharacterInput/LateEntityUpdate/表现统计继续最小实际热点处理，不重复已闭合的正例、同构观察或仅修测试；默认collector切换、EXT1、Mono/ATLAS及新资源格式仍需原授权门。没有同意改变模拟cadence或用降低AI工作量伪造收益。

