# 第85批输入四边界归因

状态：VERIFIED / ATTRIBUTION_COMPLETE / DIAGNOSTIC_ONLY / NO_DEFAULT_PROMOTION。仅本批四边界归因闭合，H07/H11 OPEN，有限阶段4/6，Goal active；本批不是优化收益或1000AI达标证书。

## 范围与资格

只在四个既有调用外围追加默认OFF计时：ProducerFreeze、ProducerSnapshotRefresh、ProxyApply、NativeSampledState；旧phase0—29稳定，新30—33，Count34。复用现有记录器和报告，新增数组冷容量payload128B不是全内存测量；业务调用顺序/条件/字段不变。冷Suite仅拥有诊断flag，恢复原值并解除World引用，未新增生命周期资源。准确四C#与禁止项见[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH85-INPUT-BOUNDARY-TIMING-20261009.md)、[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H07-INPUT-BOUNDARY-TIMING-085.md)、[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH85-INPUT-BOUNDARY-TIMING-20261009/RECORD.md)。

新测试实际RED32/32；GREEN1 30/32，两扩展checksum夹具错误修正而不弱化断言；GREEN2实际86/86（Core49含新12、旧Role owner19、旧PostBindings18）。合并两GREEN覆盖新32全通过，不称单次32全绿。首个空job0用例及所有失败XML保留。两输入域同态用例不是正式EXE全tick准入。

原Editor6402/Unity2022.3.62f3/唯一Menu clean8、非Play非compile无测试，启动前无CS error。2026-10-09T04:55:49+08源冻结；770保护项、10预期不存在、漂移0；11当前备份SHA保持，staged空、HEAD a10c358，diff-check0。四源准确SHA见[pre-window-01](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH85-INPUT-BOUNDARY-TIMING-20261009/pre-window-01.json)。不读取Q06活跃排序方法体，不改生产默认或资源/Scene。

## 唯一四窗口

通过当前已安装execute_menu_item(menu_path)启动唯一 `NTSD/Optimization/Run Batch85 Input Boundary Timing Windows`。两布局各OFF/ON，均实际1000 Sakura AI、相同既有seed/spawn/minimalAI/profile、共享绑定、120warmup＋180sample、collector=role、Role branch timing与84guard关闭。四窗均StoppedCleanly/workloadValid；Suite DONE / MEASUREMENTS_COMPLETED、4/4，2026-10-09T04:59:33+08终态。不能把ON计时税称收益，也不能用末态相同代替所有逐tick或native/Android/120FPS准入。

| 窗口 | logic mean / P95 ms | dropped（全run） | 新四phase call数（各项） | SetPass / 中央CPU draw | 有效性/恢复 |
|---|---|---|---|---|---|
| OFF Dispersed1000 | 42.478598 / 47.443065 | 397 | 0 | 5 / 2 | PASS / restored |
| ON Dispersed1000 | 42.504037 / 47.370805 | 257 | 180000 | 5 / 2 | PASS / restored |
| OFF Combat1000 | 42.985574 / 51.347850 | 258 | 0 | 5 / 2 | PASS / restored |
| ON Combat1000 | 43.124067 / 51.436010 | 261 | 180000 | 5 / 2 | PASS / restored |

每窗300000 AI entity-ticks包含120warmup＋180sample，ON四边界均180000调用、180 timing samples；OFF四调用/时间均0，覆盖/开关稳定/恢复全部通过。dropped是既有报告的全run字段，不能误称steady-only；OFF first窗397含启动过渡，不豁免，H07仍未达门。没有重复采样以追求更好数字。

| 四边界（ON，ms/tick均值） | Dispersed1000 | Combat1000 |
|---|---|---|
| ProducerFreeze | 0.231867 | 0.230008 |
| ProducerSnapshotRefresh | 0.952143 | 0.949964 |
| ProxyApply | 0.043049 | 0.042878 |
| NativeSampledState | 2.246682 | 2.250134 |

四者是顺序独立边界，合计3.473741/3.472985ms（仍含诊断税）；不能与它们内部其它嵌套phase再次相加。最贵的是这四段中的NativeSampledState（约2.25ms），其次ProducerSnapshotRefresh（约0.95ms）。它们不是已证可删除工作或纯数学kernel；后继应先核对原生input/动作路由及refresh的实际读写/已存在快路径，明确安全候选再建准确Task，不凭毫秒数跳到Burst/降频/新的Grid。无需再采本次同源窗口。

最新OFF总体CandidateCollect约14.150744/14.735133ms，CharacterInput约10.665070/10.568163ms，LateEntityUpdate约5.377514/5.363714ms；因此空间粗筛后的残余精确判定仍是整体大头，不能把所有CharacterInput叫寻敌，也不能称本批已解决千人耗时。当前生产冷默认仍未推广Role，与本包独立；81空间收益/82有限资格复用，不重开H06评估。

## 正确性、0GC及关闭边界

两对OFF/ON末tick300 Extended checksum JSON逐字节相同，报告20个Parity/Lockstep hash全部相同；分散SHA F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA，集中SHA E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。完整JSON/各hash保留在各report及.final-checksum.json。新两聚焦parity逐输入tick通过不等于这四窗逐tick全逻辑/native对照。

四窗同步Driver.StepOneTick recorder前/后positive＋empty校准都PASS，300 begun/ended/accepted、180steady/120nonsteady，各steady event0/证明分配0B/sequence error0/invalid0/rejected0，coverage/zeroAllocationPassed均true。校准knownLive1MiB只是正对照，raw值100/200并非字节；不把全帧wrapper GC或H11完整camera路径偷换为logic scope。H11第43批旧自然12event FAIL未因本批消去。

四窗完整base roster/突变/pool扩容采样拒绝均0；每窗1000 activeGO/2000 World objects/1000实体与slot正常回收归0，borrowers0/cleanup exception0。Suite orderedShutdown=true、remainingObjects/Slots/Borrowers三0、原Menu/Battle Scene SHA保持并恢复唯一Menu clean8 idle。第一窗inactive缓存容量10→1050只留存信息，不裁剪/删除，也不称活跃残留。正常owner退出运行已验；外部Play退出接线仅源码＋幂等恢复聚焦，未新增外部退出运行实验，不扩大验收。

## 执行与已知失败

- afe7e3facade4b7bbba83d0bde807d56：编译准备错误后的0实际用例，非有效RED/通过；empty-01/tests.xml SHA D00E9EEC0B04CFD5AC5A3B70B6839A71350BB864E93BA81EBA92C7CE72129BD7。
- 8647fb95aeb049559b94407a180edbe1：32实际RED/32FAIL，API/schema不存在；red-01/tests.xml SHA A6068ED3690B231936960B5FD612FEBCC043FB356CC8F64A596E244FBE30EF98。
- 7498af24669d4c739cd47e2b5f6ad7cc：32实际/30PASS/2FAIL，两个World夹具不支持Extended checksum；green-01/tests.xml SHA 634DBB5DFA168566827A591AC67C53C1E81290FFC6407DBC54F94EB788EC54E5。
- a12b5f31a8534547b7bfa2865b6ddfcb：86实际/86PASS/0FAIL；green-02/tests.xml SHA B29BE7D8A9481AF738F5DA194089AD195C6F890FF70E6FE72295529CBB0EA4A3。

上述XML均位于Operation同批独立目录；无删除/移动/第二Editor/Scene保存/Profiler或GPU capture。Validator实际exit0/PASSED、1380 records/14 governed diff、4191历史warning/0ERROR（正确捕获所有输出流）；早先仅捕获标准输出所得空数组/warning0是采集错误，不是validator无warning。最终源/guard/输出SHA见Operation after-01.json。阶段4/6、64执行、34父关闭0；不因诊断包收口标Goal完成。
