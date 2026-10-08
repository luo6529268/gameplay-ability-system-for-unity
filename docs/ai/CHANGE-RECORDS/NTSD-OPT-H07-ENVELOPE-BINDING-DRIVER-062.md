<!-- CHANGE-RECORD
id: NTSD-OPT-H07-ENVELOPE-BINDING-DRIVER-062
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs
authority: user approved continuing incomplete H07; effective goal Task sections0-8 and Batch62 existing envelope-binding interaction scope; formal336 unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH62-ENVELOPE-BINDING-DRIVER-20261008/REPORT.md
-->
# 第62批：已有包络＋拒绝绑定复用组合 Driver 资格

最终核验UTC2026-10-08T00:16:50.6867272Z：pwsh validator exit0/PASSED、1358records/20governed/4252历史warnings/0errors；git diff --check exit0/26CRLF提示/无其它错误。104guards和八accurate backup/HEAD核同，source46288B/DCD8031ADC0E9B5094F0C900431AFD39604F603D07366693F1EEB3E573AF565B与实际GREEN版本相同，公共XML2658642E7F5A09A943AA513AECC3AF43189C782BD8A51571AD7813F59C974A9B同。34unique父项、六项各3表行，M13旧三行保持；41已执行/阶段4of6/H07-H11 OPEN/Goal active，63只READY。没有额外测试或实景/生产推广。

最终资格 FOCUSED_TEST_PASS / COMBINATION_DRIVER_PASS / NOT_ADMITTED：job e9049489f13b404bb21f7ff8e79368f8 succeeded、8/8/0skip/494.8178684s，原XML7193881B/SHA2658642E7F5A09A943AA513AECC3AF43189C782BD8A51571AD7813F59C974A9B已freshcopy。四JSON88paired/176完整Driver，原声明六字段同/候选actual/noFallback；两个千人probe32000各/reuse10928860及9760678，root13/11与3/3。新组合执行资格通过，不是新FPS/0GC/bytes或全native证书；production OFF、61UNKNOWN/60FAIL/H11旧FAIL保持，41已执行/阶段4of6/Goal active。Task63新组合收益对照仅READY，不计已执行、无Change或作业；源未在运行中修改。

00:09:17Z Dispersed1000固定32paired结果已CreateNew保存，SHA6E4EC19FF0C4BF16171C9F55B4BB67969D583593CE30BF9A4CB6F89C0B258F06；baseline envelope32/ON applied32、candidate first probes合计32000/reuses10928860，非每帧max或性能收益。右/左root12paired各candidate probe13/11、reuse3/3，完整矩阵仍待Combat1000与原终态XML。计数覆盖该collection拒绝路径，并非只envelope拒绝分支的排他归因。

GREEN_RUNNING：原Editor job e9049489f13b404bb21f7ff8e79368f8，源DCD8031ADC0E9B5094F0C900431AFD39604F603D07366693F1EEB3E573AF565B，源码import/reload后freshidle才启动新5＋旧3。00:06:38Z两root JSON已完成，三case进度无failure，千人仍待；多个get_test_job TCP观察超时后继续同handle，没有restart/rerun。旧方法27仅Run/RunCoarseEnvelope按声明适配，其余25正文原文同，自有diff123增2删。

保护/审查：104guards与八accurate backup核同，源/生产默认未另改。validator以pwsh -NoProfile -File运行实际exit0/PASSED/1358records/20governed/4252历史WARNING/0ERROR；警告未逐条复审。之前直接调用以2>&1不能捕获Write-Host导致输出截断/JSON解析失败，且*>&1调用未获完整JSON；powershell(Windows5)子进程exit1未获有效验证，此后改项目当前pwsh执行才可靠。先前派生0warning不是有效计数，不据其做验收。历史diff-check exit0/26CRLF提示保持，最终另核。没有新0GC/FPS或预算通过，61未知与60FAIL不撤回。

PLANNED / COMBINATION_NOT_QUALIFIED / NOT_ADMITTED。先登记，尚未改C#或运行本批测试。

CODE_WRITTEN：新增RunEnvelopeBinding/AssertEnvelopeBindingApplication，Run只增可选rejectedBindingReuse并在原envelope默认断言之后设置，新元数据记录应用/first-probe/reuse。旧null调用行为与所有断言保持；原反射RunCoarseEnvelope补末尾null仅适配新增可选参数。没有Query/runtime/default变化。固定两root各12、两千人各32，88paired/176ticks，绿色结果尚待；没有新FPS/0GC/预算通过。

RED_CONFIRMED：原Editor job8a0b6d7eb31d4cd89b1e071448787700 terminalfailed，实际XML五new case全部因RunEnvelopeBinding缺入口失败，0skip；树9794不是执行数。XML已freshcopy 11673B/SHAD851DC17111DE751D294011E0FD55CDFDF91680AD81BB8F2FE9F35250DFF2005，2026-10-08T00:01:19.8193590Z，允许后续原callback覆盖前提已满足。只缺新测试接入，无runtime失败或新FPS结论；不重跑此RED。

TESTS_WRITTEN：八dirty副本23:57:43Z核同之后，唯一C#新增固定四组合case和默认guard及反射调用；RunEnvelopeBinding尚不存在，预期新5有效缺入口RED。没有改原Run/TickRow/旧方法、运行时或生产默认。事前治理前缀copy顺序失败及修正见Operation，C#准确before未变。
依据：用户要求阶段未达继续，而非次数停止；60 collector43.172/45.820ms仍超33，48已有拒绝绑定复用的局部信号并无组合Driver资格，61失败API不重测。

准确唯一代码路径：Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs。
符号：新增 EnvelopeBindingQualificationDoesNotPromoteDefaults、EnvelopeBindingPreservesApplicableFormalRootAndEveryTick、EnvelopeBindingThousandCanonicalAiPreservesEveryTick、InvokeEnvelopeBindingRun、RunEnvelopeBinding、AssertEnvelopeBindingApplication；
Run新增可选rejectedBindingReuse，在原coarseEnvelope/default断言之后设置；TickRow增加绑定应用/probe/reuse元数据；既有RunCoarseEnvelope反射实参补末尾null以保留旧行为。不削弱任何旧断言。
先新5case反射缺helper的有效RED，再实现helper；GREEN只新5＋旧OrdinaryBruteDefaults1/ProductionRequest2共8，0skip预期。既有59单候选/48cost/60四窗/61owner-counter/full历史不重跑。

冻结：两Formal root fixture各12tick，Dispersed1000/Combat1000各32tick；同seed/input/roster，每tick比较原声明extended/lockstep hashes、两RNG计数和实体数，共88paired/176fullDriver。原root SHA及实际正式336 SHA需一致，适用oid/action/hp/vx原字段保持。双方四admitted机制true、ForceBruteForce/noWorker、envelope ON，只有rejected-binding OFF/ON；kind5/eligibility/coarseDispatch/timingfalse。千人ON需实际first probe和reuse、包络direction/reject，否则INVALID，不制造收益。

readonly原Query：2608-2617 envelope拒绝保持PreserveBruteRejectedBinding；6310-6335首次原资格门读/按targetOrdinal+entity同collection复用；6338-6355资格门及accepted HasVrest保持，值每build重置。Query/Buffer/Suite/Harness/observer完全不写。复用48 stale-binding focused资格，指纹检查；本批不是全native证书。
没有新增Runtime模块/owner/容量/数组或关闭阶段，仍现有World fixture生命周期，十一阶段/workerJoin不重排。61 stride/byte预算UNKNOWN保留，本批不测0GC/FPS/Android/正式1800/中央GPU。资格通过仅判断新组合窗口是否值得，禁止自动生产推广。

事前UTC 2026-10-07T23:52:35.5416842Z，HEAD 45bbed41c64e0599601fa0df4028072d9f83303a；八准确dirty副本/104readonly guards见Operation before manifest，含Scene/font、Q06只hash、正式EXE、Server和既有证据。原Editor PID19040/6402 freshidle/nonPlay/Menu单Scene clean8roots。项目Unity2022.3.62f3、既有本地MCP控制，不安装Pipeline/启动第二Editor。
源before 05547FD2284635CD28D3FE6028798AACB4B971CD7448505DBB33DD70BFC4343F / 39016B，HEAD不是dirty恢复来源。

验证：限定RED/GREEN原job和callback XML原件需保存；实际8case不是全树testcount；新四JSON CreateNew。运行结束104保护/八backup/HEAD核同，Tools/Validate-ChangeLedger.ps1、git diff --check，源与准确副本做自有diff；主总表仍34父/阶段4/6、H07/H11 OPEN，不把资格当帧率通过。
回滚只反向本批hunk或准确before副本且另获准确批准，无reset/restore/checkout/clean/stash/删除/move/push。Scene/资源/settings/Input/Gen/Plugins/Server不改，Q06方法体不读，EXT1/Mono/ATLAS门不解冻。
