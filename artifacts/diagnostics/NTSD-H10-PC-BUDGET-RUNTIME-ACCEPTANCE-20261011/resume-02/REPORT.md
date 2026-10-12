# H10 PC预算运行验收 resume-02

Task `NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011` / Change `NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001` / Operation `OP-NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-RESUME02-20261011`。

**本版raw cue身份修正通过，两轮真实音频热负载的42个有效scope均为0分配事件；唯一Play整体仍FAIL，在第二轮generic播放观察处停止。** 第一轮完成正常准备/播放/关闭/重入，第二轮完成准备和全部热scope；第三轮、第二轮完整播放门、pending取消/迟到及自然Destroy没有完成。状态为`SCOPED_HOT_0EVENTS_PASS_SECOND_CYCLE_GENERIC_PLAY_OBSERVATION_FAILED`，不能声明RUNTIME_AUDIO_GATES_VERIFIED，H10父项继续OPEN。

## 本轮改动与授权边界

独立审阅后按人类继续H10与已批准PC预算授权恢复resume-02，先登记[PLAN.md](PLAN.md)及11份真实dirty备份。唯一脚本H10ResidentClosureDiagnostics.cs限四点：真实raw ID `@"data\016.wav"`；只读builtin Ordinal成员资格；cold wrapper/key/clips/cache身份先记后断言；关闭证据字段改为pendingAtPublicShutdownReturn并说明整个公开关闭返回时点。输出/session key使用resume-02。

旧非PcBudget前缀、三个recorder及资格算法、sink/emitter、ExerciseHot均按原始字节保持。本轮未改生产、recorder本体、Scene/资源/Settings/Plugins；不注册alias、不换root或补文件，不改33ms/输入RNG/pass/有序关闭。无新缓存/owner，全是原fixture冷证据；无新Q06正文、子代理、Goal或其他优化。

## 实际执行与门结果

| 门 | 本轮结果 | 原件 |
|---|---|---|
| 原项目编译 | 首次PASS；同PID39108，Unity2022.3.62f3，当前Menu clean8 | compile-request/state/console-01.json |
| 两项无解码资格 | 2/2 PASS；计量0.1009171s，现场/raw-key0.2178549s | [qualification-tests.xml](qualification-tests.xml)、[measurement-qualification.json](measurement-qualification.json) |
| 精确raw ID资格 | Ordinal builtin成员true，文件存在true；磁盘路径与cache key分列 | [raw-key-qualification.json](raw-key-qualification.json) |
| 两轮cold wrapper身份 | 两轮raw key存在、wrapper/public数组同引用、cache值精确匹配；generic ID -1261106复用 | cold-wrapper-cycle-1/2.json；cold-identity-cycle-1/2.json |
| 三个真实热范围 | 两轮各7个测量tick×3范围，共42份有效scope、0分配事件 | [hot-scope-audit.json](hot-scope-audit.json)、[hot-cycle-1.json](hot-cycle-1.json)、[hot-cycle-2.json](hot-cycle-2.json) |
| 第一轮播放与正常关闭重入 | PASS到隔离诊断范围；formal及generic在播，非零输出，公开关闭/drain/后帧owned归零 | [cycle-1.json](cycle-1.json)、[play-result.json](play-result.json) |
| 唯一Play整体 | 0/1 FAIL，24.919952s；第二轮generic观察布尔断言失败 | [unique-play-tests.xml](unique-play-tests.xml) |
| 第三轮、pending取消/迟到、自然Destroy门 | NOT_REACHED | 无通过原件，不补称完成 |
| Player/真实1000实体/完整Battle地图/整进程2/3GiB/目标PC | 未执行 | 原Player只读提案保持 |

原Editor每次运行前确认idle/nonPlay/noncompile/update/nontest及clean场景。EditMode测试显式Enter/ExitPlayMode，不使用会改domainreload选项的PlayMode工具路径，不保存Scene。bridge job跨domainreload后stale running不作判定；实际NUnit XML、失败/清理/恢复文件已完成，Editor回到idle/nonPlay/tests false。没有进程启动/停止或Editor重启。

## 42个scope的实际覆盖

每轮3warm+7measured tick，涵盖1请求、1000同cue、1000多cue（5个formal代表）、同帧publication重放和连续tick。scope为World.QueueSound循环、prepared NTSDSoundPlayer.PresentSounds、publication+dispatch+player；外层采样时sink内层关闭，无嵌套Reset。

42份均有前后1MiB正控事件响应和empty0，scope valid/calibratedBefore=true、未wrapped/未saturated、事件0，容量256、owner/current线程1。实际Marker=GC.Alloc、Category=Internal、Unit=TimeNanoseconds；rawValue按原单位保存，不解释成分配字节或GC回收停顿，正控某次raw0而events1也保留。测量前后load/build/failure/rejection增量0，实际播放计数增量符合1或5个cue聚合预期。总42是样本份数，不将重叠调用的范围耗时相加。

这是真实包围范围的0分配事件证据，区别于资格空scope。仍不是全Driver/engine、真实千人实体、CPU收益、正式120warm+180/1800性能证书或完整三轮验收。

## 第二轮首差与未知项

两轮public prepare各记录22个owned clip、去重8,431,640B，generation2→6，generic始终ID -1261106，精确wrapper与cache引用通过。第一轮formal/generic播放观察均通过，listener输出峰值1.0为非零观察；人工听感false，不评价音质。

第二轮热测完成且owned身份保持，随后formal voice的clip/enabled/active/isPlaying断言通过（控制流证据）；在PlaySfx(generic)后，PcBudgetFixture.RunThreeSessions第4125行`genericVoice != null && genericVoice.isPlaying`失败。第二轮波形与play-evidence行尚未写出，不补造其值。

只读旧helper FindVoiceWithClip:1248–1260确认它返回首个同clip AudioSource，不筛选isPlaying。generic按合同跨轮复用，故“停播旧voice被先选中、另有voice在播”是可解释风险；**失败原件没有全部voice状态，尚不能证明该原因，也不能证明生产未播放。** 当前只确定观察断言FAIL。详[NEXT-REVIEW.md](NEXT-REVIEW.md)，后继需先审精确观察记录，不能改生产或降低门消除失败。本轮没有新增诊断/第二次Play，源码保持受测版a833e450…9efe5b。

## 关闭与保护

第一轮正常关闭及第二轮失败清理均走公开ShutdownBattleRuntime，返回stage10、World解绑、remaining objects/slots/pool borrowers0；每次返回时22个owned native仍live，pending0/tail drained。空地图真实App.BattleBootstrap guard通过后stage11，下一帧owned归零。第一轮另证实关闭不误停当时generic借用voice。第二轮失败清理不替代第二轮借用播放门。此处没有真实Battle地图。

最终精确移除本次新增`NTSD.AudioFile::data\016.wav`和clip -1261106；自有host全部销毁，Play内clip初始/结束集合均空，cache/loader/singleton恢复。config/MM settings及Menu稳定组件快照前后一致，Menu clean8/setup/active恢复。原Menu四个asset clip不以跨reload instanceID稳定来证明历史恢复；旧Menu UNKNOWN/事故范围保持。详[fixture-cleanup.json](fixture-cleanup.json)、[scene-restored.json](scene-restored.json)。

原1682输入指纹仍匹配，旧全目录去重228918567B（218.313758MiB）继续复用通过320MiB对象门，无重解码全目录。PC与320MiB/2GiB/3GiB预算已批准，非认证硬件；Process.PrivateMemorySize64旧0继续不可用，本轮不评价进程预算。既有容量负控、旧PCM/R1–R4/native binder及其它未受影响证据不重跑。

[protection-audit.json](protection-audit.json)核4094保护、9源/DLL/recorder、196份原根与resume-01文件无漂移，11dirty备份有效；旧方法/recorder/ExerciseHot保持，HEAD/stage不变。差异见[resume-only-source.diff](resume-only-source.diff)。最终validator与范围检查见[final-audit.json](final-audit.json)、[change-ledger.log](change-ledger.log)。

## 实际命令与采集更正

完整参数/时间/响应见[commands.jsonl](commands.jsonl)。实际执行：

```text
get_editor_state {}
manage_scene {"action":"get_loaded_scenes"}
refresh_unity {"mode":"if_dirty","scope":"scripts","compile":"request","wait_for_ready":false}
read_console {"action":"get","types":["error"],"count":10,"format":"detailed"}
run_tests EditMode [PcBudgetAllocationMeasurementQualification, PcBudgetRuntimeQualification]
run_tests EditMode [PcBudgetRuntimeThreeSessionsPlay]
get_test_job {job_id: 结果文件中的具名ID, include_details: true, include_failed_tests: true}
git status --short --branch
git rev-parse HEAD
git diff --cached --binary
git diff --binary
pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity
```

另有Python备份/哈希/XML/JSON复算和只读CIM。一次CIM进程查询误选CommandLine，输出中带入启动凭据字段；发现后立即从本地新建editor-process.json移除整个CommandLine，只保留PID/创建时间。未使用该凭据，也未复制含凭据的原始备份；操作记录仅存修改前SHA及脱敏原因。已出现的工具输出不能由本地文件改写撤回。以后进程证据只选择PID/创建时间，不采完整启动参数。

## 当前下一步

交回第二轮generic voice观察的精确独立审阅，不自动写生产、改旧helper或增加运行额度。H10父项OPEN，三轮/pending/natural/真实场景与进程门仍缺。H08ON/M03OFF/H07producer暂缓/95OFF/H06不续、旧两次夹具失败及13/14/尖峰/事故均保留；不称Windows H07或Android认证通过。

## 最终治理校验

原validator在现有PowerShell7执行 **PASS，exit0**；1422条Record、1个受管脚本改动被当前Change覆盖，本Change无warning。4531条其他历史Record声明路径不在当前diff的提示保留，未改validator/Git配置。实际原件[validator-result.json](validator-result.json)和[change-ledger.log](change-ledger.log)。此次只补记结果，不改已检查metadata或源码、不增加测试。
