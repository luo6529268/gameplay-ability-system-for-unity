# H10 PC预算运行验收 resume-01

Task `NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011`；Change `NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001`；Operation `OP-NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-RESUME01-20261011`。

**计量资格2/2通过；唯一新Play组0/1失败，状态MEASUREMENT_QUALIFIED_FIXTURE_CUE_ID_MISMATCH。** 第一轮冷准备后，新增夹具使用的generic raw ID与实际预热注册key不一致，查询返回null。正式热测、三轮播放/关闭重入及pending取消/自然Destroy未执行到，不声称RUNTIME_AUDIO_GATES_VERIFIED。H10父项继续OPEN；这次是夹具错误，未证明生产音频存在分配缺陷。源码保持实际受测版本，不事后修成未执行版，不追加Play。

## 本轮授权与改动

独立审阅后的resume-01派发沿用人类继续H10及具体PC预算授权。只改H10ResidentClosureDiagnostics.cs原Task新增PcBudget*，原其他方法体保持；BattleScopedGcAllocationRecorder只读复用。11份当前dirty字节备份先于代码修改，准确符号/owner/容量/失效/finally及关闭位置见[PLAN.md](PLAN.md)。生产、资源、Scene、Settings、Plugins冻结；未读新Q06方法体，未委派、恢复Goal或其他队列。

三个recorder各自容量256、owner线程1；QueueSound与prepared PresentSounds在真实Step的子范围测量，外层publication+dispatch重放时关闭sink内层采样，避免嵌套Reset。校准/证据写入/await在热范围之外，End/Dispose/输出使用finally。sink样本128、每轮阶段行容量24对应最多20行；未增加生产缓存。

新资格及冷检查使用真实generic文件016，增加Menu稳定场景/资产引用的序列化组件快照、config/MM settings及native clip前后原件，并使用真实NTSD.App.BattleBootstrap空地图guard。计量资格修正有效；generic查询常量仍写错，见下文。

## 实际结果

| 门 | 本轮结果 | 原件 |
|---|---|---|
| 原Editor安全与编译 | PID39108，Unity2022.3.62f3；首次编译通过，idle、非Play/非编译更新/无其他测试、Menu clean8 | initial/before-compile/compile-state-01/compile-console-01 JSON |
| PcBudgetAllocationMeasurementQualification | PASS，0.0872158s；三个独立实例前后正控各1个事件，empty及目标空scope均0事件；inactive及uncalibrated拒绝认证 | [measurement-qualification.json](measurement-qualification.json) |
| PcBudgetRuntimeQualification | PASS，0.149237s；实际generic文件存在、cache空、loader idle、旧player无owned/prepared/活动声音，记录Menu组件与原4个asset AudioClip | [qualification.json](qualification.json)、[qualification-tests.xml](qualification-tests.xml) |
| 唯一resume Play | 0/1 Error，15.118494s；冷generic wrapper读取NullReferenceException，未到热测 | [unique-play-tests.xml](unique-play-tests.xml)、[play-result.json](play-result.json) |
| 热scope/实际播放/正常三轮/pending取消/自然Destroy | NOT_REACHED，不能认证 | [hot-scope-status.json](hot-scope-status.json) |
| 失败清理与当前Menu保护 | PASS到声明的隔离诊断范围，见下节 | [fixture-cleanup.json](fixture-cleanup.json)、[scene-restored.json](scene-restored.json) |
| Player/真实1000实体/整进程2/3GiB/目标PC | 未执行 | 保留原只读Player提案 |

计量原件Marker=GC.Alloc、Category=Internal、Unit=TimeNanoseconds、容量256，前后正控为1MiB live数组。rawValueTotal原值100–400按原Unit保存，不称分配字节、不称GC回收停顿。三份空scope有效、前后校准通过、未wrapped/未saturated；这是测量工具资格，不是实际生产热路径0GC证书。本轮没有hot-cycle文件，因为热方法尚未进入。

桥接job在Play domain reload后仍显示stale running；实际具名NUnit XML已失败，Editor idle/nonPlay/tests false、cleanup和恢复结果均完成。本报告以实际XML为准，没有重复轮询或修工具。没有启动/停止Editor进程，没有“Editor重启”的新事实。

## 首差与执行者夹具错误

执行者将`PcBudgetGenericCue`写成`"data/016.wav"`，而生产BuiltInBattleSoundIds实际为`@"data\016.wav"`。Windows文件存在检查通过，并不证明sealed preparedCues接受另一raw字符串。GetOrPrepareCue在sealed目录下先按原soundId查表，未命中直接返回null，尚未走后续磁盘路径规范化；generic注册也保留原soundId。

predeclared-generic-routes原件、当前只读生产链与异常位置互相对应。新增夹具还漏掉了读取Clips前先断言wrapper非null这一要求，导致错误呈现为NullReferenceException。这里不归责于生产加载/内存/分配实现，也不把文件存在当目录身份一致证明。精确后继仅需真实注册key、无解码key成员断言和冷wrapper断言，见[NEXT-REVIEW.md](NEXT-REVIEW.md)；未实施、未新增测试。

## 已观察的清理与恢复边界

public prepare已返回，formal封存、pending0、failed loads0、generation推进和nonmono引用检查均在异常之前执行。prepared摘要行位于错误之后而未写出；不补造本轮对象内存数字。

失败清理捕获22个owned native clip IDs。公开ShutdownBattleRuntime返回stage10，World解绑且remaining objects/slots/pool borrowers均0；此时22个native引用仍live，pending0/tail drained。真实App.BattleBootstrap和测试bootstrap均不存在，空诊断World完成stage11；下一帧22个owned全部消失，voice无旧clip。没有真实Battle地图，不扩写成地图卸载验收。变量pendingAtStage5Return实际是在整个公开关闭返回时读的值，不能称独立stage5时点证据。

最终仅移除本次准确缓存key `NTSD.AudioFile::data\016.wav` 和clip ID -1258522，自有hosts自然销毁后才释放。Play夹具内global clip基线与结束都是空集合；资格时Menu的4个asset clip属于另一场景/重载边界，不用instanceID冒称跨domain稳定。cache、loader、singleton绑定恢复；configStateEqual和mmSettingsStateEqual均true。Menu clean8、setup/active恢复，稳定场景/资产标识归一后的全部组件序列化快照前后相等。这仅证明本次保护，不覆盖旧Menu UNKNOWN或历史事故。

没有ClearCache、UnloadUnusedAssets、GC.Collect、全局Destroy或资源清理。人工听感false，isPlaying/波形门未执行，不把调用计数当声音输出。先前Unity Process.PrivateMemorySize64的0继续不可用，本轮没有借此评价预算。

## 证据复用、保护与命令

当前1682项catalog/DAT/音频输入与原指纹一致，320MiB对象门继续复用旧去重228918567B（218.313758MiB），未重跑完整目录、旧PCM/生命周期/R1–R4/native binder或容量负控。PC候选与320MiB/2GiB/3GiB预算已批准，不再UNDECIDED；对象门不覆盖整进程。

[protection-audit.json](protection-audit.json)记录4094条本轮保护指纹无漂移，9份源码/程序集/recorder在本次Play前后一致，11份真实dirty备份有效，旧非PcBudget方法体保持，105个旧根目录artifact哈希原样保留，HEAD/暂存不变。差异见[resume-only-source.diff](resume-only-source.diff)。最终ChangeLedger结果与文件范围核对见[final-audit.json](final-audit.json)和[change-ledger.log](change-ledger.log)。

实际入口（完整时间/参数见[commands.jsonl](commands.jsonl)）：

```text
get_editor_state {}
manage_scene {"action":"get_loaded_scenes"}
refresh_unity {"mode":"if_dirty","scope":"scripts","compile":"request","wait_for_ready":false}
read_console {"action":"get","types":["error"],"count":10,"format":"detailed"}
run_tests EditMode [PcBudgetAllocationMeasurementQualification, PcBudgetRuntimeQualification]
run_tests EditMode [PcBudgetRuntimeThreeSessionsPlay]
get_test_job {job_id: 具名结果文件中的ID, include_details: true, include_failed_tests: true}
git status --short --branch
git rev-parse HEAD
git diff --cached --binary
git diff --binary
pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity
```

另有内联Python备份/SHA/XML/JSON及只读进程CIM。首次离线摘要读取资格JSON误用系统GBK发生UnicodeDecodeError，尚未写XML副本；随后显式UTF-8读取成功，错误记录保留。这不是Unity资格失败或新测试，不影响实际2/2原件。

下一仍是同H10精确夹具key修正的独立审阅及尚缺门，不自动扩大写域或执行Player。H10父项、Windows H07与Android最终门保持OPEN；H08 ON/M03 OFF/H07 producer暂缓/95 OFF/H06不续及旧失败/事故均保留。
