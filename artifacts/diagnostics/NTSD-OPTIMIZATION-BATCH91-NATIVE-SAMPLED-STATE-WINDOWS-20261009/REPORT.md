# 第91批：NativeSampledState 千人八子步骤归因

结论：ATTRIBUTION_COMPLETE / DIAGNOSTIC_ONLY / NO_DEFAULT_PROMOTION。H-07 仍 PERFORMANCE_FAIL；六项首阶段5/6，70已执行子批、34父项关闭0，Goal active。四窗完成是本诊断阶段出口，不是总目标停止条件。

## 范围与执行

只有 BattleOptimizationWindowsAiSuiteEditor.cs 新增独立冷菜单/输出路由、SuiteState bool、复用85 owner及双reuse OFF门、10反射test-first；90运行时、World/AI/collector/规则/资源均未修改。原2022.3.62f3 PID74724 TCP6402；唯一菜单 NTSD/Optimization/Run Batch91 Native Sampled State Timing Windows。01:41:00Z新请求至01:45:43Z完成/恢复，600秒冷准备守卫未触发。四窗均120预热+180采样、1000实际AI/OID1 Sakura、seed1314149188、DataOrientedCanonical、显式Role；87producer与89frame reuse/83/84额外诊断OFF，未改变生产默认。

新请求由原85 factory产生，只改outputPath。配对workload/roster/config fingerprints相同；Packages两外部已更改依赖指纹沿90最新基线，旧88 CPU不是同期基线。不得将本轮计时ON更快称为性能收益：没有算法候选，顺序采集的OFF/ON有明显时间波动，无法由这四窗隔离observer成本。

## 原Editor测试

首次 job c241e85e90084d12b80d28404a958020 在重载前选中0项，保留selection-before-reload-01/tests.xml（AC7CD0CFAD8E1E98BAEA55313E4F5E4EC1297E408A124A6925581BA99C50CC65），不算PASS/RED。有效RED df3e72dcb7b7465797f94a4b181d79ae：10/10缺API/字段预期FAIL、0skip，red-01/tests.xml SHA3675B1036D3A3320C488AA0EFDA68D386288A9CB8D93569EB83026198A3ECC45。GREEN 1cbfae4d9d4c45cba320f7e077f03549：新10+原61具名owner/request/postbindings共71/71 PASS、0skip、2.9259872s，green-01/tests.xml SHA BE8683DD5599F1E58C1451BAB0C4F4B83255AEF7EFF9685E645B243697EAFD96。90原23测试源码保持，仅复用其限定证据，不重跑全历史。

## 实际四窗（毫秒）

| 窗口 | logic mean | logic P95 | 全run dropped backlog ticks | 全帧SetPass mean/max | 中央CPU submission draw mean/max |
|---|---:|---:|---:|---:|---:|
| 00-dispersed1000-native-off | 58.974448 | 112.537870 | 462 | 5/5 | 2/2 |
| 01-dispersed1000-native-on | 48.120733 | 60.104435 | 315 | 5/5 | 2/2 |
| 02-combat1000-native-off | 47.892279 | 63.032595 | 316 | 5/5 | 2/2 |
| 03-combat1000-native-on | 47.399534 | 57.298315 | 308 | 5/5 | 2/2 |

SetPass/central统计均89个接受帧；物理segment/chunk各1。这里中央CPU DrawMesh/submission计数、全帧ProfilerRecorder SetPass、GPU batch三者不等同；未启动Profiler UI、Frame Debugger/GPU capture、专项M0，不能给真实GPU batch结论。Time.unscaledDeltaTime采集150帧的平均可见帧时长185.009978/152.997277/154.592559/153.134674ms，不把logic tick耗时倒数当画面FPS；非Player/Android/120FPS认证。

## 八子步骤平均（ON，ms）

| 子步骤 | 分散1000 | 集中1000 |
|---|---:|---:|
| NativeSamplePreprocess | 0.455880 | 0.427777 |
| NativeComboRoute | 0.148180 | 0.137310 |
| NativeThreeButtonRoute | 0.126059 | 0.123811 |
| NativeDirectionRoute | 0.285173 | 0.270583 |
| NativeBuiltinsRoute | 1.088330 | 1.035558 |
| NativeLegacyProjection | 0.183278 | 0.171904 |
| NativeCanonicalAiSync | 0.373751 | 0.360146 |
| NativeInputStateSync | 0.072767 | 0.068828 |

两个ON：每个子步骤180000次调用、180个tick timing样本、均有非零时间；两个OFF八call/time全0；phase数组42完整。NativeSampledState inclusive父 mean3.131679/2.990395ms，不能与子相加。最贵子步骤NativeBuiltinsRoute约1.09/1.04ms，仅全logic mean约2.26%/2.18%；这是实测观测占比，不是可消除成本或可承诺收益。

更大同源热点：ON CandidateCollect mean15.734382/16.155797ms（其中PairExactLoop8.114959/8.357180、DirectBroadphase3.113146/3.329526ms），CharacterInput12.626828/12.087653ms、LateEntityUpdate5.945469/5.777792ms。RemainingAiDecision5.111138/4.942133ms也大于该单一Builtins子步骤。阶段父/子重叠，不可全部相加。不能仅靠约1ms子步骤解决全logic约48/47ms相对33ms的缺口。

## 正确性、0GC、关闭与证据边界

四run均workloadValid、实际AI/基础角色最低1000、Role实际3且配置无漂移/未观察mismatch、计时flag applied/unchanged/restored全部true。完整同步StepOneTick校准前后通过，300 scopes=120非稳态+180稳态，稳态180全部0allocation event/0proven bytes；invalid/rejected/sequenceErrors0，CALIBRATED_FULL_DRIVER_ZERO_EVENTS。不是1800 formal稳态或全部300tick0GC声明。四capacityPressure门通过、180观察tick拒绝/容量critical delta0、roster mutation/pool expansion拒绝0。

两配对末tick300完整final-checksum JSON逐字相同，各10hash、总20条hash比较；分散SHA F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA，集中SHA E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。只终态与诊断开关等价，不是本轮逐tick全state或正式EXE千人复核。

四terminal原件PASS且teardownRestored true，activeGO1000→0/World2000→0/entities1000→0/claimed1000→0/cleanupExceptions0；最终orderedShutdown true、objects/slots/borrowers0。Menu/Battle dirty false/磁盘SHA相同，原Menu root8/idle非Play；没有Scene保存或第二Editor。四run中央89接受帧feature/material/immutable/category/submission/submitted-pixels全部就绪，source/resolved统计相同，无中央拒绝理由；这些不替代全帧stale/透明/GPU证明，报告未提供全帧stale计数，正式H07相关完整门仍待实际正式验收。

因为四窗P95>=33、dropped非0，本轮不启动1800 formal重复失败证书。下一从现有Role候选收集/精确循环的可消除重复工作选一个有证据动作；先检查旧失败覆盖，不能重试89帧引用无收益候选或为了获得更细报告继续堆无决策价值的子计时。不更换Grid/生产collector/Burst，不解冻EXT1/Mono/ATLAS或重开已过五项。

## 文件/治理审计

SuiteEditor SHA 6B01A2B69E84A74E09DCA4FB35AAB4CE7070622340494FA9A2FD79D3D13D52FE；8dirty前副本完整，新窗口原件CreateNew，无覆盖旧85/88/90证据。主审阅before差异仅新冷入口/10tests。正式336与playable闭包/Scene/Packages/90source均保护。最终validator/diffcheck/950范围外/8before/HEAD暂存审计与after文件清单见同IDaudit-final-01.json和after-manifest-01.json（待生成，不以占位宣称通过）。

工具缺口保留：第一次backup前误用manifest字段名导致读校验错误，未修改，改用sha核同后备份；脚本重载6402暂时拒连/closed header保留原job再取terminal；两次只读suite摘要PowerShell括号错误未产生写入，修正后完整重核。一次误读取单行完整snapshot导致工具输出截断，后用原文件SHA/JSON读取重新核对，不当作完整输出证据。


最终审计已完成：950范围外保护0drift、8dirty副本0drift、HEAD a10c358eeb956f2153e7d5bfbb60c3c1b353fd50/暂存空、shared request absent；git diff --check exit0，仅24已有CRLF warnings。Tools/Validate-ChangeLedger.ps1 exit0、1386records/17governed代码/4168历史WARN/0ERROR。原Editor idle/nonPlay/sole Menu clean root8，Scene磁盘SHA同；四八子合同显式重核通过、中央接受最后simulation/display均298且拒绝理由空（不是末300全GPU证明）。详audit-final-01.json；逐文件SHA在after-manifest-01.json新清单保存，不将本诊断VERIFIED升为H07完成。
